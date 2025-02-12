
using System.Text.Json.Serialization;
using Backtesting.Services;

namespace Backtesting.Models
{

    public class BackTestingResponse
    {
        public Strategies Strategy;

        [JsonConverter(typeof(IBacktestSettingsJsonConverter))]
        public IBacktestSettings BacktestSettings {get; set;}

        public BacktestMetrics BacktestStatistics {get; set;}

        public List<PortfolioValue> PortfolioValues {get; set;}
    }


    public class PortfolioValue
    {
        public long Date {get; set;}
        public double Value {get; set;}
    }    

    public class BacktestMetrics 
    {
        private DateTime StartDate;

        private DateTime EndDate;

        private int TotalTradingDaysInBacktest;

        private int DaysInvestedDuringBacktest;

        private double TotalProfitsOfWinningTrades;

        private double TotalLossesOfLosingTrades;

        private double TotalProfitPercentageOfWinningTrades;

        private double TotalLossPercentageOfLosingTrades;

        private List<double> PortfolioValues;

        private List<TradeResult> TradeResultHistory;

        private List<BacktestMetric> MetricList;

        public BacktestMetrics(DateTime startDate, DateTime endDate, double startingCash)
        {
            NetProfitPercentage = new BacktestMetric(MetricNames.NetProfitPercentage, 0, METRIC_TYPE.OVERALL);
            WinRate = new BacktestMetric(MetricNames.WinRate, 0, METRIC_TYPE.OVERALL);
            ProfitFactor = new BacktestMetric(MetricNames.ProfitFactor, 0, METRIC_TYPE.OVERALL);
            MaximumDrawdownPercent = new BacktestMetric(MetricNames.MaximumDrawdownPercent, 0, METRIC_TYPE.OVERALL);
            NumberOfTrades = new BacktestMetric(MetricNames.NumberOfTrades, 0, METRIC_TYPE.OVERALL);
            PercentTimeInvested = new BacktestMetric(MetricNames.PercentTimeInvested, 0, METRIC_TYPE.OVERALL);

            NumberOfWinningTrades = new BacktestMetric(MetricNames.NumberOfWinningTrades, 0, METRIC_TYPE.WINS);
            AverageProfitPercent = new BacktestMetric(MetricNames.AverageProfitPercent, 0, METRIC_TYPE.WINS);
            MaxConsecutiveWins = new BacktestMetric(MetricNames.MaxConsecutiveWins, 0, METRIC_TYPE.WINS);
            LargestWinPercentage = new BacktestMetric(MetricNames.LargestWinPercentage, 0, METRIC_TYPE.WINS);

            NumberOfLosingTrades = new BacktestMetric(MetricNames.NumberOfLosingTrades, 0, METRIC_TYPE.LOSSES);
            AverageLossPercent = new BacktestMetric(MetricNames.AverageLossPercent, 0, METRIC_TYPE.LOSSES);
            MaxConsecutiveLosses = new BacktestMetric(MetricNames.MaxConsecutiveLosses, 0, METRIC_TYPE.LOSSES);
            LargestLossPercentage = new BacktestMetric(MetricNames.LargestLossPercent, 0, METRIC_TYPE.LOSSES);

            MetricList = [NetProfitPercentage, WinRate, ProfitFactor, MaximumDrawdownPercent, NumberOfTrades, PercentTimeInvested,
                            NumberOfWinningTrades, AverageProfitPercent, MaxConsecutiveWins, LargestWinPercentage,
                            NumberOfLosingTrades, AverageLossPercent, MaxConsecutiveLosses, LargestLossPercentage];

            DaysInvestedDuringBacktest = 0; 
            TotalTradingDaysInBacktest = 0;
            StartDate = startDate;
            EndDate = endDate;
            TradeResultHistory = new List<TradeResult>();
            TotalProfitsOfWinningTrades = 0;
            TotalProfitPercentageOfWinningTrades = 0;
            TotalLossesOfLosingTrades = 0;
            TotalLossPercentageOfLosingTrades = 0;
            PortfolioValues = [startingCash];
        }

        public void UpdatePercentTimeInvested(bool isInvested)
        {
            TotalTradingDaysInBacktest++;

            if (isInvested)
            {
                DaysInvestedDuringBacktest++;
            }

            if (DaysInvestedDuringBacktest == 0 || TotalTradingDaysInBacktest == 0)
            {
               PercentTimeInvested.MetricValue = 0;
               return;
            }

            PercentTimeInvested.MetricValue = ((double) DaysInvestedDuringBacktest / (double) TotalTradingDaysInBacktest) * 100;
        }

