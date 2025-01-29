


using Backtesting.Models;

namespace Backtesting.Services
{

    class MovingAverageCrossTradingStrategy : IndicatorStrategy, ITradingStrategy
    {

        private SimpleMovingAverage ShortTermMva;
        private SimpleMovingAverage LongTermMva;

        public MovingAverageCrossTradingStrategy(MvaCrossBacktestSettings options)
        {
            Options = options;
            StrategyPortfolio = new Portfolio();
            ShortTermMva = new SimpleMovingAverage(options.ShortTermMva);
            LongTermMva = new SimpleMovingAverage(options.LongTermMva);
        }

        public void UpdateIndicators(double price)
        {
            ShortTermMva.UpdateMovingAverage(price);
            LongTermMva.UpdateMovingAverage(price);
        }

        protected override void UpdateIndicators()
        {
            AssetToTrackProcessedTimeSeriesData!.ToDictionary().TryGetValue(TradingDaysIterator.Current,
                out var price);
            ShortTermMva.UpdateMovingAverage(price.AdjustedClose);
            LongTermMva.UpdateMovingAverage(price.AdjustedClose);
        }

        protected override bool IsSellConditionMet()
        {
            if (ShortTermMva.IsAverageFilled() && LongTermMva.IsAverageFilled())
            {
                var previousShortTermMva = ShortTermMva.GetPreviousValue();
                var previousLongTermMva = LongTermMva.GetPreviousValue();
                var currentShortTermMva = ShortTermMva.GetCurrentValue();
                var currentLongTermMva = LongTermMva.GetCurrentValue();
                return previousShortTermMva >= previousLongTermMva && currentShortTermMva < currentLongTermMva;
            }
            return false;
        }

        protected override bool IsBuyConditionMet()
        {
            if (ShortTermMva.IsAverageFilled() && LongTermMva.IsAverageFilled())
            {
                var previousShortTermMva = ShortTermMva.GetPreviousValue();
                var previousLongTermMva = LongTermMva.GetPreviousValue();
                var currentShortTermMva = ShortTermMva.GetCurrentValue();
                var currentLongTermMva = LongTermMva.GetCurrentValue();
                return previousShortTermMva <= previousLongTermMva && currentShortTermMva > currentLongTermMva;
            }
            return false;
        }

    }

}