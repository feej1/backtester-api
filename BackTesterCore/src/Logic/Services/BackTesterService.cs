
using Backtesting.Clients;
using Backtesting.Models;
using Microsoft.Extensions.Logging;

namespace Backtesting.Services
{


    public interface IBackTestingService
    {

        public Task<BackTestingResponse> BackTest(IBacktestSettings settings);
    }

    public class BacktestingService : IBackTestingService
    {
        private readonly IStockDataApiClient _apiClient;
        private readonly ILogger _logger;

        public BacktestingService(ILoggerFactory loggerFactory, IStockDataApiClient apiClient)
        {
            _logger = loggerFactory.CreateLogger<BacktestingService>();
            _apiClient = apiClient;
        }

        public async Task<BackTestingResponse> BackTest(IBacktestSettings settings)
        {

            switch (settings.Strategy)
            {
                case Strategies.MACD_CROSS:
                    {
                        return await HandleBacktest(settings);
                    }
                case Strategies.MOVING_AVERAGE_CROSS:
                    {
                        return await HandleMovingAverageCrossNew(settings);
                    }
                case Strategies.BUY_AND_HOLD:
                    {
                        return await HandleBuyAndHoldNew(settings);
                    }
                default:
                    throw new Exception("Strategy not implemented");
            }
        }

