using System.Text.Json;
using System.Text.Json.Serialization;
using Backtesting.Clients;
using Backtesting.Models;
using Backtesting.Services;
using Microsoft.Extensions.Logging;



namespace Backtesting.Models
{

    public class MacdBacktestOptions : IBacktestSettings
    {

        private int MAX_EMA_LENGTH = 250;
        private int SHORTEST_BACKTEST_LENGTH = 200;

        private IStockDataApiClient _apiClient;

        public Strategies Strategy {get; set;}
        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string AssetToTradeTicker { get; set; }

        public string AssetToTrackTicker { get; set; }

        public string? StaticHoldingTicker { get; set; }

        public double? StopLossPercentage { get; set; }

        public int ShortTermEma { get; set; }

        public int LongTermEma { get; set; }

        public int MacdSignalLine { get; set; }

        public void SetApiClient(IStockDataApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public bool AreValid()
        {
            if (!((IBacktestSettings)this).IsValidTicker(AssetToTradeTicker) ||
            (((IBacktestSettings)this).ShouldHoldAssetBetweenTrades() && !((IBacktestSettings)this).IsValidTicker(StaticHoldingTicker) )||
            !((IBacktestSettings)this).IsValidTicker(AssetToTradeTicker))
            {
                return false;
            }

            if (StartDate >= EndDate || EndDate.Subtract(StartDate).Days < SHORTEST_BACKTEST_LENGTH)
            {
                return false;
            }

            if (ShortTermEma > MAX_EMA_LENGTH || LongTermEma > MAX_EMA_LENGTH || ShortTermEma > LongTermEma || MacdSignalLine > MAX_EMA_LENGTH)
            {
                return false;
            }

            return true;
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

        public async Task<TimeSeries> GetTradingAssetTimeSeries()
        {
            return (await _apiClient.GetTimeSeriesDaily(AssetToTradeTicker)).ToTimeSeriesDataModel();
        }

        public async Task<StockSplit> GetTradingAssetStockSplits()
        {
            return (await _apiClient.GetStockSplits(AssetToTradeTicker)).ToStockSplitDataModel();
        }

        public async Task<List<AlphaAdvantageDividendPayoutData>> GetTradingAssetDividendPayouts()
        {
            return (await _apiClient.GetDividendPayouts(AssetToTradeTicker)).Data;
        }

        public async Task<TimeSeries> GetStaticHoldingAssetTimeSeries()
        {
            if (((IBacktestSettings)this).ShouldHoldAssetBetweenTrades())
            {
                return (await _apiClient.GetTimeSeriesDaily(StaticHoldingTicker)).ToTimeSeriesDataModel();
            }
            else return null;
        }

        public async Task<StockSplit> GetStaticHoldingAssetStockSplits()
        {
            if (((IBacktestSettings)this).ShouldHoldAssetBetweenTrades())
            {
                return (await _apiClient.GetStockSplits(StaticHoldingTicker)).ToStockSplitDataModel();
            }
            else return null;
        }

        public async Task<List<AlphaAdvantageDividendPayoutData>> GetStaticHoldingAssetDividendPayouts()
        {
            if (((IBacktestSettings)this).ShouldHoldAssetBetweenTrades())
            {
                return (await _apiClient.GetDividendPayouts(StaticHoldingTicker)).Data;
            }
            else return null;
        }

        public ITradingStrategy GetTradingStrategyHandler()
        {
            return new MacdTradingStrategy(ShortTermEma, LongTermEma, MacdSignalLine);
        }

    }

}
