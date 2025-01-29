// using Backtesting.Clients;
// using Backtesting.Services;
// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.Azure.Functions.Worker;
// using Microsoft.Azure.Functions.Worker.Http;
// using Microsoft.Extensions.Logging;

// namespace BackTest.Function
// {
//     public class BackTesterJob
//     {
//         private readonly ILogger _logger;
//         private readonly IStockDataApiClient _apiClient;
//         private readonly IBackTestingService _backTestingService;

//         public BackTesterJob(ILoggerFactory loggerFactory, IStockDataApiClient apiClient, IBackTestingService backTestingService)
//         {
//             _logger = loggerFactory.CreateLogger<BackTesterJob>();
//             _apiClient = apiClient;
//             _backTestingService = backTestingService;
//         }


//         [Function("backTest")]
//         public async Task<IActionResult> backTest(
//         [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
//         {

//             _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

//             var res = _backTestingService.BackTest(Strategies.MACD_CROSS,
//             null);

//             return new OkObjectResult($"Welcome to Azure Functions, {req.Query["name"]}!");
//         }
//     }
// }