        private async Task<BackTestingResponse> HandleBacktest(IBacktestSettings settings)
        {

            if (!settings.AreValid())
            {
                _logger.LogError($"BacktesterService.HandleMacdBacktest(): Invalid Backtest Settings: {settings}");
                return null;
            }

            Console.WriteLine("Hello this started");

            // Holding asset
            TimeSeries assetToHoldTimeSeriesData = null;
            StockSplit assetToHoldStockSplitData = null;
            List<AlphaAdvantageDividendPayoutData> assetToHoldDividentPayoutData = null;
            IOrderedEnumerable<KeyValuePair<DateTime, TimeSeriesElement>> assetToHoldProcessedTimeSeriesData = null;

            // Trading asset
            TimeSeries assetToTradeTimeSeriesData = await settings.GetTradingAssetTimeSeries();
            StockSplit assetToTradeStockSplitData = await settings.GetTradingAssetStockSplits();
            List<AlphaAdvantageDividendPayoutData> assetToTradeDividendPayoutData = await settings.GetTradingAssetDividendPayouts();

            // Tracking asset
            TimeSeries assetToTrackTimeSeriesData = await settings.GetTrackingAssetTimeSeries();
            StockSplit assetToTrackStockSplitData = await settings.GetTrackingAssetStockSplits();
            if (settings.ShouldHoldAssetBetweenTrades())
            {
                assetToHoldTimeSeriesData = await settings.GetStaticHoldingAssetTimeSeries();
                assetToHoldStockSplitData = await settings.GetStaticHoldingAssetStockSplits();
                assetToHoldDividentPayoutData = await settings.GetStaticHoldingAssetDividendPayouts();
                assetToHoldTimeSeriesData.CalculateAdjustedClose(assetToHoldStockSplitData);
                assetToHoldProcessedTimeSeriesData = assetToHoldTimeSeriesData.Data.Where(x => x.Key >= settings.StartDate && x.Key <= settings.EndDate).OrderBy(x => x.Key);
            }

            assetToTrackTimeSeriesData.CalculateAdjustedClose(assetToTrackStockSplitData);
            var assetToTrackProcessedTimeSeriesData = assetToTrackTimeSeriesData.Data.Where(x => x.Key >= settings.StartDate && x.Key <= settings.EndDate).OrderBy(x => x.Key);

            IOrderedEnumerable<KeyValuePair<DateTime, TimeSeriesElement>> assetToTradeProcessedTimeSeriesData = null;
            if (settings.AssetToTrackTicker == settings.AssetToTradeTicker)
            {
                assetToTradeProcessedTimeSeriesData = assetToTrackProcessedTimeSeriesData;
            }
            else{
                assetToTradeTimeSeriesData.CalculateAdjustedClose(assetToTradeStockSplitData);
                var assetToTradeProcessedTimSeriesData = assetToTradeTimeSeriesData.Data.Where(x => x.Key >= settings.StartDate && x.Key <= settings.EndDate).OrderBy(x => x.Key);
            }

            var portfolio = new Portfolio();
            var backTestMetrics = new BacktestMetrics(
                assetToTrackProcessedTimeSeriesData.First().Key,
                assetToTrackProcessedTimeSeriesData.Last().Key,
                portfolio.GetBuyingPower());
            var tradingStrategyHandler = settings.GetTradingStrategyHandler();

            var assetToTrackItr = assetToTrackProcessedTimeSeriesData.GetEnumerator();
            var loop = assetToTrackItr.MoveNext();
            
            while (loop)
            {
                // update stats
                tradingStrategyHandler.UpdateIndicators(assetToTrackItr.Current.Value.AdjustedClose);
                if (tradingStrategyHandler.IsBuyConditionMet())
                {
                    assetToTradeProcessedTimeSeriesData!.ToDictionary().TryGetValue(assetToTrackItr.Current.Key, out var assetToTradeDataPoint); 
                    if (settings.ShouldHoldAssetBetweenTrades() && portfolio.OwnsStock(settings.StaticHoldingTicker))
                    {
                        assetToHoldProcessedTimeSeriesData!.ToDictionary().TryGetValue(assetToTrackItr.Current.Key, out var assetToHoldDataPoint); 
                        portfolio.LiquidateIfOwnsStock(settings.StaticHoldingTicker, assetToHoldDataPoint.AdjustedClose);
                        
                        var buyPrice = portfolio.GetPriceOfMostRecentStockPurchase();
                        var amountTraded = portfolio.GetNumberOfSharesFromLastSell();
                        backTestMetrics.UpdateTradeStatistics(buyPrice, assetToHoldDataPoint.AdjustedClose, amountTraded, portfolio.GetBuyingPower());
                    }
                    portfolio.BuyAsMuchStockAsPossible(settings.AssetToTradeTicker, assetToTradeDataPoint.AdjustedClose);
                }
                else if (tradingStrategyHandler.IsSellConditionMet())
                {
                    assetToTradeProcessedTimeSeriesData!.ToDictionary().TryGetValue(assetToTrackItr.Current.Key, out var assetToTradeDataPoint); 
                    portfolio.LiquidateIfOwnsStock(settings.AssetToTradeTicker, assetToTradeDataPoint.AdjustedClose);

                    var buyPrice = portfolio.GetPriceOfMostRecentStockPurchase();
                    var amountTraded = portfolio.GetNumberOfSharesFromLastSell();
                    backTestMetrics.UpdateTradeStatistics(buyPrice, assetToTradeDataPoint.AdjustedClose, amountTraded, portfolio.GetBuyingPower());
                    
                    if (settings.ShouldHoldAssetBetweenTrades())
                    {
                        assetToHoldProcessedTimeSeriesData!.ToDictionary().TryGetValue(assetToTrackItr.Current.Key, out var assetToHoldDataPoint); 
                        portfolio.BuyAsMuchStockAsPossible(settings.StaticHoldingTicker, assetToHoldDataPoint.AdjustedClose);
                    }

                }

                backTestMetrics.UpdatePercentTimeInvested(portfolio.OwnsAnyStock());

                loop = assetToTrackItr.MoveNext();

                // should sell all stock on final trading day
                if(!loop){
                    if (portfolio.OwnsStock(settings.AssetToTradeTicker))
                    {
                        assetToTradeProcessedTimeSeriesData!.ToDictionary().TryGetValue(assetToTrackItr.Current.Key, out var assetToTradeDataPoint); 
                        portfolio.LiquidateIfOwnsStock(settings.AssetToTradeTicker, assetToTradeDataPoint.AdjustedClose);
                        
                        var buyPrice = portfolio.GetPriceOfMostRecentStockPurchase();
                        var amountTraded = portfolio.GetNumberOfSharesFromLastSell();
                        backTestMetrics.UpdateTradeStatistics(buyPrice, assetToTradeDataPoint.AdjustedClose, amountTraded, portfolio.GetBuyingPower());
                    }
                    else if (settings.ShouldHoldAssetBetweenTrades() && portfolio.OwnsStock(settings.AssetToTrackTicker))
                    {
                        assetToHoldProcessedTimeSeriesData!.ToDictionary().TryGetValue(assetToTrackItr.Current.Key, out var assetToHoldDataPoint); 
                        portfolio.LiquidateIfOwnsStock(settings.StaticHoldingTicker, assetToHoldDataPoint.AdjustedClose);
                        
                        var buyPrice = portfolio.GetPriceOfMostRecentStockPurchase();
                        var amountTraded = portfolio.GetNumberOfSharesFromLastSell();
                        backTestMetrics.UpdateTradeStatistics(buyPrice, assetToHoldDataPoint.AdjustedClose, amountTraded, portfolio.GetBuyingPower());
                    }
                }
            }

            Console.WriteLine(backTestMetrics.ToString());
            return new BackTestingResponse()
            {
                Strategy = Strategies.MACD_CROSS,
                BacktestSettings = settings,
                BacktestStatistics = backTestMetrics
            };
        }

