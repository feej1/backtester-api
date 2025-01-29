
using Backtesting.Services;

namespace Backtesting.Models
{

    public class BuyAndHoldSettings : IBacktestSettings
    {

        public override ITradingStrategy GetTradingStrategyHandler()
        {
            return new BuyAndHoldTradingStrategy(this);
        }

        public override bool AreValid()
        {
            if (!this.IsValidTicker(AssetToTradeTicker))
            {
                return false;
            }

            if (StartDate >= EndDate || EndDate.Subtract(StartDate).Days < SHORTEST_BACKTEST_LENGTH)
            {
                return false;
            }

            return true;
        }
    }
}