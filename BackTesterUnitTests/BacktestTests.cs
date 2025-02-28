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
            _systemUnderTest = new BacktestingService(new LoggerFactory());
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
                StaticHoldingTicker = "",
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
            Assert.That(result.ResponseType == RESPONSE_TYPES.SUCCESS, Is.True);
        }

        [Test]
        public async Task BackTest_MvaCross()
        {
            // prepare
            IBacktestSettings options = new MvaCrossBacktestSettings()
            {
                StartDate = DateTime.Parse("01/01/2018"),
                EndDate = DateTime.Parse("01/01/2022"),
                AssetToTrackTicker = "SPXL",
                AssetToTradeTicker = "SPXL",
                StaticHoldingTicker = "",
                StopLossPercentage = 7,
                ShortTermMva = 12,
                LongTermMva = 26,
                Strategy = Strategies.MOVING_AVERAGE_CROSS
            };
            options.SetApiClient(new SampleDataClient());

            // run
            var result = await _systemUnderTest.BackTest(options);

            // assert
            Assert.That(result.ResponseType == RESPONSE_TYPES.SUCCESS, Is.True);
        }

        [Test]
        public async Task BackTest_BuyAndHold()
        {
            // prepare
            IBacktestSettings options = new BuyAndHoldSettings()
            {
                StartDate = DateTime.Parse("01/01/2018"),
                EndDate = DateTime.Parse("01/01/2024"),
                AssetToTradeTicker = "SPXL",
                Strategy = Strategies.BUY_AND_HOLD
            };
            options.SetApiClient(new SampleDataClient());

            // run
            var result = await _systemUnderTest.BackTest(options);

            // assert
            Assert.That(result.ResponseType == RESPONSE_TYPES.SUCCESS, Is.True);
        }
    }
}

