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
    public class SampleBacktestController
    {
        private readonly ILogger _logger;
        private readonly IBackTestingService _backTestingService;

        public SampleBacktestController(ILoggerFactory loggerFactory, IStockDataApiClient apiClient, IBackTestingService backTestingService)
        {
            _logger = loggerFactory.CreateLogger<SampleBacktestController>();
            _backTestingService = backTestingService;
        }


        [Function("SampleBacktest")]
        public async Task<IActionResult> SampleBacktest(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "alpha-v1.0/sample-backtest/run-macd-test")] HttpRequest req)
        {
            // get settings from body and create 
            var options = JsonSerializer.Deserialize<MacdBacktestOptions>(req.Body);
            options.SetApiClient(new SampleDataClient());

            // run backtest
            var result = await _backTestingService.BackTest(options);
            return new JsonResult(result);
        }

        [Function("DefaultSampleBacktest")]
        public async Task<IActionResult> DefaultSampleBacktest(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "alpha-v1.0/sample-backtest/run-macd-test")] HttpRequest req)
        {
            // get settings from body and create 
            var options = new MacdBacktestOptions()
            {
                StartDate = DateTime.Parse("2018-10-24"),
                EndDate = DateTime.Parse("2022-01-01"),
                AssetToTrackTicker = "SPXL",
                AssetToTradeTicker = "SPXL",
                StaticHoldingTicker = "SPXS",
                StopLossPercentage = null,
                ShortTermEma = 12,
                LongTermEma = 26,
                MacdSignalLine = 9,
                Strategy = Strategies.MACD_CROSS
            };
            options.SetApiClient(new SampleDataClient());

            // run backtest
            var result = await _backTestingService.BackTest(options);
            return new JsonResult(result);
        }
    }
}