        private async Task<BackTestingResponse> HandleMovingAverageCrossNew(IBacktestSettings settings)
        {
            return new BackTestingResponse();
        }

        private async Task<BackTestingResponse> HandleBuyAndHoldNew(IBacktestSettings settings)
        {
            return new BackTestingResponse();
        }

        private async Task<BackTestingResponse> HandleMacdBacktest(
                string bullTkr,
                string bearTkr,
                int lossStopPercentage,
                DateTime startDate,
                DateTime endDate)
        {

            // get stock data
            var bullTimeSeries = (await _apiClient.GetTimeSeriesDaily(bullTkr)).ToTimeSeriesDataModel();
            // var bearTimeSeries = (await _apiClient.GetTimeSeriesDaily(bearTkr)).ToTimeSeriesDataModel().Data.Where(x => x.Key >= startDate && x.Key <= endDate).OrderBy(x => x.Key);
            var bearTimeSeries = (await _apiClient.GetTimeSeriesDaily(bearTkr)).ToTimeSeriesDataModel();


            var bullDividendPayout = (await _apiClient.GetDividendPayouts(bullTkr)).Data.Where(x => x.PaymentDate >= startDate && x.PaymentDate <= endDate).OrderBy(x => x.PaymentDate);
            var bearDividendPayout = (await _apiClient.GetDividendPayouts(bearTkr)).Data.Where(x => x.PaymentDate >= startDate && x.PaymentDate <= endDate).OrderBy(x => x.PaymentDate);

            var bullStockSplits = (await _apiClient.GetStockSplits(bullTkr)).ToStockSplitDataModel();
            // var bearStockSplits = (await _apiClient.GetStockSplits(bullTkr)).ToStockSplitDataModel().Data.Where(x => x.Key >= startDate && x.Key <= endDate);
            var bearStockSplits = (await _apiClient.GetStockSplits(bearTkr)).ToStockSplitDataModel();


            // set up portfolio
            var currTkr = "";
            double shares = 0;
            double cash = 1000;
            Dictionary<(DateTime date, string tkr), (Backtesting.Models.Action action, double amount, double price, double value)> history = new Dictionary<(DateTime date, string tkr), (Backtesting.Models.Action action, double amount, double price, double value)>();

            void BuyStock(string tkr, double price, DateTime date)
            {
                var val = cash;
                currTkr = tkr;
                shares = (cash - 1) / price;
                cash = cash - (price * shares);

                history.Add((date: date, tkr: tkr), (action: Models.Action.BUY, amount: shares, price: price, value: cash + (price * shares)));
            }
            void LiquidateStock(double price, DateTime date)
            {
                if (shares > 0)
                {
                    history.Add((date: date, tkr: currTkr), (action: Backtesting.Models.Action.SELL, amount: shares, price: price, value: cash + (price * shares)));
                    currTkr = "";
                    cash = cash + shares * price;
                    shares = 0;
                }
            }

            Macd macdIndicator = new Macd(12, 26, 9);
            var day12Ema = new List<double>();
            var day26Ema = new List<double>();
            var macd = new List<double>();
            var dates = new List<string>();
            var prices = new List<double>();
            var adjCloses = new List<double>();
            var signals = new List<double>();

            void updateStats(KeyValuePair<DateTime, TimeSeriesElement> current)
            {
                prices.Add(current.Value.Close);
                adjCloses.Add(current.Value.AdjustedClose);
                macd.Add(macdIndicator.GetCurrentMacd());
                day12Ema.Add(macdIndicator.shortPeriodEma.GetCurrentValue());
                day26Ema.Add(macdIndicator.longPeriodEma.GetCurrentValue());
                dates.Add(current.Key.ToString());
                signals.Add(macdIndicator.GetCurrentSignalValue());

            };

            bullTimeSeries.CalculateAdjustedClose(bullStockSplits);
            var bullTimeSeriesData = bullTimeSeries.Data.Where(x => x.Key >= startDate && x.Key <= endDate).OrderBy(x => x.Key);
            bearTimeSeries.CalculateAdjustedClose(bearStockSplits);
            var bearTimeSeriesData = bearTimeSeries.Data.Where(x => x.Key >= startDate && x.Key <= endDate).OrderBy(x => x.Key);

            var bullItr = bullTimeSeriesData.GetEnumerator();
            while (bullItr.MoveNext())
            {
                // update stats
                macdIndicator.UpdateMacd(bullItr.Current.Value.AdjustedClose);
                updateStats(bullItr.Current);
                var crossStatus = macdIndicator.GetCrossStatus();
                if (crossStatus == Macd.CrossStatus.MacdCrossedAboveSignal)
                {
                    bearTimeSeriesData.ToDictionary().TryGetValue(bullItr.Current.Key, out var bearDataPoint);
                    LiquidateStock(bearDataPoint.AdjustedClose, bullItr.Current.Key);
                    BuyStock(bullTkr, bullItr.Current.Value.AdjustedClose, bullItr.Current.Key);
                }
                else if (crossStatus == Macd.CrossStatus.MacdCrossedBelowSignal)
                {
                    bearTimeSeriesData.ToDictionary().TryGetValue(bullItr.Current.Key, out var bearDataPoint);
                    LiquidateStock(bullItr.Current.Value.AdjustedClose, bullItr.Current.Key);
                    BuyStock(bearTkr, bearDataPoint.AdjustedClose, bullItr.Current.Key);
                }
            }


            for (int i = 0; i < dates.Count; i++)
            {
                var date = dates.ElementAt(i);
                var day12 = day12Ema.ElementAt(i);
                var day26 = day26Ema.ElementAt(i);
                var macdVal = macd.ElementAt(i);
                var price = prices.ElementAt(i);
                var adjusted = adjCloses.ElementAt(i);
                var signal = signals.ElementAt(i);
                Console.WriteLine($"ASSET STATS -- date: {date}     price: {adjusted} signal: {signal} macd: {macdVal}");
                //Console.WriteLine($"date: {date}     price: {price}   adjusted close: {adjusted}");
            }

            history.AsEnumerable().OrderBy(x => x.Key).ToList().ForEach(x =>
            {
                Console.WriteLine($"HISTORY -- date: {x.Key.date}    action: {x.Value.action}  tkr: {x.Key.tkr} price: {x.Value.price} amount: {x.Value.amount} port vals: {x.Value.value}");
            });

            return new BackTestingResponse();
        }

