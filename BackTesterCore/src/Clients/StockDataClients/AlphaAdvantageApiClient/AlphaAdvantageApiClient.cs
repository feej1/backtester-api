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

    public class AlphaAdvantageClient : IStockDataApiClient
    {

        private readonly AlphaAdvantageApiClientSettings _settings;
        private readonly HttpClient _httpClient;

        public AlphaAdvantageClient(
            AlphaAdvantageApiClientSettings settings,
            IHttpClientFactory httpClientFactory
        )
        {
            _settings = settings;
            _httpClient = httpClientFactory.CreateClient(AlphaAdvantageApiClientConstants.httpClientName);
        }

        public async Task<AlphaAdvantageTimeSeriesDailyResponse> GetTimeSeriesDaily(string tkr)
        {

            // https://www.alphavantage.co/query?function=TIME_SERIES_DAILY&symbol=IBM&apikey=V6V3OG1MINVI8JE9
            var request = QueryStringBuilder
                .Initalize(AlphaAdvantageRequestTypes.TIME_SERIES_DAILY)
                .AddSymbol(tkr)
                .AddApiKey(_settings.ApiKey);

            var response = await _httpClient.GetAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var timeseries = JsonSerializer.Deserialize<AlphaAdvantageTimeSeriesDailyResponse>(response.Content.ReadAsStream());
            return timeseries;

            // var data = JsonObject.Parse(response.Content.ReadAsStream());
            // JsonNode metaData = data[AlphaAdvantageApiClientConstants.timeSeriesMetaDataJsonKey];
            // result.MetaData = JsonSerializer.Deserialize<AlphaAdvantageTimeSeriesMetaData>(metaData.ToJsonString());
            // JsonNode data1 = data[AlphaAdvantageApiClientConstants.timeSeriesDataJsonKey];
            // var timeseiresEle = JsonSerializer.Deserialize<Dictionary<string, AlphaAdvantageTimeSeriesElement>>(data1.ToString());
        }

        public async Task<AlphaAdvantageStockSplitResponse> GetStockSplits(string tkr)
        {

            // https://www.alphavantage.co/query?function=SPLITS&symbol=SPXL&apikey=V6V3OG1MINVI8JE9
            var request = QueryStringBuilder
                .Initalize(AlphaAdvantageRequestTypes.SPLITS)
                .AddSymbol(tkr)
                .AddApiKey(_settings.ApiKey);

            var response = await _httpClient.GetAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var stockSplits = JsonSerializer.Deserialize<AlphaAdvantageStockSplitResponse>(response.Content.ReadAsStream());
            return stockSplits;
        }

        public async Task<AlphaAdvantageDividendPayoutResponse> GetDividendPayouts(string tkr)
        {

            // https://www.alphavantage.co/query?function=DIVIDENDS&symbol=IBM&apikey=V6V3OG1MINVI8JE9
            var request = QueryStringBuilder
                .Initalize(AlphaAdvantageRequestTypes.DIVIDENDS)
                .AddSymbol(tkr)
                .AddApiKey(_settings.ApiKey);

            var response = await _httpClient.GetAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var dividendPayouts = JsonSerializer.Deserialize<AlphaAdvantageDividendPayoutResponse>(response.Content.ReadAsStream());
            return dividendPayouts;
        }

    }

    public static class QueryStringBuilder
    {
        public static string Initalize(AlphaAdvantageRequestTypes type)
        {
            return "query?" + "function=" + Enum.GetName(type.GetType(), type);
        }

        public static string AddSymbol(this string currUriQuery, string tkr)
        {
            return currUriQuery + "&" + "symbol=" + tkr;
        }

        public static string AddApiKey(this string currUriQuery, string apiKey)
        {
            return currUriQuery + "&" + "apikey=" + apiKey;
        }
    }

    public enum AlphaAdvantageRequestTypes
    {
        TIME_SERIES_DAILY,
        SPLITS,
        DIVIDENDS
    }

}







