using System.Text.Json.Serialization;
using Backtesting.Clients;


namespace Backtesting.Models 
{

    public class AlphaAdvantageStockSplitResponse 
    {
        [JsonPropertyName(AlphaAdvantageApiClientConstants.stockSlpitTickerKey)]
        public string Ticker {get; set;}
        [JsonPropertyName(AlphaAdvantageApiClientConstants.stockSlpitDataKey)]
        public List<AlphaAdvantageStockSplitData> Data {get; set;}
    }

    public class AlphaAdvantageStockSplitData
    {
        [JsonPropertyName(AlphaAdvantageApiClientConstants.stockSplitAmountKey)]
        public string SplitRatioString {get; set;}

        // this date represent the day your stocks get multiplied by four
        [JsonPropertyName(AlphaAdvantageApiClientConstants.stockSplitDateKey)]
        public string SplitDateString {get; set;} 

        public double SplitRatio
        {
            get 
            {
               return double.Parse(SplitRatioString);
            }
            set
            {
                SplitRatioString = value.ToString();
            }
        }
        public DateTime SplitDate
        {
            get 
            {
               return DateTime.Parse(SplitDateString);
            }
            set
            {
                SplitDateString = value.ToString();
            }
        }
    }
}