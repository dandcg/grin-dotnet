using System;
using System.Collections.Generic;
using System.Text;

namespace Common
{
    public static class DateTimeExtensions
    {

        public static DateTime FromUnixTime(this long unixTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var dt = epoch.AddSeconds(unixTime);
            return dt;
        }

        public static long ToUnixTime(this DateTime date)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var ut = Convert.ToInt64((date - epoch).TotalSeconds);
            return ut;
        }

        public static DateTime PrecisionSeconds(this DateTime date)
        {
          
            return new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, date.Kind);
        }
    }


}
