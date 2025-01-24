using Backtesting.Models;
using Backtesting.Clients;
using Backtesting.Models;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Backtesting.Services;
using System.Text.Json.Serialization;


namespace Backtesting.Models
{
    public interface IBacktestSettings
    {
        public Strategies Strategy {get; set;}

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string AssetToTradeTicker { get; set; }

        public string AssetToTrackTicker { get; set; }

        public string? StaticHoldingTicker { get; set; }

        public double? StopLossPercentage { get; set; }

        public void SetApiClient(IStockDataApiClient apiClient);

        public bool AreValid();

        public Task<TimeSeries> GetTrackingAssetTimeSeries();

        public Task<StockSplit> GetTrackingAssetStockSplits();

        public Task<List<AlphaAdvantageDividendPayoutData>> GetTrackingAssetDividendPayouts();

        public Task<TimeSeries> GetTradingAssetTimeSeries();

        public Task<StockSplit> GetTradingAssetStockSplits();

        public Task<List<AlphaAdvantageDividendPayoutData>> GetTradingAssetDividendPayouts();

        public Task<TimeSeries> GetStaticHoldingAssetTimeSeries();

        public Task<StockSplit> GetStaticHoldingAssetStockSplits();

        public Task<List<AlphaAdvantageDividendPayoutData>> GetStaticHoldingAssetDividendPayouts();

        public ITradingStrategy GetTradingStrategyHandler();

        // eventually this should be moved out into a some other class or file
        public bool IsValidTicker(string ticker)
        {
            var rx = new Regex(@"^[a-zA-Z]{1,5}$");
            return rx.IsMatch(ticker);
        }

        public bool ShouldHoldAssetBetweenTrades()
        {
            return StaticHoldingTicker != null;
        }
    }

}
