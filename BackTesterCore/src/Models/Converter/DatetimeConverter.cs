
namespace Backtesting.Models
{
    public static class DateTimeExtension
    {
        public static long UnixTimestampFromDateTime(this DateTime date)
        {
            long unixTimestamp = date.Ticks - new DateTime(1970, 1, 1).Ticks;
            unixTimestamp /= TimeSpan.TicksPerSecond;
            // need to multiply by 1000 to get milliseconds, required for moment js library on FE
            return 1000 * unixTimestamp;
        }
    }

}

