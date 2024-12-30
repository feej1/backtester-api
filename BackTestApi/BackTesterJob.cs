using Backtesting.Clients;
using Backtesting.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace BackTest.Function
{
    public class BackTesterJob
    {
        private readonly ILogger _logger;
        private readonly IStockDataApiClient _apiClient;
        private readonly IBackTestingService _backTestingService;

        public BackTesterJob(ILoggerFactory loggerFactory, IStockDataApiClient apiClient, IBackTestingService backTestingService)
        {
            _logger = loggerFactory.CreateLogger<BackTesterJob>();
            _apiClient = apiClient;
            _backTestingService = backTestingService;
        }

        // [Function("BackTesterJob")]
        // public async Task Run([TimerTrigger("0 0 12 1 * *", RunOnStartup = true)] TimerInfo myTimer)
        // {
        //     _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

        //     var res = await _apiClient.GetTimeSeriesDaily("SPXL");

        //     _logger.LogInformation(res.ToString());

        //     if (myTimer.ScheduleStatus is not null)
        //     {
        //         _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
        //     }
        // }



        [Function("backTest2")]
        public async Task<IActionResult> backTest(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
        {

            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            var res = _backTestingService.BackTest(Strategies.MACD_CROSS,
            "SPXL",
            "SPXS",
            -1,
            DateTime.Parse("2024-03-01"),
            DateTime.Parse("2024-09-03"));

            return new OkObjectResult($"Welcome to Azure Functions, {req.Query["name"]}!");
        }


        // [Function("getTimeSeries")]
        // public async Task<IActionResult> getTimeSeries(
        // [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
        // {

        //     _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

        //     var res = await _apiClient.GetTimeSeriesDaily("SPXL");

        //     return new OkObjectResult($"Welcome to Azure Functions, {req.Query["name"]}!");
        // }

        // [Function("getStockSplit")]
        // public async Task<IActionResult> getStockSplit(
        // [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
        // {

        //     _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

        //     var res = await _apiClient.GetStockSplits("");

        //     return new OkObjectResult($"Welcome to Azure Functions, {req.Query["name"]}!");
        // }

        // [Function("getDividendPayout")]
        // public async Task<IActionResult> getDividendPayout(
        // [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
        // {

        //     _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

        //     var res = await _apiClient.GetDividendPayouts("SPXL");

        //     return new OkObjectResult($"Welcome to Azure Functions, {req.Query["name"]}!");
        // }

    }
}
