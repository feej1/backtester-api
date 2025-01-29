using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Transactions;
using Backtesting.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Backtesting.Clients
{

    public class SampleDataClient : IStockDataApiClient
    {
        
        private readonly string[] allowedTkrs = {"SPXL", "SPXS"};

        public SampleDataClient()
        {}

        public Task<AlphaAdvantageTimeSeriesDailyResponse> GetTimeSeriesDaily(string tkr)
        {
            validateTkr(tkr);
            var stream = new StreamReader("./src/SampleData/" + tkr.ToUpper() + "/" + tkr.ToUpper() + "DailyTimeSeries.json");
            var timeseries = JsonSerializer.Deserialize<AlphaAdvantageTimeSeriesDailyResponse>(stream.BaseStream);
            return Task.FromResult<AlphaAdvantageTimeSeriesDailyResponse>(timeseries);
        }

        public Task<AlphaAdvantageStockSplitResponse> GetStockSplits(string tkr)
        {
            validateTkr(tkr);
            var stream = new StreamReader("./src/SampleData/" + tkr.ToUpper() + "/" + tkr.ToUpper() + "StockSplit.json");
            var stockSplits = JsonSerializer.Deserialize<AlphaAdvantageStockSplitResponse>(stream.BaseStream);
            return Task.FromResult<AlphaAdvantageStockSplitResponse>(stockSplits);
        }

        public Task<AlphaAdvantageDividendPayoutResponse> GetDividendPayouts(string tkr)
        {
            validateTkr(tkr);
            var stream = new StreamReader("./src/SampleData/" + tkr.ToUpper() + "/" + tkr.ToUpper() + "StockDividend.json");
            var dividendPayouts = JsonSerializer.Deserialize<AlphaAdvantageDividendPayoutResponse>(stream.BaseStream);
            return Task.FromResult<AlphaAdvantageDividendPayoutResponse>(dividendPayouts);
        }

        private void validateTkr(string tkr)
        {
            if (!allowedTkrs.Contains(tkr)){
                throw new Exception($"There is no sample data for tkr: {tkr} ");
            }
            
        }
    }
}







