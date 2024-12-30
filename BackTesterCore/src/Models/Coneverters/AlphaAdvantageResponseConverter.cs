

using System.Runtime.CompilerServices;

namespace Backtesting.Models
{


    public static class AlphaAdvantageResponseConverters
    {
        public static StockSplit ToStockSplitDataModel(this AlphaAdvantageStockSplitResponse apiResponse)
        {
            StockSplit stockSplit = new StockSplit(apiResponse.Ticker);
            apiResponse.Data.ForEach(ele => stockSplit.Data.Add(ele.SplitDate, ele.SplitRatio));
            return stockSplit;
        }

        public static TimeSeries ToTimeSeriesDataModel(this AlphaAdvantageTimeSeriesDailyResponse apiResponse)
        {
            TimeSeries timeSeries = new TimeSeries(apiResponse.MetaData.StockSymbol);
            foreach (var keyValuePair in apiResponse.Data)
            {
                timeSeries.Data.Add(DateTime.Parse(keyValuePair.Key), new TimeSeriesElement()
                {
                    Close = keyValuePair.Value.Close,
                    High = keyValuePair.Value.High,
                    Low = keyValuePair.Value.Low,
                    Volume = keyValuePair.Value.Volume,
                    Open = keyValuePair.Value.Open
                });
            }
            return timeSeries;
        }
    }

}