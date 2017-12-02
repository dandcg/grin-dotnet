using System;

namespace Common
{
    public static class ExceptionExtensions
    {
        public static Exception Data(this Exception ex, string key, object value)
        {
            ex.Data.Add(key,value);
            return ex;


        }


    }
}
