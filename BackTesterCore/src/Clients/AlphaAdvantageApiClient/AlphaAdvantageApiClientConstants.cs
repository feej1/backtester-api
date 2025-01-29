

namespace Backtesting.Clients
{
    public static class AlphaAdvantageApiClientConstants
    {
        public const string timeSeriesMetaDataJsonKey= "Meta Data";
        public const string timeSeriesInformationJsonKey= "1. Information";
        public const string timeSeriesStockTickerJsonKey= "2. Symbol";
        public const string timeSeriesLastRefreshedJsonKey= "3. Last Refreshed";
        public const string timeSeriesOutputSizeJsonKey= "4. Output Size";
        public const string timeSeriesTimeZoneJsonKey= "5. Time Zone";
        public const string timeSeriesDataJsonKey= "Time Series (Daily)";
        public const string timeSeriesOpenJsonKey = "1. open";
        public const string timeSeriesHighJsonKey = "2. high";
        public const string timeSeriesLowJsonKey = "3. low";
        public const string timeSeriesCloseJsonKey = "4. close";
        public const string timeSeriesVolumeJsonKey = "5. volume";
        public const string httpClientName = "alphaAdvantage";
        public const string stockSlpitTickerKey = "symbol";
        public const string stockSlpitDataKey = "data";
        public const string stockSplitDateKey = "effective_date";
        public const string stockSplitAmountKey = "split_factor";
        public const string stockDividendExDividendDateKey = "ex_dividend_date";
        public const string stockDividendDeclerationDateKey = "declaration_date";
        public const string stockDividendRecordDateKey = "record_date";
        public const string stockDividendPaymentDateKey = "payment_date";
        public const string stockDividendAmountPerShareKey = "amount";
    }
}