        private async Task<BackTestingResponse> HandleMovingAverageCross(
            string tkr,
            int lossStopPercentage,
            DateTime startDate,
            DateTime endDate)
        {

            // get stock data
            var timeSeries = (await _apiClient.GetTimeSeriesDaily(tkr)).ToTimeSeriesDataModel();
            var dividendPayout = (await _apiClient.GetDividendPayouts(tkr)).Data.Where(x => x.PaymentDate >= startDate && x.PaymentDate <= endDate).OrderBy(x => x.PaymentDate);
            var stockSplits = (await _apiClient.GetStockSplits(tkr)).ToStockSplitDataModel();

            // set up portfolio
            var currTkr = "";
            double shares = 0;
            double cash = 1000;
            Dictionary<(DateTime date, string tkr), (Backtesting.Models.Action action, double amount, double price, double value)> history = new Dictionary<(DateTime date, string tkr), (Backtesting.Models.Action action, double amount, double price, double value)>();

            void BuyStock(string tkr, double price, DateTime date)
            {
                var val = cash;
                currTkr = tkr;
                shares = (cash - 1) / price;
                cash = cash - (price * shares);

                history.Add((date: date, tkr: tkr), (action: Backtesting.Models.Action.BUY, amount: shares, price: price, value: cash + (price * shares)));
            }
            void LiquidateStock(double price, DateTime date)
            {
                if (shares > 0)
                {
                    history.Add((date: date, tkr: currTkr), (action: Backtesting.Models.Action.SELL, amount: shares, price: price, value: cash + (price * shares)));
                    currTkr = "";
                    cash = cash + shares * price;
                    shares = 0;
                }
            }

            var leadingMovingAverage = new SimpleMovingAverage(10);
            var laggingMovingAverage = new SimpleMovingAverage(50);

            var dates = new List<DateTime>();
            var prices = new List<double>();
            var adjCloses = new List<double>();
            var movingAveragesLeading = new List<double>();
            var movingAveragesLagging = new List<double>();
            var values = new List<double>();

            void updateStats(KeyValuePair<DateTime, TimeSeriesElement> current)
            {
                prices.Add(current.Value.Close);
                adjCloses.Add(current.Value.AdjustedClose);
                dates.Add(current.Key);
                movingAveragesLeading.Add(leadingMovingAverage.GetCurrentValue());
                movingAveragesLagging.Add(laggingMovingAverage.GetCurrentValue());
                values.Add(cash + (shares * current.Value.AdjustedClose));
            };

            timeSeries.CalculateAdjustedClose(stockSplits);
            var timeSeriesData = timeSeries.Data.Where(x => x.Key >= startDate && x.Key <= endDate).OrderBy(x => x.Key);

            var itr = timeSeriesData.GetEnumerator();
            while (itr.MoveNext())
            {
                // update stats
                var currentLeadingMovingAverage = leadingMovingAverage.UpdateMovingAverage(itr.Current.Value.AdjustedClose);
                var currentLaggingMovingAverage = laggingMovingAverage.UpdateMovingAverage(itr.Current.Value.AdjustedClose);
                updateStats(itr.Current);

                var previousLeadingMovingAverage = leadingMovingAverage.GetPreviousValue();
                var previousLaggingMovingAverage = laggingMovingAverage.GetPreviousValue();

                if (laggingMovingAverage.IsAverageFilled() && previousLeadingMovingAverage <= previousLaggingMovingAverage && currentLeadingMovingAverage > currentLaggingMovingAverage)
                {
                    // buy signal
                    BuyStock(tkr, itr.Current.Value.AdjustedClose, itr.Current.Key);
                }
                else if (laggingMovingAverage.IsAverageFilled() && previousLeadingMovingAverage >= previousLaggingMovingAverage && currentLeadingMovingAverage < currentLaggingMovingAverage)
                {
                    LiquidateStock(itr.Current.Value.AdjustedClose, itr.Current.Key);
                }
            }

            for (int i = 0; i < dates.Count; i++)
            {
                var dateObj = dates.ElementAt(i);
                var date = dateObj.ToString();
                var offset = new DateTimeOffset(dateObj);
                var price = prices.ElementAt(i);
                var adjusted = adjCloses.ElementAt(i);
                var mvaLeading = movingAveragesLeading.ElementAt(i);
                var mvaLagging = movingAveragesLagging.ElementAt(i);
                var value = values.ElementAt(i);
                // Console.WriteLine($"ASSET STATS -- date: {date}     price: {adjusted} mvaLeading: {mvaLeading} mvaLagging: {mvaLagging}");
                // Console.WriteLine($"date: {date}     price: {price}   adjusted close: {adjusted}");
                Console.WriteLine($"[{offset.ToUnixTimeMilliseconds()},{value}],");
            }

            history.AsEnumerable().OrderBy(x => x.Key).ToList().ForEach(x =>
            {
                // Console.WriteLine($"HISTORY -- date: {x.Key.date}    action: {x.Value.action}  tkr: {x.Key.tkr} price: {x.Value.price} amount: {x.Value.amount} port vals: {x.Value.value}");
            });

            return new BackTestingResponse();
        }

