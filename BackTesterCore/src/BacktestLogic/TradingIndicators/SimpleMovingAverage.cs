

namespace Backtesting.Services
{

    public class SimpleMovingAverage
    {

        private readonly List<double> Values;
        private double CurrentMovingAverage;
        private double PreviousMovingAverage;
        private readonly int MovingAveragePeriod;

        public SimpleMovingAverage(int period)
        {
            MovingAveragePeriod = period;
            CurrentMovingAverage = 0;
            PreviousMovingAverage = 0;
            Values = new List<double>();
        }

        public double UpdateMovingAverage(double currentValue)
        {
            PreviousMovingAverage = CurrentMovingAverage;

            if (Values.Count < MovingAveragePeriod)
            {
                Values.Add(currentValue);
            }
            else
            {
                Values.Remove(Values.ElementAt(0));
                Values.Add(currentValue);
            }

            CurrentMovingAverage = Values.Aggregate((a, x) => a + x) / (double)MovingAveragePeriod;
            return CurrentMovingAverage;
        }

        public double GetPreviousValue() =>
            PreviousMovingAverage;

        public double GetCurrentValue() =>
            CurrentMovingAverage;

        public bool IsAverageFilled() =>
            Values.Count >= MovingAveragePeriod;

    }


}