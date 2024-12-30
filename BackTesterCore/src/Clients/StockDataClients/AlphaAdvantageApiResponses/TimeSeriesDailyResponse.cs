using System.Text.Json.Serialization;
using Backtesting.Clients;


namespace Backtesting.Models 
{

    public class AlphaAdvantageTimeSeriesDailyResponse 
    {
        [JsonPropertyName(AlphaAdvantageApiClientConstants.timeSeriesMetaDataJsonKey)]
        public AlphaAdvantageTimeSeriesMetaData MetaData {get; set;}
        [JsonPropertyName(AlphaAdvantageApiClientConstants.timeSeriesDataJsonKey)]
        public Dictionary<string, AlphaAdvantageTimeSeriesElement> Data {get; set;}
    }

    public class AlphaAdvantageTimeSeriesMetaData
    {
        [JsonPropertyName(AlphaAdvantageApiClientConstants.timeSeriesInformationJsonKey)]
        public string EndpointInformation {get; set;}
        [JsonPropertyName(AlphaAdvantageApiClientConstants.timeSeriesStockTickerJsonKey)]
        public string StockSymbol {get; set;}
        [JsonPropertyName(AlphaAdvantageApiClientConstants.timeSeriesLastRefreshedJsonKey)]
        public DateTime DataLastRefreshed {get; set;}
        [JsonPropertyName(AlphaAdvantageApiClientConstants.timeSeriesOutputSizeJsonKey)]
        public string OutputSize {get; set;}
        [JsonPropertyName(AlphaAdvantageApiClientConstants.timeSeriesTimeZoneJsonKey)]
        public string TimeZone {get; set;}
    }

    public class AlphaAdvantageTimeSeriesElement
    {
        [JsonPropertyName(AlphaAdvantageApiClientConstants.timeSeriesOpenJsonKey)]
        public string OpenString {get; set;}
        [JsonPropertyName(AlphaAdvantageApiClientConstants.timeSeriesCloseJsonKey)]
        public string CloseString {get; set;} 
        [JsonPropertyName(AlphaAdvantageApiClientConstants.timeSeriesHighJsonKey)]
        public string HighString {get; set;}
        [JsonPropertyName(AlphaAdvantageApiClientConstants.timeSeriesLowJsonKey)]
        public string LowString {get; set;}
        [JsonPropertyName(AlphaAdvantageApiClientConstants.timeSeriesVolumeJsonKey)]
        public string VolumeString {get; set;}

        public double Open
        {
            get 
            {
               return double.Parse(OpenString);
            }
            set
            {
                OpenString = value.ToString();
            }
        }
        public double Close 
        {
            get 
            {
               return double.Parse(CloseString);
            }
            set
            {
                CloseString = value.ToString();
            }
        }
        public double High
        {
            get 
            {
               return double.Parse(HighString);
            }
            set
            {
                HighString = value.ToString();
            }
        }
        public double Low
        {
            get 
            {
               return double.Parse(LowString);
            }
            set
            {
                LowString = value.ToString();
            }
        }
        public int Volume
        {
            get 
            {
               return int.Parse(VolumeString);
            }
            set
            {
                VolumeString = value.ToString();
            }
        }

    }


}