        private async Task<BackTestingResponse> HandleByAndHold(
            string tkr,
            DateTime startDate,
            DateTime endDate)
        {

            // get stock data
            var timeSeries = (await _apiClient.GetTimeSeriesDaily(tkr)).ToTimeSeriesDataModel();
            var dividendPayout = (await _apiClient.GetDividendPayouts(tkr)).Data.Where(x => x.PaymentDate >= startDate && x.PaymentDate <= endDate).OrderBy(x => x.PaymentDate);
            var stockSplits = (await _apiClient.GetStockSplits(tkr)).ToStockSplitDataModel();

            // set up portfolio
            var currTkr = "";
            double shares = 0;
            double cash = 1000;
            Dictionary<(DateTime date, string tkr), (Backtesting.Models.Action action, double amount, double price, double value)> history = new Dictionary<(DateTime date, string tkr), (Backtesting.Models.Action action, double amount, double price, double value)>();

            void BuyStock(string tkr, double price, DateTime date)
            {
                var val = cash;
                currTkr = tkr;
                shares = (cash - 1) / price;
                cash = cash - (price * shares);

                history.Add((date: date, tkr: tkr), (action: Backtesting.Models.Action.BUY, amount: shares, price: price, value: cash + (price * shares)));
            }
            void LiquidateStock(double price, DateTime date)
            {
                if (shares > 0)
                {
                    history.Add((date: date, tkr: currTkr), (action: Backtesting.Models.Action.SELL, amount: shares, price: price, value: cash + (price * shares)));
                    currTkr = "";
                    cash = cash + shares * price;
                    shares = 0;
                }
            }

            var dates = new List<string>();
            var prices = new List<double>();
            var adjCloses = new List<double>();

            void updateStats(KeyValuePair<DateTime, TimeSeriesElement> current)
            {
                prices.Add(current.Value.Close);
                adjCloses.Add(current.Value.AdjustedClose);
                dates.Add(current.Key.ToString());
            };

            timeSeries.CalculateAdjustedClose(stockSplits);
            var timeSeriesData = timeSeries.Data.Where(x => x.Key >= startDate && x.Key <= endDate).OrderBy(x => x.Key);

            var start = timeSeriesData.First();
            BuyStock(tkr, start.Value.AdjustedClose, start.Key);

            var last = timeSeriesData.ElementAt(timeSeriesData.Count() - 1);
            LiquidateStock(last.Value.AdjustedClose, last.Key);

            for (int i = 0; i < dates.Count; i++)
            {
                var date = dates.ElementAt(i);
                var price = prices.ElementAt(i);
                var adjusted = adjCloses.ElementAt(i);
                Console.WriteLine($"ASSET STATS -- date: {date}     price: {adjusted}");
                //Console.WriteLine($"date: {date}     price: {price}   adjusted close: {adjusted}");
            }

            history.AsEnumerable().OrderBy(x => x.Key).ToList().ForEach(x =>
            {
                Console.WriteLine($"HISTORY -- date: {x.Key.date}    action: {x.Value.action}  tkr: {x.Key.tkr} price: {x.Value.price} amount: {x.Value.amount} port vals: {x.Value.value}");
            });

            return new BackTestingResponse();
        }

    }

