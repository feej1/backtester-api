using System.Security.Policy;
using System.Text.Json;
using Backtesting.Models;

namespace Backtesting.Clients
{

    public class AlphaAdvantageClient : IStockDataApiClient
    {

        private readonly AlphaAdvantageApiClientSettings _settings;
        private readonly HttpClient _httpClient;

        public AlphaAdvantageClient(AlphaAdvantageApiClientSettings settings)
        {
            if (settings.ApiBaseUrl == null || settings.ApiKey == null)
                throw new Exception("Settings cant have null values");

            _settings = settings;
            _httpClient = new HttpClient()
            {
                BaseAddress = new Uri(_settings.ApiBaseUrl)
            };
            _httpClient.DefaultRequestHeaders.Add("X-Forwarded-For", AlphaAdvantageRateLimitBypasser.GetRandomIpAddress());
        }

        public async Task<AlphaAdvantageTimeSeriesDailyResponse> GetTimeSeriesDaily(string tkr)
        {

            // https://www.alphavantage.co/query?function=TIME_SERIES_DAILY&symbol=IBM&outputsize=full&apikey=V6V3OG1MINVI8JE9
            var request = QueryStringBuilder
                .Initalize(AlphaAdvantageRequestTypes.TIME_SERIES_DAILY)
                .AddSymbol(tkr)
                .AddApiKey(_settings.ApiKey);

            var response = await _httpClient.GetAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var respy = await response.Content.ReadAsStringAsync();
            // var timeseries = JsonSerializer.Deserialize<AlphaAdvantageTimeSeriesDailyResponse>(response.Content.ReadAsStream());
            // return timeseries;
            return null;

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
            var url = "query?" + "function=" + Enum.GetName(type.GetType(), type);
            
            if (type == AlphaAdvantageRequestTypes.TIME_SERIES_DAILY)
            {
                url += "&outputsize=full";
            }

            return url;
        }

        public static string AddSymbol(this string currUriQuery, string tkr)
        {
            return currUriQuery + "&" + "symbol=" + tkr;
        }

        public static string AddApiKey(this string currUriQuery, string apiKey)
        {
            if (String.IsNullOrWhiteSpace(apiKey))
                throw new Exception("Invalid Api Key");
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







