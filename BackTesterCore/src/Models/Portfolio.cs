




using System.Diagnostics.PerformanceData;
using Microsoft.AspNetCore.Authentication;

namespace Backtesting.Models
{


    public class Portfolio
    {
        private double BuyingPower { get; set; }

        // key: ticker  value: amount
        private readonly Dictionary<string, double> Holdings;

        public readonly Dictionary<TradeHistoryKey, TradeHistoryData> TradeHistory;

        public Portfolio(double startingCash = 1000)
        {
            Holdings = new Dictionary<string, double>();
            TradeHistory = new Dictionary<TradeHistoryKey, TradeHistoryData>();
            BuyingPower = startingCash;
        }


        public bool OwnsAnyStock()
        {
            return Holdings.Count() > 0;
        }

        public double GetPriceOfMostRecentStockPurchase()
        {
            return TradeHistory.Where(e => e.Value.Action == Action.BUY).LastOrDefault().Value.Price;
        }

        public double GetNumberOfSharesFromLastSell()
        {
            return TradeHistory.Where(e => e.Value.Action == Action.SELL).LastOrDefault().Value.Amount;
        }

        public double GetBuyingPower()
        {
            return BuyingPower;
        }

        public int GetNumberOfTrades()
        {
            return TradeHistory.Keys.Count();
        }

        public double GetAmountOfStockOwned(string ticker)
        {
            if (OwnsStock(ticker))
            {
                return Holdings.FirstOrDefault(holding => holding.Key == ticker).Value;
            }

            return 0;

        }

        public bool OwnsStock(string ticker)
        {
            return Holdings.Keys.Any(holdingTicker => holdingTicker == ticker);
        }

        public void BuyStock(string ticker, double amount, double cost)
        {
            if (amount * cost > BuyingPower)
            {
                throw new Exception($"Insignifigant funds to purchase {amount} shares of {ticker} at price {cost}");
            }

            if (OwnsStock(ticker))
            {
                Holdings[ticker] += amount;
            }
            else
            {
                Holdings.Add(ticker, amount);
            }

            var tradeHistoryKey = new TradeHistoryKey()
            {
                Ticker = ticker,
                DateTime = DateTime.Now
            };
            var tradeHistoryData = new TradeHistoryData()
            {
                Action = Action.BUY,
                Amount = amount,
                Price = cost,
                Value = BuyingPower + (amount * cost)
            };
            TradeHistory.Add(tradeHistoryKey, tradeHistoryData);

            BuyingPower -= amount * cost;
        }

        public void BuyAsMuchStockAsPossible(string ticker, double cost)
        {
            if (BuyingPower - 1 >= 0)
            {
                var amount = (BuyingPower - 1) / cost;
                BuyStock(ticker, amount, cost);
            }
        }

        public void SellStock(string ticker, double amount, double cost)
        {

            if (!OwnsStock(ticker))
            {
                throw new Exception($"Trying to sell {ticker} when zero shares are owned");
            }
            else if (amount > Holdings.First(ele => ele.Key == ticker).Value)
            {
                throw new Exception($"Trying to sell more shares of {ticker} than currently owned. Owned: {Holdings.First(ele => ele.Key == ticker).Value} Sell Volume: {amount}");
            }

            var tradeHistoryKey = new TradeHistoryKey()
            {
                Ticker = ticker,
                DateTime = DateTime.Now
            };
            var tradeHistoryData = new TradeHistoryData()
            {
                Action = Action.SELL,
                Amount = amount,
                Price = cost,
                Value = BuyingPower + (amount * cost)
            };
            TradeHistory.Add(tradeHistoryKey, tradeHistoryData);
            Holdings.Remove(ticker);

            BuyingPower += amount * cost;
        }

        public void LiquidateStock(string ticker, double cost)
        {
            if (!OwnsStock(ticker))
            {
                new Exception($"Trying to sell {ticker} when zero shares are owned");
            }

            SellStock(ticker, Holdings[ticker], cost);

        }

        public void LiquidateIfOwnsStock(string ticker, double cost)
        {
            if (OwnsStock(ticker))
            {
                SellStock(ticker, Holdings[ticker], cost);
            }
        }

        public void LiquidateStocks(Dictionary<string, double> holdingsToLiquidate)
        {
            foreach (var holding in holdingsToLiquidate)
            {
                if (!OwnsStock(holding.Key))
                {
                    throw new Exception($"Trying to sell {holding.Key} when zero shares are owned");
                }
            }

            foreach (var holding in holdingsToLiquidate)
            {
                LiquidateStock(holding.Key, holding.Value);
            }
        }
    }

    public class TradeHistoryKey
        {
            public DateTime DateTime;
            public string Ticker;
        }

    public class TradeHistoryData
        {
            public Action Action;
            public double Amount;
            public double Price;
            public double Value;
        }

    public enum Action
    {
        BUY,
        SELL
    }

}


