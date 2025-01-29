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
            // get settings from body and create 
            var options = JsonSerializer.Deserialize<MacdBacktestOptions>(req.Body);
            options.SetApiClient(_apiClient);

            // run backtest
            var result = await _backTestingService.BackTest(options);
            return new JsonResult(result);
        }
    }
}
