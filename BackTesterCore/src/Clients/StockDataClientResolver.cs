
using Backtesting.Clients;

public static class StockDataClientResolver
{

    public static IStockDataApiClient GetSampleDataApiClient()
    {
        return new SampleDataClient();
    }

    public static IStockDataApiClient GetApiClient(string alphaAdvantageApiKey)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

        if (environment == "Development")
        {
            return GetSampleDataApiClient();
        }

        var baseUrl = Environment.GetEnvironmentVariable("AlphaAdvantageApiClientSettingsApiBaseUrl");
        var alphaAdvantageApiSetting = new AlphaAdvantageApiClientSettings()
        {
            ApiBaseUrl = baseUrl,
            ApiKey = alphaAdvantageApiKey
        };

        return new AlphaAdvantageClient(alphaAdvantageApiSetting);

    }


}