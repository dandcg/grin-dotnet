using System;

namespace Common
{
    public static class DateTimeExtensions
    {

        public static DateTime FromUnixTime(this long unixTime)
        {
            var dt = DateTimeOffset.FromUnixTimeSeconds(unixTime).UtcDateTime;
            return dt;
        }

        public static long ToUnixTime(this DateTime date)
        {
            var ut = ((DateTimeOffset)date).ToUnixTimeSeconds();
            return ut;
        }

        public static DateTime PrecisionSeconds(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, date.Kind);
        }
    }


}