    public class Macd
    {
        private double currentMacd = 0;

        private double previousMacd = 0;

        public ExponentialMovingAverage shortPeriodEma;

        public ExponentialMovingAverage longPeriodEma;

        private ExponentialMovingAverage singnalLineEma;

        public Macd(int shortPeriod, int longPeriod, int signalPeriod)
        {
            shortPeriodEma = new ExponentialMovingAverage(shortPeriod);
            longPeriodEma = new ExponentialMovingAverage(longPeriod);
            singnalLineEma = new ExponentialMovingAverage(signalPeriod);
        }

        public enum CrossStatus
        {
            NoCross,
            MacdCrossedAboveSignal,
            MacdCrossedBelowSignal
        }

        public void UpdateMacd(double newPrice)
        {
            previousMacd = currentMacd;
            (var prevShortEma, var currShortEma) = shortPeriodEma.UpdateEma(newPrice);
            (var prevLongEma, var currLongEma) = longPeriodEma.UpdateEma(newPrice);
            currentMacd = currShortEma - currLongEma;
            singnalLineEma.UpdateEma(currentMacd);
        }

        public CrossStatus GetCrossStatus()
        {
            if (previousMacd <= singnalLineEma.GetPreviousValue() && currentMacd > singnalLineEma.GetCurrentValue())
            {
                return CrossStatus.MacdCrossedAboveSignal;
            }
            else if (previousMacd >= singnalLineEma.GetPreviousValue() && currentMacd < singnalLineEma.GetCurrentValue())
            {
                return CrossStatus.MacdCrossedBelowSignal;
            }
            else return CrossStatus.NoCross;
        }

