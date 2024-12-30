using NUnit.Framework;
using Backtesting.Clients;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.Serialization;
using NuGet.Frameworks;


namespace BackTesterUnitTests
{
    public class ApiTests
    {

        private IStockDataApiClient _systemUnderTest;

        [SetUp]
        public void Setup()
        {
            _systemUnderTest = new SampleDataClient();
        }

        [Test]
        public async Task GetData_ShouldFail()
        {
            //act 
            try 
            {
                var res1 = _systemUnderTest.GetDividendPayouts("");
                var res2 = _systemUnderTest.GetStockSplits("");
                var res3 = _systemUnderTest.GetTimeSeriesDaily("");
            }
            catch (Exception e)
            {
                Assert.AreEqual(e.Message, "There is no sample data for tkr:  ");
                Assert.Pass();
            }
            
            Assert.Fail();
        }

        [Test]
        public async Task GetTimeSeriesDaily_ShouldReturnSuccess()
        {
            // act
            var res = await _systemUnderTest.GetTimeSeriesDaily("SPXL");

            // assert
            Assert.Multiple(() => {
                    Assert.IsNotNull(res);
                    Assert.IsNotNull(res.Data);
                    Assert.IsNotEmpty(res.Data);
                    var getResult = res.Data.TryGetValue("2024-10-04", out var value);
                    Assert.IsTrue(getResult);
                    Assert.AreEqual(value.Close, 162.64);
                    Assert.AreEqual(value.Open, 162.2);
                    Assert.AreEqual(value.Volume, 3462834);
                    Assert.AreEqual(value.High, 163.04);
            });
        }

        [Test]
        public async Task GetStockSplits_ShouldReturnSuccess()
        {
            // act
            var res = await _systemUnderTest.GetStockSplits("SPXL");

            // assert
            Assert.Multiple(() => {
                    Assert.IsNotNull(res);
                    Assert.IsNotNull(res.Data);
                    Assert.IsNotEmpty(res.Data);
                    var stockSplitData = res.Data.ElementAt(0);
                    Assert.AreEqual(res.Data.Count, 2);
                    Assert.AreEqual(stockSplitData.SplitDate, DateTime.Parse("2017-05-01"));
                    Assert.AreEqual(stockSplitData.SplitRatio, 4);
            });
        }

        [Test]
        public async Task GetDividendPayouts_ShouldReturnSuccess()
        {
            // act
            var res = await _systemUnderTest.GetDividendPayouts("SPXL");

            // assert
            Assert.Multiple(() => {
                    Assert.IsNotNull(res);
                    Assert.IsNotNull(res.Data);
                    Assert.IsNotEmpty(res.Data);
                    var stockSplitData = res.Data.ElementAt(0);
                    Assert.AreEqual(stockSplitData.DeclerationDate, DateTime.Parse("2024-01-18"));
                    Assert.AreEqual(stockSplitData.RecordDate, DateTime.Parse("2024-09-24"));
                    Assert.AreEqual(stockSplitData.PaymentDate, DateTime.Parse("2024-10-01"));
                    Assert.AreEqual(stockSplitData.AmountPerShare, 0.19251);
            });
        }
    }

}

