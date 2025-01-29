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

    public interface IStockDataApiClient
    {
        public Task<AlphaAdvantageTimeSeriesDailyResponse> GetTimeSeriesDaily(string tkr);

        public Task<AlphaAdvantageStockSplitResponse> GetStockSplits(string tkr);

        public Task<AlphaAdvantageDividendPayoutResponse> GetDividendPayouts(string tkr);

    }
}







