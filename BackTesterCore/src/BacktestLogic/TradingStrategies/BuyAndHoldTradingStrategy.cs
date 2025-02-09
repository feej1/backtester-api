

using Backtesting.Models;

namespace Backtesting.Services
{

    public class BuyAndHoldTradingStrategy : ITradingStrategy
    {

        private BacktestMetrics BacktestMetrics;

        private Portfolio StrategyPortfolio;

        private IBacktestSettings Options;

        private IOrderedEnumerable<KeyValuePair<DateTime, TimeSeriesElement>> AssetToTradeProcessedTimeSeriesData;

        private IEnumerator<DateTime> TradingDaysIterator;

        private DateTime FinalTradingDay;

        private List<PortfolioValue> PortfolioValues;

        public BuyAndHoldTradingStrategy(BuyAndHoldSettings options)
        {
            StrategyPortfolio = new Portfolio();
            PortfolioValues = new List<PortfolioValue>();
            Options = options;
        }

        public List<PortfolioValue> GetPortfolioValues()
        {
            return PortfolioValues.OrderBy(e => e.Date).ToList();
        }

        public void UpdatePortfolioValues(bool isTradingOver = false)
        {
            var dateTime = isTradingOver ? FinalTradingDay : TradingDaysIterator.Current;
            var currentPortfolioValue = new PortfolioValue()
            {
                Date = TradingDaysIterator.Current.UnixTimestampFromDateTime()
            };

            if (StrategyPortfolio.OwnsAnyStock())
            {
                AssetToTradeProcessedTimeSeriesData!.ToDictionary().TryGetValue(dateTime,
                            out var assetToTradeDataPoint);
                var ownedStock = StrategyPortfolio.GetAmountOfStockOwned(Options.AssetToTradeTicker);
                currentPortfolioValue.Value = assetToTradeDataPoint.AdjustedClose * ownedStock;
            }

            currentPortfolioValue.Value = StrategyPortfolio.GetBuyingPower();
            PortfolioValues.Add(currentPortfolioValue);
        }


        public bool MoveNext()
        {
            // sell stock on last day
            if (!TradingDaysIterator.MoveNext())
            {
                AssetToTradeProcessedTimeSeriesData!.ToDictionary().TryGetValue(FinalTradingDay,
                    out var assetToTradeDataPoint);
                StrategyPortfolio.LiquidateStock(Options.AssetToTradeTicker, assetToTradeDataPoint.AdjustedClose);

                var buyPrice = StrategyPortfolio.GetPriceOfMostRecentStockPurchase();
                var amountTraded = StrategyPortfolio.GetNumberOfSharesFromLastSell();
                BacktestMetrics.UpdateTradeStatistics(buyPrice, assetToTradeDataPoint.AdjustedClose, amountTraded, StrategyPortfolio.GetBuyingPower());

                UpdatePortfolioValues(true);

                return false;
            }

            // buy stock on first day
            if (!StrategyPortfolio.OwnsAnyStock())
            {
                AssetToTradeProcessedTimeSeriesData!.ToDictionary().TryGetValue(TradingDaysIterator.Current,
                    out var assetToTradeDataPoint);
                StrategyPortfolio.BuyAsMuchStockAsPossible(Options.AssetToTradeTicker, assetToTradeDataPoint.AdjustedClose);
            }

            BacktestMetrics.UpdatePercentTimeInvested(true);
            UpdatePortfolioValues();

            return true;
        }

        public BacktestMetrics GetStatistics()
        {
            return BacktestMetrics;
        }

        public async Task RetreiveData()
        {
            TimeSeries assetToTradeTimeSeriesData = await Options.GetTradingAssetTimeSeries();
            StockSplit assetToTradeStockSplitData = await Options.GetTradingAssetStockSplits();
            List<AlphaAdvantageDividendPayoutData> assetToTradeDividendPayoutData = await Options.GetTradingAssetDividendPayouts();

            assetToTradeTimeSeriesData.CalculateAdjustedClose(assetToTradeStockSplitData);
            AssetToTradeProcessedTimeSeriesData = assetToTradeTimeSeriesData.Data.Where(x => x.Key >= Options.StartDate && x.Key <= Options.EndDate).OrderBy(x => x.Key);

            BacktestMetrics = new BacktestMetrics(
                AssetToTradeProcessedTimeSeriesData.First().Key,
                AssetToTradeProcessedTimeSeriesData.Last().Key,
                StrategyPortfolio.GetBuyingPower());

            TradingDaysIterator = AssetToTradeProcessedTimeSeriesData.Select(e => e.Key).GetEnumerator();
            FinalTradingDay = AssetToTradeProcessedTimeSeriesData.Select(e => e.Key).Last();
        }

    }

}