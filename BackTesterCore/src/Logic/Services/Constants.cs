

namespace Backtesting.Services
{


    public enum Strategies
    {
        MOVING_AVERAGE_CROSS = 0,
        MACD_CROSS,
        BUY_AND_HOLD
    }

    public enum TradeResult: int
    {
        WIN = 0,
        LOSS
    }

    public class MetricNames
    {
        public static readonly string NetProfitPercentage = "Net Profit (%)";
        
        // ratio of trades won vs total 
        public static readonly string WinRate = "Win Rate (%)";
        
        //
        public static readonly string ProfitFactor = "Profit Factor";
        
        // largest loss
        public static readonly string MaximumDrawdownPercent = "Maximum Drawdown (%)";

        // trades made
        public static readonly string NumberOfTrades = "Number of Trades";

        // ratio of days holding stocks vs total trading days
        public static readonly string PercentTimeInvested = "Time Invested (%)";

        // number of winning trades
        public static readonly string NumberOfWinningTrades = "Number of Winning Trades";

        // average size of profits
        public static readonly string AverageProfitPercent = "Average Profit (%)";

        // max consecutive wins
        public static readonly string MaxConsecutiveWins = "Max Consecutive Winning Trades";

        // biggest win percentage
        public static readonly string LargestWinPercentage = "Largest Win (%)";

        // Number of losing trades
        public static readonly string NumberOfLosingTrades = "Number of Losing Trades";

        // Average size of losses
        public static readonly string AverageLossPercent = "Average Loss (%)";

        // Max consecutives losses
        public static readonly string MaxConsecutiveLosses = "Max Consectutive Losses";

        // Biggest loss percentage
        public static readonly string LargestLossPercent = "Largest Loss (%)";

    }



}