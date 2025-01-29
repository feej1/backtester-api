

namespace Backtesting.Services
{

    public class ExponentialMovingAverage
    {

        private double currentEma = 0;
        private double previousEma = 0;

        private readonly int emaPeriod;

        private readonly double K;

        public ExponentialMovingAverage(int period)
        {
            emaPeriod = period;
            K = (double)2 / (double)(period + 1);
        }

        public (double updatedPreviousEma, double updatedEma) UpdateEma(double currentValue)
        {

            previousEma = currentEma;

            if (currentEma == 0)
            {
                currentEma = currentValue;
                return (updatedPreviousEma: previousEma, updatedEma: currentEma);
            }

            currentEma = (currentValue * K) + (currentEma * (1 - K));
            return (updatedPreviousEma: previousEma, updatedEma: currentEma);
        }

        public double GetPreviousValue()
        {
            return previousEma;
        }

        public double GetCurrentValue()
        {
            return currentEma;
        }

    }


}