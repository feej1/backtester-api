

namespace Backtesting.Models
{

    public abstract class IndicatorStrategySettings : IBacktestSettings
    {

        public string AssetToTrackTicker { get; set; }


        public async Task<TimeSeries> GetTrackingAssetTimeSeries()
        {
            CheckApiClient();
            return (await _apiClient.GetTimeSeriesDaily(AssetToTrackTicker)).ToTimeSeriesDataModel();
        }

        public async Task<StockSplit> GetTrackingAssetStockSplits()
        {
            CheckApiClient();
            return (await _apiClient.GetStockSplits(AssetToTrackTicker)).ToStockSplitDataModel();
        }

        public async Task<List<AlphaAdvantageDividendPayoutData>> GetTrackingAssetDividendPayouts()
        {
            CheckApiClient();
            return (await _apiClient.GetDividendPayouts(AssetToTrackTicker)).Data;
        }

    }

}