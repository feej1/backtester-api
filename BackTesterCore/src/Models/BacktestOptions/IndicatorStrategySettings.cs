

namespace Backtesting.Models
{

    public abstract class IndicatorStrategySettings : IBacktestSettings
    {

        private const double MAX_STOP_LOSS_PERCENT = 50;
        private const double MIN_STOP_LOSS_PERCENT = .5;

        public string AssetToTrackTicker { get; set; }

        public string? StaticHoldingTicker { get; set; }

        public double? StopLossPercentage { get; set; }

        public bool ShouldHoldAssetBetweenTrades()
        {
            return !String.IsNullOrWhiteSpace(StaticHoldingTicker);
        }

        public bool HasStopLoss()
        {
            return StopLossPercentage != null && 
                StopLossPercentage >= MIN_STOP_LOSS_PERCENT &&
                StopLossPercentage <= MAX_STOP_LOSS_PERCENT;
        }

        public async Task<TimeSeries> GetTrackingAssetTimeSeries()
        {
            return (await _apiClient.GetTimeSeriesDaily(AssetToTrackTicker)).ToTimeSeriesDataModel();
        }

        public async Task<StockSplit> GetTrackingAssetStockSplits()
        {
            return (await _apiClient.GetStockSplits(AssetToTrackTicker)).ToStockSplitDataModel();
        }

        public async Task<List<AlphaAdvantageDividendPayoutData>> GetTrackingAssetDividendPayouts()
        {
            return (await _apiClient.GetDividendPayouts(AssetToTrackTicker)).Data;
        }

        public async Task<TimeSeries> GetStaticHoldingAssetTimeSeries()
        {
            if (this.ShouldHoldAssetBetweenTrades())
            {
                return (await _apiClient.GetTimeSeriesDaily(StaticHoldingTicker)).ToTimeSeriesDataModel();
            }
            else return null;
        }

        public async Task<StockSplit> GetStaticHoldingAssetStockSplits()
        {
            if (this.ShouldHoldAssetBetweenTrades())
            {
                return (await _apiClient.GetStockSplits(StaticHoldingTicker)).ToStockSplitDataModel();
            }
            else return null;
        }

        public async Task<List<AlphaAdvantageDividendPayoutData>> GetStaticHoldingAssetDividendPayouts()
        {
            if (this.ShouldHoldAssetBetweenTrades())
            {
                return (await _apiClient.GetDividendPayouts(StaticHoldingTicker)).Data;
            }
            else return null;
        }


    }

}