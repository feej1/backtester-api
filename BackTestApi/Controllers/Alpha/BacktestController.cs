using Backtesting.Clients;
using Backtesting.Models;
using Backtesting.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace BackTest.Function
{
    public class BacktestController
    {
        private readonly ILogger _logger;
        private readonly IBackTestingService _backTestingService;

        public BacktestController(ILoggerFactory loggerFactory, IBackTestingService backTestingService)
        {
            _logger = loggerFactory.CreateLogger<BacktestController>();
            _backTestingService = backTestingService;
        }


        [Function("MacdBacktest")]
        public async Task<IActionResult> MacdBacktest(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "alpha-v1.0/backtest/macd")] HttpRequest req)
        {

            // get alpha advantage api key, null if not found
            var apiKey = req.Query["AlphaAdvantageApiKey"];

            // get settings from body and create 
            var options = JsonSerializer.Deserialize<MacdBacktestOptions>(req.Body);
            options.SetApiClient(StockDataClientResolver.GetApiClient(apiKey));
            
            // run backtest
            var result = await _backTestingService.BackTest(options);
            return new JsonResult(result);
        }

        [Function("MvaBacktest")]
        public async Task<IActionResult> MvaBacktest(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "alpha-v1.0/backtest/mva")] HttpRequest req)
        {

            // get alpha advantage api key, null if not found
            var apiKey = req.Query["AlphaAdvantageApiKey"];

            // get settings from body and create 
            var options = JsonSerializer.Deserialize<MvaCrossBacktestSettings>(req.Body);
            options.SetApiClient(StockDataClientResolver.GetApiClient(apiKey));
            
            // run backtest
            var result = await _backTestingService.BackTest(options);
            return new JsonResult(result);
        }

        [Function("BuyAndHoldBacktest")]
        public async Task<IActionResult> BuyAndHoldBacktest(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "alpha-v1.0/backtest/buyhold")] HttpRequest req)
        {
            // get alpha advantage api key, null if not found
            var apiKey = req.Query["AlphaAdvantageApiKey"];

            // get settings from body and create 
            var options = JsonSerializer.Deserialize<BuyAndHoldSettings>(req.Body);
            options.SetApiClient(StockDataClientResolver.GetApiClient(apiKey));
            
            // run backtest
            var result = await _backTestingService.BackTest(options);
            return new JsonResult(result);
        }
    }
}
