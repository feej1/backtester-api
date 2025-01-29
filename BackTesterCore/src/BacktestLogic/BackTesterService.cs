
using Backtesting.Clients;
using Backtesting.Models;
using Microsoft.Extensions.Logging;

namespace Backtesting.Services
{


    public interface IBackTestingService
    {

        public Task<BackTestingResponse> BackTest(IBacktestSettings settings);
    }

    public class BacktestingService : IBackTestingService
    {
        private readonly IStockDataApiClient _apiClient;
        private readonly ILogger _logger;

        public BacktestingService(ILoggerFactory loggerFactory, IStockDataApiClient apiClient)
        {
            _logger = loggerFactory.CreateLogger<BacktestingService>();
            _apiClient = apiClient;
        }

        public async Task<BackTestingResponse> BackTest(IBacktestSettings settings)
        {
            return await HandleBacktest(settings);
        }

        private async Task<BackTestingResponse> HandleBacktest(IBacktestSettings settings)
        {

            if (!settings.AreValid())
            {
                _logger.LogError($"BacktesterService.HandleBacktest(): Invalid Backtest Settings: {settings}");
                return null;
            }

            var tradingStrategyHandler = settings.GetTradingStrategyHandler();
            await tradingStrategyHandler.RetreiveData();

            while (tradingStrategyHandler.MoveNext())
            {
                // nothing currently 
            }

            Console.WriteLine(tradingStrategyHandler.GetStatistics().ToString());
            return new BackTestingResponse()
            {
                Strategy = Strategies.MACD_CROSS,
                BacktestSettings = settings,
                BacktestStatistics = tradingStrategyHandler.GetStatistics()
            };
        }
    }
}