        public bool IsMAcdPositive() =>
            currentMacd > 0;

        public double GetCurrentMacd() =>
            currentMacd;

        public double GetPreviousMacd() =>
            previousMacd;

        public double GetCurrentSignalValue() =>
            singnalLineEma.GetCurrentValue();

        public double GetPreviousSignalLine() =>
            singnalLineEma.GetPreviousValue();

    }

    public class ExponentialMovingAverage
    {

        private double currentEma = 0;
        private double previousEma = 0;

        private readonly int emaPeriod;

        private readonly double K;

        public ExponentialMovingAverage(int period)
        {
            emaPeriod = period;
            K = (double)2 / (double)(period + 1);
        }

        public (double updatedPreviousEma, double updatedEma) UpdateEma(double currentValue)
        {

            previousEma = currentEma;

            if (currentEma == 0)
            {
                currentEma = currentValue;
                return (updatedPreviousEma: previousEma, updatedEma: currentEma);
            }

            currentEma = (currentValue * K) + (currentEma * (1 - K));
            return (updatedPreviousEma: previousEma, updatedEma: currentEma);
        }

        public double GetPreviousValue()
        {
            return previousEma;
        }

        public double GetCurrentValue()
        {
            return currentEma;
        }

    }

    public class SimpleMovingAverage
    {

        private readonly List<double> Values;
        private double CurrentMovingAverage;
        private double PreviousMovingAverage;
        private readonly int MovingAveragePeriod;

        public SimpleMovingAverage(int period)
        {
            MovingAveragePeriod = period;
            CurrentMovingAverage = 0;
            PreviousMovingAverage = 0;
            Values = new List<double>();
        }

        public double UpdateMovingAverage(double currentValue)
        {
            PreviousMovingAverage = CurrentMovingAverage;

            if (Values.Count < MovingAveragePeriod)
            {
                Values.Add(currentValue);
            }
            else
            {
                Values.Remove(Values.ElementAt(0));
                Values.Add(currentValue);
            }

            CurrentMovingAverage = Values.Aggregate((a, x) => a + x) / (double)MovingAveragePeriod;
            return CurrentMovingAverage;
        }

        public double GetPreviousValue() =>
            PreviousMovingAverage;

        public double GetCurrentValue() =>
            CurrentMovingAverage;

        public bool IsAverageFilled() =>
            Values.Count >= MovingAveragePeriod;

    }


}