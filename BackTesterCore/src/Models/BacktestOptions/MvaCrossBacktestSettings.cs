using Backtesting.Clients;
using Backtesting.Services;



namespace Backtesting.Models
{

    public class MvaCrossBacktestSettings : IndicatorStrategySettings
    {

        private int MAX_MVA_LENGTH = 250;

        // this member and below are not inherited from interface
        public int ShortTermMva { get; set; }

        public int LongTermMva { get; set; }

        public override bool AreValid()
        {
            if (!this.IsValidTicker(AssetToTradeTicker) ||
            (this.ShouldHoldAssetBetweenTrades() && !this.IsValidTicker(StaticHoldingTicker) )||
            !this.IsValidTicker(AssetToTradeTicker))
            {
                return false;
            }

            if (StartDate >= EndDate || EndDate.Subtract(StartDate).Days < SHORTEST_BACKTEST_LENGTH)
            {
                return false;
            }

            if (ShortTermMva > MAX_MVA_LENGTH || LongTermMva > MAX_MVA_LENGTH || ShortTermMva > LongTermMva)
            {
                return false;
            }

            return true;
        }

        public override ITradingStrategy GetTradingStrategyHandler()
        {
            return new MovingAverageCrossTradingStrategy(this);
        }
    }

}
