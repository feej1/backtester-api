

using System.ComponentModel.Design;
using Backtesting.Models;

namespace Backtesting.Services
{

    public abstract class IndicatorStrategy : ITradingStrategy
    {
        protected IndicatorStrategySettings Options;

        protected Portfolio StrategyPortfolio;

        protected BacktestMetrics BacktestMetrics;

        protected IOrderedEnumerable<KeyValuePair<DateTime, TimeSeriesElement>> AssetToTradeProcessedTimeSeriesData;

        protected IOrderedEnumerable<KeyValuePair<DateTime, TimeSeriesElement>> AssetToTrackProcessedTimeSeriesData;

        protected IEnumerator<DateTime> TradingDaysIterator;

        protected DateTime FinalTradingDay;

        protected List<PortfolioValue> PortfolioValues;
        
        public List<PortfolioValue> GetPortfolioValues()
        {
            return PortfolioValues.OrderBy(e => e.Date).ToList();
        }

        public void UpdatePortfolioValues(bool isTradingOver = false)
        {
            var dateTime = isTradingOver ? FinalTradingDay : TradingDaysIterator.Current;

            var currentPortfolioValue = new PortfolioValue()
            {
                Date = dateTime.UnixTimestampFromDateTime()
            };

            var heldStocks = StrategyPortfolio.GetStocksCurrentlyHeld();
            if (heldStocks.Count > 0)
            {
                foreach (var tkr in heldStocks)
                {
                    if (tkr == Options.AssetToTradeTicker)
                    {
                        AssetToTradeProcessedTimeSeriesData!.ToDictionary().TryGetValue(dateTime,
                            out var assetToTradeDataPoint);
                        var ownedStock = StrategyPortfolio.GetAmountOfStockOwned(Options.AssetToTradeTicker);
                        currentPortfolioValue.Value = assetToTradeDataPoint.AdjustedClose * ownedStock;
                    }
                }
            }
            else
            {
                currentPortfolioValue.Value = StrategyPortfolio.GetBuyingPower();
            }

            PortfolioValues.Add(currentPortfolioValue);
        }

        public bool MoveNext()
        {
            if (!TradingDaysIterator.MoveNext())
            {
                FinishTest();
                return false;
            }

            UpdatePortfolioValues();
            UpdateIndicators();

            if (IsBuyConditionMet())
            {
                AssetToTradeProcessedTimeSeriesData!.ToDictionary().TryGetValue(TradingDaysIterator.Current,
                    out var assetToTradeDataPoint);
                
                StrategyPortfolio.BuyAsMuchStockAsPossible(Options.AssetToTradeTicker, assetToTradeDataPoint.AdjustedClose);
            }
            else if (IsSellConditionMet() && StrategyPortfolio.OwnsAnyStock())
            {
                AssetToTradeProcessedTimeSeriesData!.ToDictionary().TryGetValue(TradingDaysIterator.Current,
                    out var assetToTradeDataPoint);
                StrategyPortfolio.LiquidateIfOwnsStock(Options.AssetToTradeTicker, assetToTradeDataPoint.AdjustedClose);

                var buyPrice = StrategyPortfolio.GetPriceOfMostRecentStockPurchase();
                var amountTraded = StrategyPortfolio.GetNumberOfSharesFromLastSell();
                BacktestMetrics.UpdateTradeStatistics(buyPrice, assetToTradeDataPoint.AdjustedClose, amountTraded, StrategyPortfolio.GetBuyingPower());

            }

            BacktestMetrics.UpdatePercentTimeInvested(StrategyPortfolio.OwnsAnyStock());
            return true;

        }

        private void FinishTest()
        {

            if (StrategyPortfolio.OwnsStock(Options.AssetToTradeTicker))
            {
                AssetToTradeProcessedTimeSeriesData!.ToDictionary().TryGetValue(FinalTradingDay, out var assetToTradeDataPoint);
                StrategyPortfolio.LiquidateIfOwnsStock(Options.AssetToTradeTicker, assetToTradeDataPoint.AdjustedClose);

                var buyPrice = StrategyPortfolio.GetPriceOfMostRecentStockPurchase();
                var amountTraded = StrategyPortfolio.GetNumberOfSharesFromLastSell();
                BacktestMetrics.UpdateTradeStatistics(buyPrice, assetToTradeDataPoint.AdjustedClose, amountTraded, StrategyPortfolio.GetBuyingPower());
            }
            UpdatePortfolioValues(true);
        }

        public async Task RetreiveData()
        {
            // Trading asset
            TimeSeries assetToTradeTimeSeriesData = await Options.GetTradingAssetTimeSeries();
            StockSplit assetToTradeStockSplitData = await Options.GetTradingAssetStockSplits();
            List<AlphaAdvantageDividendPayoutData> assetToTradeDividendPayoutData = await Options.GetTradingAssetDividendPayouts();

            // Tracking asset
            TimeSeries assetToTrackTimeSeriesData = await Options.GetTrackingAssetTimeSeries();
            StockSplit assetToTrackStockSplitData = await Options.GetTrackingAssetStockSplits();

            assetToTrackTimeSeriesData.CalculateAdjustedClose(assetToTrackStockSplitData);
            AssetToTrackProcessedTimeSeriesData = assetToTrackTimeSeriesData.Data.Where(x => x.Key >= Options.StartDate && x.Key <= Options.EndDate).OrderBy(x => x.Key);

            IOrderedEnumerable<KeyValuePair<DateTime, TimeSeriesElement>> assetToTradeProcessedTimeSeriesData = null;
            if (Options.AssetToTrackTicker == Options.AssetToTradeTicker)
            {
                AssetToTradeProcessedTimeSeriesData = AssetToTrackProcessedTimeSeriesData;
            }
            else
            {
                assetToTradeTimeSeriesData.CalculateAdjustedClose(assetToTradeStockSplitData);
                var assetToTradeProcessedTimSeriesData = assetToTradeTimeSeriesData.Data.Where(x => x.Key >= Options.StartDate && x.Key <= Options.EndDate).OrderBy(x => x.Key);
            }

            TradingDaysIterator = AssetToTrackProcessedTimeSeriesData.Select(itm => itm.Key).GetEnumerator();
            FinalTradingDay = AssetToTrackProcessedTimeSeriesData.Select(itm => itm.Key).Last();

            BacktestMetrics = new BacktestMetrics(
                AssetToTrackProcessedTimeSeriesData.First().Key,
                AssetToTrackProcessedTimeSeriesData.Last().Key,
                StrategyPortfolio.GetBuyingPower());

        }

        public BacktestMetrics GetStatistics()
        {
            return BacktestMetrics;
        }

        protected abstract bool IsSellConditionMet();

        protected abstract bool IsBuyConditionMet();

        protected abstract void UpdateIndicators();

    }
}