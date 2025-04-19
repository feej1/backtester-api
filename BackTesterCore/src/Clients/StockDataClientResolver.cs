
using Backtesting.Clients;

public static class StockDataClientResolver
{

    public static IStockDataApiClient GetSampleDataApiClient()
    {
        return new SampleDataClient();
    }

    public static IStockDataApiClient GetAlphaAdvantageClient()
    {
        var baseUrl = Environment.GetEnvironmentVariable("AlphaAdvantageApiClientSettingsApiBaseUrl");
        var alphaAdvantageApiSetting = new AlphaAdvantageApiClientSettings()
        {
            ApiBaseUrl = baseUrl,
            ApiKey = AlphaAdvantageRateLimitBypasser.GetRandomApiKey()
        };

        return new AlphaAdvantageClient(alphaAdvantageApiSetting);

    }

    public static IStockDataApiClient GetApiClient()
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
            ApiKey = AlphaAdvantageRateLimitBypasser.GetRandomApiKey()
        };

        return new AlphaAdvantageClient(alphaAdvantageApiSetting);

    }


}