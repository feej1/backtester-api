


using Backtesting.Models;

namespace Backtesting.Services
{

    class MacdTradingStrategy : IndicatorStrategy, ITradingStrategy
    {

        private Macd MacdIndicator;

        public MacdTradingStrategy(MacdBacktestOptions macdOptions)
        {
            Options = macdOptions;
            StrategyPortfolio = new Portfolio();
            PortfolioValues = new List<PortfolioValue>();
            MacdIndicator = new Macd(macdOptions.ShortTermEma, macdOptions.LongTermEma, macdOptions.MacdSignalLine);
        }
        protected override bool IsSellConditionMet()
        {
            return MacdIndicator.GetCrossStatus() == Macd.CrossStatus.MacdCrossedBelowSignal ? true : false;
        }

        protected override bool IsBuyConditionMet()
        {
            return MacdIndicator.GetCrossStatus() == Macd.CrossStatus.MacdCrossedAboveSignal ? true : false;
        }

        protected override void UpdateIndicators()
        {
            AssetToTrackProcessedTimeSeriesData!.ToDictionary().TryGetValue(TradingDaysIterator.Current,
                out var price);
            MacdIndicator.UpdateMacd(price.AdjustedClose);
        }
    }

}