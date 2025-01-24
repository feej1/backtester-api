using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Backtesting.Clients;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Backtesting.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices( (context, services ) => {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        services.Configure<KestrelServerOptions>(options =>
        {
            options.AllowSynchronousIO = true;
        });

        var alphaAdvantageClientSettings = new AlphaAdvantageApiClientSettings();
        alphaAdvantageClientSettings.ApiBaseUrl = context.Configuration["AlphaAdvantageApiClientSettingsApiBaseUrl"];
        alphaAdvantageClientSettings.ApiKey = context.Configuration["AlphaAdvantageApiClientSettingsApiKey"];
        services.AddSingleton(alphaAdvantageClientSettings);

        services.AddHttpClient(AlphaAdvantageApiClientConstants.httpClientName, (serviceProvider, httpClient) => {
            httpClient.BaseAddress = new Uri(alphaAdvantageClientSettings.ApiBaseUrl!);
        });
        services.AddScoped<IStockDataApiClient, SampleDataClient>();
        services.AddScoped<IBackTestingService, BacktestingService>();
    })
    .Build();

host.Run();



