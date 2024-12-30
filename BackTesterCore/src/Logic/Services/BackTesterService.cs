
using System.Drawing.Text;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using Backtesting.Clients;
using Backtesting.Models;
using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.FileSystemGlobbing.Internal.PathSegments;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;

namespace Backtesting.Services
{


    public interface IBackTestingService
    {

        public BackTestingResponse BackTest(Strategies strategy,
            string bullTkr,
            string bearTkr,
            int lossStopPercentage,
            DateTime startDate,
            DateTime endDate);
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

        public BackTestingResponse BackTest(Strategies strategy,
            string bullTkr,
            string bearTkr,
            int lossStopPercentage,
            DateTime startDate,
            DateTime endDate)
        {

            switch (strategy)
            {
                case Strategies.MACD_CROSS:
                    {
                        HandleMacdBacktest(bullTkr, bearTkr, lossStopPercentage, startDate, endDate);
                        break;
                    }
                case Strategies.MOVING_AVERAGE_CROSS:
                    {
                        HandleMovingAverageCross(bullTkr, lossStopPercentage, startDate, endDate);
                        break;
                    }
                case Strategies.BUY_AND_HOLD:
                    {
                        HandleByAndHold(bullTkr, startDate, endDate);
                        break;
                    }
                default:
                    throw new Exception("Strategy not implemented");
            }

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
            Dictionary<(DateTime date, string tkr), (Action action, double amount, double price, double value)> history = new Dictionary<(DateTime date, string tkr), (Action action, double amount, double price, double value)>();

            void BuyStock(string tkr, double price, DateTime date)
            {
                var val = cash;
                currTkr = tkr;
                shares = (cash - 1) / price;
                cash = cash - (price * shares);

                history.Add((date: date, tkr: tkr), (action: Action.BUY, amount: shares, price: price, value: cash + (price * shares)));
            }
            void LiquidateStock(double price, DateTime date)
            {
                if (shares > 0)
                {
                    history.Add((date: date, tkr: currTkr), (action: Action.SELL, amount: shares, price: price, value: cash + (price * shares)));
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
            Dictionary<(DateTime date, string tkr), (Action action, double amount, double price, double value)> history = new Dictionary<(DateTime date, string tkr), (Action action, double amount, double price, double value)>();

            void BuyStock(string tkr, double price, DateTime date)
            {
                var val = cash;
                currTkr = tkr;
                shares = (cash - 1) / price;
                cash = cash - (price * shares);

                history.Add((date: date, tkr: tkr), (action: Action.BUY, amount: shares, price: price, value: cash + (price * shares)));
            }
            void LiquidateStock(double price, DateTime date)
            {
                if (shares > 0)
                {
                    history.Add((date: date, tkr: currTkr), (action: Action.SELL, amount: shares, price: price, value: cash + (price * shares)));
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
                var offset  = new DateTimeOffset(dateObj);
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
            Dictionary<(DateTime date, string tkr), (Action action, double amount, double price, double value)> history = new Dictionary<(DateTime date, string tkr), (Action action, double amount, double price, double value)>();

            void BuyStock(string tkr, double price, DateTime date)
            {
                var val = cash;
                currTkr = tkr;
                shares = (cash - 1) / price;
                cash = cash - (price * shares);

                history.Add((date: date, tkr: tkr), (action: Action.BUY, amount: shares, price: price, value: cash + (price * shares)));
            }
            void LiquidateStock(double price, DateTime date)
            {
                if (shares > 0)
                {
                    history.Add((date: date, tkr: currTkr), (action: Action.SELL, amount: shares, price: price, value: cash + (price * shares)));
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

    public enum Action
    {
        BUY,
        SELL
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