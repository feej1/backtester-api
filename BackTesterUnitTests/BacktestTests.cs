using NUnit.Framework;
using NUnit;
using Backtesting.Services;
using Microsoft.Extensions.Logging;
using Backtesting.Clients;
using Microsoft.Extensions.Logging.Abstractions;
using Backtesting.Models;

namespace BackTesterUnitTests
{
    public class BacktestTests
    {

        private IBackTestingService _systemUnderTest;

        [SetUp]
        public void Setup()
        {
            _systemUnderTest = new BacktestingService(new LoggerFactory(), new SampleDataClient());
        }

        // Update later, currently just there so I dont have to build th azure function each time
        [Test]
        public async Task BackTest_MacdCross()
        {
            // prepare
            IBacktestSettings options = new MacdBacktestOptions()
            {
                StartDate = DateTime.Parse("01/01/2018"),
                EndDate = DateTime.Parse("01/01/2022"),
                AssetToTrackTicker = "SPXL",
                AssetToTradeTicker = "SPXL",
                StaticHoldingTicker = "SPXS",
                StopLossPercentage = null,
                ShortTermEma = 12,
                LongTermEma = 26,
                MacdSignalLine = 9,
                Strategy = Strategies.MACD_CROSS
            };
            options.SetApiClient(new SampleDataClient());

            // run
            var result = await _systemUnderTest.BackTest(options);

            // assert
            Assert.That(result == null, Is.False);
        }

        //     Update later, currently just there so I dont have to build th azure function each time
        // [Test]
        // public async Task BackTest_BuyAndHold()
        // {
        //     var result = await _systemUnderTest.BackTest(Strategies.BUY_AND_HOLD,
        //     null);

        //     Assert.IsFalse(result == null);
        //     Assert.Pass();
        // }

        // [Test]
        // public async Task BackTest_MvaCross()
        // {
        //     var result = await _systemUnderTest.BackTest(Strategies.MOVING_AVERAGE_CROSS,
        //         null);

        //     Assert.IsFalse(result == null);
        //     Assert.Pass();
        // }
    }

}

