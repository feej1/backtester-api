using System.Text.Json.Serialization;
using Backtesting.Clients;


namespace Backtesting.Models 
{

    public class StockSplit
    {
        public string Ticker {get; set;}
        public Dictionary<DateTime, double> Data {get;}

        public StockSplit(string tkr)
        {
            Ticker = tkr;
            Data = new Dictionary<DateTime, double>();
        }

        public StockSplit(string tkr, Dictionary<DateTime, double> data)
        {
            Ticker = tkr;
            Data = data;
        }
    }
}