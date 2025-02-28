
using Backtesting.Clients;
using Backtesting.Models;
using Microsoft.Extensions.Logging;

namespace Backtesting.Services
{


    public interface IBackTestingService
    {

        public Task<BacktestResult> BackTest(IBacktestSettings settings);
    }

    public class BacktestingService : IBackTestingService
    {
        private readonly ILogger _logger;

        public BacktestingService(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<BacktestingService>();
        }

        public async Task<BacktestResult> BackTest(IBacktestSettings settings)
        {
            try 
            {
                return await HandleBacktest(settings);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                var result =  new BacktestResult() {
                    ResultData = "Some error occurred processing the backtest",
                    ResponseType = RESPONSE_TYPES.INTERNAL_ERROR
                };
                return result;
            }
                
        }

        private async Task<BacktestResult> HandleBacktest(IBacktestSettings settings)
        {

            if (!settings.AreValid())
            {
                _logger.LogError($"BacktesterService.HandleBacktest(): Invalid Backtest Settings: {settings}");
                var result =  new BacktestResult() {
                    ResultData = "Invalid Settings",
                    ResponseType = RESPONSE_TYPES.INVALID_SETTINGS
                };
                return result;
            }

            var tradingStrategyHandler = settings.GetTradingStrategyHandler();
            await tradingStrategyHandler.RetreiveData();

            while (tradingStrategyHandler.MoveNext())
            {
                // nothing currently 
            }

            Console.WriteLine(tradingStrategyHandler.GetStatistics().ToString());
            var data = new BackTestingResponse()
            {
                Strategy = Strategies.MACD_CROSS,
                BacktestSettings = settings,
                BacktestStatistics = tradingStrategyHandler.GetStatistics(),
                PortfolioValues = tradingStrategyHandler.GetPortfolioValues()
            };
            return new BacktestResult() 
            {
                ResultData = data,
                ResponseType = RESPONSE_TYPES.SUCCESS
            };
        }
    }
}