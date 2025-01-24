using Backtesting.Clients;
using Backtesting.Models;
using Backtesting.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;
using System.Text.Json;

namespace BackTest.Function
{
    public class BacktestController
    {
        private readonly ILogger _logger;
        private readonly IStockDataApiClient _apiClient;
        private readonly IBackTestingService _backTestingService;

        public BacktestController(ILoggerFactory loggerFactory, IStockDataApiClient apiClient, IBackTestingService backTestingService)
        {
            _logger = loggerFactory.CreateLogger<BacktestController>();
            _apiClient = apiClient;
            _backTestingService = backTestingService;
        }


        [Function("Backtest")]
        public async Task<IActionResult> Backtest(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "alpha-v1.0/backtest/run-macd-test")] HttpRequest req)
        {

            // _logger.LogInformation($"C# http trigger executed at: {DateTime.Now}");

            // get settings from body and create 
            var timeseries = JsonSerializer.Deserialize<MacdBacktestOptions>(req.Body);
            IBacktestSettings options = new MacdBacktestOptions()
            {
                StartDate = DateTime.Parse("01/01/2018"),
                EndDate = DateTime.Parse("01/01/2022"),
                AssetToTrackTicker = "SPXL",
                AssetToTradeTicker = "SPXL",
                StaticHoldingTicker = "SPXS",
                StopLossPercentage = null,
                ShortTermEma = 12,
                LongTermEma = 26,
                MacdSignalLine = 9,
                Strategy = Strategies.MACD_CROSS
            };
            options.SetApiClient(_apiClient);

            // run backtest
            var result = await _backTestingService.BackTest(options);
            return new JsonResult(result);
        }

        [Function("BacktestGeneral")]
        public async Task<IActionResult> BacktestGeneral(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "alpha-v1.0/backtest/run-test")] HttpRequest req)
        {

            _logger.LogInformation($"C# http trigger executed at: {DateTime.Now}");

            // get settings from body and create 
            var webOptions = JsonSerializer.Deserialize<MacdBacktestOptions>(req.Body);

            if (webOptions.AreValid())
            {
                return new BadRequestObjectResult($"Incorrect options");
            }

            IBacktestSettings options = new MacdBacktestOptions()
            {
                StartDate = DateTime.Parse("01/01/2018"),
                EndDate = DateTime.Parse("01/01/2022"),
                AssetToTrackTicker = "SPXL",
                AssetToTradeTicker = "SPXL",
                StaticHoldingTicker = "SPXS",
                StopLossPercentage = null,
                ShortTermEma = 12,
                LongTermEma = 26,
                MacdSignalLine = 9
            };

            // run backtest
            var result = await _backTestingService.BackTest(options);
            return new OkObjectResult(result);
        }
    }
}
