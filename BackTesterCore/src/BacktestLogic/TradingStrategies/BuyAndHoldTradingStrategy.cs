

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

        public BuyAndHoldTradingStrategy(BuyAndHoldSettings options)
        {
            StrategyPortfolio = new Portfolio();
            Options = options;
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

                return false;
            }

            // buy stock on first day
            if(!StrategyPortfolio.OwnsAnyStock())
            {
                AssetToTradeProcessedTimeSeriesData!.ToDictionary().TryGetValue(TradingDaysIterator.Current,
                    out var assetToTradeDataPoint);
                StrategyPortfolio.BuyAsMuchStockAsPossible(Options.AssetToTradeTicker, assetToTradeDataPoint.AdjustedClose);
            }

            BacktestMetrics.UpdatePercentTimeInvested(true);

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