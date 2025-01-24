


namespace Backtesting.Services 
{

    class MacdTradingStrategy: ITradingStrategy
    {

        private Macd MacdIndicator;

        public MacdTradingStrategy(int shortTermEma, int longTermEma, int macdSignalLine)
        {
            MacdIndicator = new Macd(shortTermEma, longTermEma, macdSignalLine);
        }

        public void UpdateIndicators(double price)
        {
            MacdIndicator.UpdateMacd(price);
            // var currMacd = MacdIndicator.GetCurrentMacd();
            // var currSignal = MacdIndicator.GetCurrentSignalValue();
            // Console.WriteLine($"signal: {currSignal}     macd: {currMacd}");
        }

        public bool IsSellConditionMet()
        {
            return MacdIndicator.GetCrossStatus() == Macd.CrossStatus.MacdCrossedBelowSignal ? true : false;
        }

        public bool IsBuyConditionMet()
        {
            return MacdIndicator.GetCrossStatus() == Macd.CrossStatus.MacdCrossedAboveSignal ? true : false;
        }

    }

}