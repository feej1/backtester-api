
using Backtesting.Models;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Microsoft.AspNetCore.SignalR.Protocol;

namespace Backtesting.Services
{

    public interface ITradingStrategy
    {

        public bool MoveNext();

        public BacktestMetrics GetStatistics();

        public Task RetreiveData();
    } 

}