        public void UpdateTradeStatistics(double enterPrice, double exitPrice, double shares, double currentPortfolioValue)
        {
            var tradeProfitPercentage  = (exitPrice / enterPrice);
            var profitInDollars = (exitPrice - enterPrice) * shares;

            if (profitInDollars < 0)
            {
                tradeProfitPercentage = (1 - tradeProfitPercentage) * 100;
                profitInDollars = profitInDollars * -1;

                NumberOfLosingTrades.MetricValue += 1;
                
                LargestLossPercentage.MetricValue = LargestLossPercentage.MetricValue < tradeProfitPercentage ? tradeProfitPercentage : LargestLossPercentage.MetricValue;
                
                TradeResultHistory.Add(TradeResult.LOSS);
                MaxConsecutiveLosses.MetricValue = GetLongestStreak(TradeResult.LOSS);
                
                TotalLossPercentageOfLosingTrades += tradeProfitPercentage;
                TotalLossesOfLosingTrades += profitInDollars;
                AverageLossPercent.MetricValue = TotalLossPercentageOfLosingTrades / NumberOfLosingTrades.MetricValue;

            }
            else
            {
                tradeProfitPercentage = (tradeProfitPercentage - 1) * 100;

                NumberOfWinningTrades.MetricValue += 1;
                
                LargestWinPercentage.MetricValue = LargestWinPercentage.MetricValue < tradeProfitPercentage ? tradeProfitPercentage : LargestWinPercentage.MetricValue;
                
                TradeResultHistory.Add(TradeResult.WIN);
                MaxConsecutiveWins.MetricValue = GetLongestStreak(TradeResult.WIN);
                
                TotalProfitPercentageOfWinningTrades += tradeProfitPercentage;
                TotalProfitsOfWinningTrades += profitInDollars;
                AverageProfitPercent.MetricValue = TotalProfitPercentageOfWinningTrades / NumberOfWinningTrades.MetricValue;
            }


            NumberOfTrades.MetricValue += 1;
            WinRate.MetricValue = NumberOfWinningTrades.MetricValue / NumberOfTrades.MetricValue;
            UpdateProfitFactor(TotalProfitPercentageOfWinningTrades, TotalLossPercentageOfLosingTrades);
            UpdateMaximumDrawdownPercentage(currentPortfolioValue);

            NetProfitPercentage.MetricValue = ((PortfolioValues.Last() / PortfolioValues.First()) - 1 ) * 100;
        }

        private void UpdateProfitFactor(double totalProfitPercentages, double totalLossPercentages)
        {
            if (totalLossPercentages == 0 && totalLossPercentages == 0)
            {
                ProfitFactor.MetricValue = 1;
                return;
            }
            else if (totalLossPercentages == 0)
            {
                ProfitFactor.MetricValue = totalProfitPercentages;
                return;
            }
            else if (totalProfitPercentages == 0)
            {
                ProfitFactor.MetricValue = 0;
                return;
            }

            ProfitFactor.MetricValue =  totalProfitPercentages / totalLossPercentages;
        }

        private void UpdateMaximumDrawdownPercentage(double currentPortfolioValue)
        {
            PortfolioValues.Add(currentPortfolioValue);

            double maxDrawdown = 0;
            if(PortfolioValues.Count() > 2)
            {
                for(int i = 0; i < PortfolioValues.Count() -1; i++)
                {
                    var tempPeak = PortfolioValues.ElementAt(i);
                    for(int j = i + 1; j < PortfolioValues.Count(); j++)
                    {
                        var tempTrough = PortfolioValues.ElementAt(j);
                        var tempDrawdown = ((tempPeak - tempTrough) / tempPeak) * 100;
                        maxDrawdown = maxDrawdown < tempDrawdown ? tempDrawdown : maxDrawdown;
                    }
                }
            }

            MaximumDrawdownPercent.MetricValue = maxDrawdown;

        }

        private int GetLongestStreak(TradeResult streakType)
        {
            int currentStreak = 0;
            int longestStreak = 0;
            var itr = TradeResultHistory.GetEnumerator();
            while(itr.MoveNext())
            {
                var tradeResult = itr.Current;
                if (tradeResult == streakType)
                {
                    currentStreak++;
                }
                else 
                {
                    longestStreak = longestStreak < currentStreak ? currentStreak : longestStreak;
                    currentStreak = 0;
                }

            }
            return longestStreak < currentStreak ? currentStreak : longestStreak;

        }

        // profit percentage
        public BacktestMetric NetProfitPercentage {get; set;}
        
        // ratio of trades won vs total 
        public BacktestMetric WinRate {get; set;}
        
        //
        public BacktestMetric ProfitFactor {get; set;}
        
        // largest loss
        public BacktestMetric MaximumDrawdownPercent {get; set;}

        // trades made
        public BacktestMetric NumberOfTrades {get; set;}

        // ratio of days holding stocks vs total trading days
        public BacktestMetric PercentTimeInvested {get; set;}





        // number of winning trades
        public BacktestMetric NumberOfWinningTrades {get; set;}

        // average size of profits
        public BacktestMetric AverageProfitPercent {get; set;}

        // max consecutive wins
        public BacktestMetric MaxConsecutiveWins {get; set;}

        // biggest win percentage
        public BacktestMetric LargestWinPercentage {get; set;}




        // Number of losing trades
        public BacktestMetric NumberOfLosingTrades {get; set;}

        // Average size of losses
        public BacktestMetric AverageLossPercent {get; set;}

        // Max consecutives losses
        public BacktestMetric MaxConsecutiveLosses {get; set;}

        // Biggest loss percentage
        public BacktestMetric LargestLossPercentage {get; set;}

        public string ToString()
        {
            var str = "---- Metrics ----\n";
            foreach (var metric in MetricList)
            {
                var valueString = string.Format("{0:N2}", metric.MetricValue);
                var unitString = metric.MetricDisplayName.Contains('%') ? "%" : "";
                str += $"{metric.MetricDisplayName}: {valueString}{unitString}\n";
            }
            return str;
        }

    }

    public enum METRIC_TYPE 
    {
        OVERALL = 0,
        WINS,
        LOSSES
    }

    public class BacktestMetric 
    {

        public BacktestMetric(string name, METRIC_TYPE type){
            MetricValue = 0;
            MetricDisplayName = name;
            MetricType = type;
        }

        public BacktestMetric(string name, double value, METRIC_TYPE type){
            MetricValue = value;
            MetricDisplayName = name;
            MetricType = type;
        }

        public METRIC_TYPE MetricType {get; set;}
        public string MetricDisplayName {get; set;}
        public double MetricValue {get; set;}
    }


}