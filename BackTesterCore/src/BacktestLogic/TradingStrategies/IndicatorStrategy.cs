

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

        protected IOrderedEnumerable<KeyValuePair<DateTime, TimeSeriesElement>> AssetToHoldProcessedTimeSeriesData;

        protected IEnumerator<DateTime> TradingDaysIterator;

        protected DateTime FinalTradingDay;

        private List<PortfolioValue> PortfolioValues;
        
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
                    else if (Options.ShouldHoldAssetBetweenTrades() && tkr == Options.StaticHoldingTicker)
                    {
                        AssetToHoldProcessedTimeSeriesData!.ToDictionary().TryGetValue(dateTime,
                            out var assetToHoldDataPoint);
                        var ownedStock = StrategyPortfolio.GetAmountOfStockOwned(Options.AssetToTradeTicker);
                        currentPortfolioValue.Value = assetToHoldDataPoint.AdjustedClose * ownedStock;
                    }
                }
            }

            currentPortfolioValue.Value = StrategyPortfolio.GetBuyingPower();
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

                // if was holding passive investment between trades sell
                if (Options.ShouldHoldAssetBetweenTrades() && StrategyPortfolio.OwnsStock(Options.StaticHoldingTicker))
                {
                    AssetToHoldProcessedTimeSeriesData!.ToDictionary().TryGetValue(TradingDaysIterator.Current,
                        out var assetToHoldDataPoint);
                    StrategyPortfolio.LiquidateIfOwnsStock(Options.StaticHoldingTicker, assetToHoldDataPoint.AdjustedClose);

                    var buyPrice = StrategyPortfolio.GetPriceOfMostRecentStockPurchase();
                    var amountTraded = StrategyPortfolio.GetNumberOfSharesFromLastSell();
                    BacktestMetrics.UpdateTradeStatistics(buyPrice, assetToHoldDataPoint.AdjustedClose, amountTraded, StrategyPortfolio.GetBuyingPower());
                }
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

                if (Options.ShouldHoldAssetBetweenTrades())
                {
                    AssetToHoldProcessedTimeSeriesData!.ToDictionary().TryGetValue(TradingDaysIterator.Current,
                        out var assetToHoldDataPoint);
                    StrategyPortfolio.BuyAsMuchStockAsPossible(Options.StaticHoldingTicker, assetToHoldDataPoint.AdjustedClose);
                }

            }
            else if (Options.HasStopLoss() && StrategyPortfolio.OwnsStock(Options.AssetToTradeTicker))
            {
                AssetToTradeProcessedTimeSeriesData!.ToDictionary().TryGetValue(TradingDaysIterator.Current,
                    out var assetToTradeDataPoint);

                var purchasePrice = StrategyPortfolio.GetPriceOfMostRecentStockPurchase();
                var currPrice = assetToTradeDataPoint.AdjustedClose; 
                var ratioToBuyPrice = currPrice / purchasePrice;
                var change = 1 - ratioToBuyPrice;

                if (change * 100 > Options.StopLossPercentage)
                {
                    StrategyPortfolio.LiquidateIfOwnsStock(Options.AssetToTradeTicker, assetToTradeDataPoint.AdjustedClose);

                    var buyPrice = StrategyPortfolio.GetPriceOfMostRecentStockPurchase();
                    var amountTraded = StrategyPortfolio.GetNumberOfSharesFromLastSell();
                    BacktestMetrics.UpdateTradeStatistics(buyPrice, assetToTradeDataPoint.AdjustedClose, amountTraded, StrategyPortfolio.GetBuyingPower());

                    if (Options.ShouldHoldAssetBetweenTrades())
                    {
                        AssetToHoldProcessedTimeSeriesData!.ToDictionary().TryGetValue(TradingDaysIterator.Current,
                            out var assetToHoldDataPoint);
                        StrategyPortfolio.BuyAsMuchStockAsPossible(Options.StaticHoldingTicker, assetToHoldDataPoint.AdjustedClose);
                    }
                }
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
            else if (Options.ShouldHoldAssetBetweenTrades() && StrategyPortfolio.OwnsStock(Options.AssetToTrackTicker))
            {
                AssetToHoldProcessedTimeSeriesData!.ToDictionary().TryGetValue(FinalTradingDay, out var assetToHoldDataPoint);
                StrategyPortfolio.LiquidateIfOwnsStock(Options.StaticHoldingTicker, assetToHoldDataPoint.AdjustedClose);

                var buyPrice = StrategyPortfolio.GetPriceOfMostRecentStockPurchase();
                var amountTraded = StrategyPortfolio.GetNumberOfSharesFromLastSell();
                BacktestMetrics.UpdateTradeStatistics(buyPrice, assetToHoldDataPoint.AdjustedClose, amountTraded, StrategyPortfolio.GetBuyingPower());
            }

            UpdatePortfolioValues(true);
        }

        public async Task RetreiveData()
        {
            // Holding asset
            TimeSeries assetToHoldTimeSeriesData = null;
            StockSplit assetToHoldStockSplitData = null;
            List<AlphaAdvantageDividendPayoutData> assetToHoldDividentPayoutData = null;

            // Trading asset
            TimeSeries assetToTradeTimeSeriesData = await Options.GetTradingAssetTimeSeries();
            StockSplit assetToTradeStockSplitData = await Options.GetTradingAssetStockSplits();
            List<AlphaAdvantageDividendPayoutData> assetToTradeDividendPayoutData = await Options.GetTradingAssetDividendPayouts();

            // Tracking asset
            TimeSeries assetToTrackTimeSeriesData = await Options.GetTrackingAssetTimeSeries();
            StockSplit assetToTrackStockSplitData = await Options.GetTrackingAssetStockSplits();
            if (Options.ShouldHoldAssetBetweenTrades())
            {
                assetToHoldTimeSeriesData = await Options.GetStaticHoldingAssetTimeSeries();
                assetToHoldStockSplitData = await Options.GetStaticHoldingAssetStockSplits();
                assetToHoldDividentPayoutData = await Options.GetStaticHoldingAssetDividendPayouts();
                assetToHoldTimeSeriesData.CalculateAdjustedClose(assetToHoldStockSplitData);
                AssetToHoldProcessedTimeSeriesData = assetToHoldTimeSeriesData.Data.Where(x => x.Key >= Options.StartDate && x.Key <= Options.EndDate).OrderBy(x => x.Key);
            }

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