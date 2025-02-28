using Backtesting.Clients;
using System.Text.RegularExpressions;
using Backtesting.Services;


namespace Backtesting.Models
{
    public abstract class IBacktestSettings
    {
        protected int SHORTEST_BACKTEST_LENGTH = 200;

        protected IStockDataApiClient _apiClient;

        public Strategies Strategy { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string AssetToTradeTicker { get; set; }

        public abstract bool AreValid();

        public abstract ITradingStrategy GetTradingStrategyHandler();

        public void SetApiClient(IStockDataApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        protected void CheckApiClient()
        {
            if (_apiClient == null)
            {
                _apiClient = StockDataClientResolver.GetApiClient();    
            }
        }

        protected bool IsValidTicker(string ticker)
        {
            var rx = new Regex(@"^[a-zA-Z]{1,5}$");
            return rx.IsMatch(ticker);
        }

        public async Task<TimeSeries> GetTradingAssetTimeSeries()
        {
            CheckApiClient();
            return (await _apiClient.GetTimeSeriesDaily(AssetToTradeTicker)).ToTimeSeriesDataModel();
        }

        public async Task<StockSplit> GetTradingAssetStockSplits()
        {
            CheckApiClient();
            return (await _apiClient.GetStockSplits(AssetToTradeTicker)).ToStockSplitDataModel();
        }

        public async Task<List<AlphaAdvantageDividendPayoutData>> GetTradingAssetDividendPayouts()
        {
            CheckApiClient();
            return (await _apiClient.GetDividendPayouts(AssetToTradeTicker)).Data;
        }


    }

}
