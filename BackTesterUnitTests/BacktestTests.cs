using NUnit.Framework;
using NUnit;
using Backtesting.Services;
using Microsoft.Extensions.Logging;
using Backtesting.Clients;
using Microsoft.Extensions.Logging.Abstractions;

namespace BackTesterUnitTests
{
    public class BacktestTests
{

    private IBackTestingService _systemUnderTest;

    [SetUp]
    public void Setup()
    {
        _systemUnderTest =  new BacktestingService(new NullLoggerFactory() , new SampleDataClient());
    }

    // Update later, currently just there so I dont have to build th azure function each time
    // [Test]
    // public void BackTest_ShouldRun()
    // {
    //     _systemUnderTest.BackTest(Strategies.MACD_CROSS,
    //         "SPXL",
    //         "SPXS",
    //         -1,
    //         DateTime.Parse("2018-03-01"),
    //         DateTime.Parse("2024-09-30"));

    //     Assert.Pass();
    // }

    // Update later, currently just there so I dont have to build th azure function each time
//     [Test]
//     public void BackTest_BuyAndHold()
//     {
//         _systemUnderTest.BackTest(Strategies.BUY_AND_HOLD,
//             "SPXL",
//             "SPXS",
//             -1,
//             DateTime.Parse("2018-03-01"),
//             DateTime.Parse("2024-09-30"));

//         Assert.Pass();
//     }
// }

    [Test]
    public void BackTest_MvaCross()
    {
        _systemUnderTest.BackTest(Strategies.MOVING_AVERAGE_CROSS,
            "SPXL",
            "SPXS",
            -1,
            DateTime.Parse("2018-03-01"),
            DateTime.Parse("2024-09-30"));

        Assert.Pass();
    }
}

}

