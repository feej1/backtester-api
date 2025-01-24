using System.Text.Json.Serialization;
using Backtesting.Clients;


namespace Backtesting.Models 
{

    public class AlphaAdvantageDividendPayoutResponse 
    {
        [JsonPropertyName(AlphaAdvantageApiClientConstants.stockSlpitTickerKey)]
        public string Ticker {get; set;}
        
        [JsonPropertyName(AlphaAdvantageApiClientConstants.stockSlpitDataKey)]
        public List<AlphaAdvantageDividendPayoutData> Data {get; set;}
    }

    public class AlphaAdvantageDividendPayoutData
    {
        [JsonPropertyName(AlphaAdvantageApiClientConstants.stockDividendExDividendDateKey)]
        public string ExDividendDateString {get; set;}

        [JsonPropertyName(AlphaAdvantageApiClientConstants.stockDividendDeclerationDateKey)]
        public string DeclerationDateString {get; set;} 
        [JsonPropertyName(AlphaAdvantageApiClientConstants.stockDividendRecordDateKey)]
        public string RecordDateString {get; set;} 
        [JsonPropertyName(AlphaAdvantageApiClientConstants.stockDividendPaymentDateKey)]
        public string PaymentDateString {get; set;} 
        [JsonPropertyName(AlphaAdvantageApiClientConstants.stockDividendAmountPerShareKey)]
        public string AmountPerShareString {get; set;} 

        public double AmountPerShare
        {
            get 
            {
               return double.Parse(AmountPerShareString);
            }
            set
            {
                AmountPerShareString = value.ToString();
            }
        }
        public DateTime PaymentDate
        {
            get 
            {
               return DateTime.Parse(PaymentDateString);
            }
            set
            {
                PaymentDateString = value.ToString();
            }
        }
        public DateTime RecordDate
        {
            get 
            {
               return DateTime.Parse(RecordDateString);
            }
            set
            {
                RecordDateString = value.ToString();
            }
        }
        public DateTime DeclerationDate
        {
            get 
            {
               return DateTime.Parse(DeclerationDateString);
            }
            set
            {
                DeclerationDateString = value.ToString();
            }
        }
        public DateTime ExDividendDate
        {
            get 
            {
               return DateTime.Parse(ExDividendDateString);
            }
            set
            {
                ExDividendDateString = value.ToString();
            }
        }
    }
}