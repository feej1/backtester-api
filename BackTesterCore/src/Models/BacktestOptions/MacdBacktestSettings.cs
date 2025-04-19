using System.Text.Json;
using System.Text.Json.Serialization;
using Backtesting.Clients;
using Backtesting.Models;
using Backtesting.Services;
using Microsoft.Extensions.Logging;



namespace Backtesting.Models
{

    public class MacdBacktestOptions : IndicatorStrategySettings
    {

        private int MAX_EMA_LENGTH = 250;

        public int ShortTermEma { get; set; }

        public int LongTermEma { get; set; }

        public int MacdSignalLine { get; set; }

        public override bool AreValid()
        {
            if (!this.IsValidTicker(AssetToTradeTicker) ||
            !this.IsValidTicker(AssetToTrackTicker))
            {
                return false;
            }

            if (StartDate >= EndDate || EndDate.Subtract(StartDate).Days < SHORTEST_BACKTEST_LENGTH)
            {
                return false;
            }

            if (ShortTermEma > MAX_EMA_LENGTH || LongTermEma > MAX_EMA_LENGTH || ShortTermEma > LongTermEma || MacdSignalLine > MAX_EMA_LENGTH)
            {
                return false;
            }

            return true;
        }

        public override ITradingStrategy GetTradingStrategyHandler()
        {
            return new MacdTradingStrategy(this);
        }

    }

}
