
namespace Backtesting.Services
{

    public interface ITradingStrategy
    {
        
        public void UpdateIndicators(double price);

        public bool IsSellConditionMet();

        public bool IsBuyConditionMet();

    } 

}