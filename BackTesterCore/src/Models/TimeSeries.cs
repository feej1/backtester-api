

namespace Backtesting.Models
{

    public class TimeSeries
    {

        public string Ticker { get; set; }
        public Dictionary<DateTime, TimeSeriesElement> Data { get; }

        public TimeSeries(string ticker)
        {
            Ticker = ticker;
            Data = new Dictionary<DateTime, TimeSeriesElement>();
        }

        // calculates adjusted close price using stock splits
        public void CalculateAdjustedClose(StockSplit stockSplits)
        {
            if (stockSplits.Ticker != Ticker)
            {
                throw new Exception("Stock splits data provided is for a different ticker than time series data");
            }

            var itr = Data.GetEnumerator();
            while (itr.MoveNext())
            {
                var currItem = itr.Current;
                currItem.Value.AdjustedClose = currItem.Value.Close;

                var splitRaitos = stockSplits.Data.Where(x => x.Key > currItem.Key).Select(x => x.Value);
                if (splitRaitos.Count() > 0)
                {
                    foreach (var ratio in splitRaitos)
                    {
                        currItem.Value.AdjustedClose = currItem.Value.AdjustedClose * (1.0 / ratio);
                    }
                }
            }

        }

    }

    public class TimeSeriesElement
    {
        public double Open { get; set; }
        public double Close { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public int Volume { get; set; }
        public double AdjustedClose { get; set; }

    }

}

