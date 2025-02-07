using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
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

        // CHANGED IMPLIMENTATION ALTHOUGH KEEPING FOR NOTES

        // var alphaAdvantageClientSettings = new AlphaAdvantageApiClientSettings();

        // values are null if not found in app settings
        // alphaAdvantageClientSettings.ApiBaseUrl = context.Configuration["AlphaAdvantageApiClientSettingsApiBaseUrl"];
        // alphaAdvantageClientSettings.ApiKey = context.Configuration["AlphaAdvantageApiClientSettingsApiKey"];
        // services.AddSingleton(alphaAdvantageClientSettings);

        // services.AddHttpClient(AlphaAdvantageApiClientConstants.httpClientName, (serviceProvider, httpClient) => {
        //     httpClient.BaseAddress = new Uri(alphaAdvantageClientSettings.ApiBaseUrl!);
        // });

        // if (context.HostingEnvironment.EnvironmentName == "Development")
        // {
        //     services.AddScoped<IStockDataApiClient, SampleDataClient>();
        // }
        // else 
        // {
        //     services.AddScoped<IStockDataApiClient, AlphaAdvantageClient>();
        // }

        services.AddScoped<IBackTestingService, BacktestingService>();
    })
    .Build();

host.Run();

