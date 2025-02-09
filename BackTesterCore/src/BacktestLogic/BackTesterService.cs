
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
        private readonly ILogger _logger;

        public BacktestingService(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<BacktestingService>();
        }

        public async Task<BackTestingResponse> BackTest(IBacktestSettings settings)
        {
            try 
            {
                return await HandleBacktest(settings);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return null;
            }
                
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

            var portfolioValue = tradingStrategyHandler.GetPortfolioValues();

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