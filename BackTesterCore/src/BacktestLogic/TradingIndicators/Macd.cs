


namespace Backtesting.Services
{

    public class Macd
    {
        private double currentMacd = 0;

        private double previousMacd = 0;

        public ExponentialMovingAverage shortPeriodEma;

        public ExponentialMovingAverage longPeriodEma;

        private ExponentialMovingAverage singnalLineEma;

        public Macd(int shortPeriod, int longPeriod, int signalPeriod)
        {
            shortPeriodEma = new ExponentialMovingAverage(shortPeriod);
            longPeriodEma = new ExponentialMovingAverage(longPeriod);
            singnalLineEma = new ExponentialMovingAverage(signalPeriod);
        }

        public enum CrossStatus
        {
            NoCross,
            MacdCrossedAboveSignal,
            MacdCrossedBelowSignal
        }

        public void UpdateMacd(double newPrice)
        {
            previousMacd = currentMacd;
            (var prevShortEma, var currShortEma) = shortPeriodEma.UpdateEma(newPrice);
            (var prevLongEma, var currLongEma) = longPeriodEma.UpdateEma(newPrice);
            currentMacd = currShortEma - currLongEma;
            singnalLineEma.UpdateEma(currentMacd);
        }

        public CrossStatus GetCrossStatus()
        {
            if (previousMacd <= singnalLineEma.GetPreviousValue() && currentMacd > singnalLineEma.GetCurrentValue())
            {
                return CrossStatus.MacdCrossedAboveSignal;
            }
            else if (previousMacd >= singnalLineEma.GetPreviousValue() && currentMacd < singnalLineEma.GetCurrentValue())
            {
                return CrossStatus.MacdCrossedBelowSignal;
            }
            else return CrossStatus.NoCross;
        }

        public bool IsMAcdPositive() =>
            currentMacd > 0;

        public double GetCurrentMacd() =>
            currentMacd;

        public double GetPreviousMacd() =>
            previousMacd;

        public double GetCurrentSignalValue() =>
            singnalLineEma.GetCurrentValue();

        public double GetPreviousSignalLine() =>
            singnalLineEma.GetPreviousValue();

    }

}