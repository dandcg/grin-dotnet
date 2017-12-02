using System;
using System.Collections.Generic;

namespace Common.Algo
{
    public static class Algo
    {
        public static IEnumerable<IEnumerable<T>> Tuples<T>(this IEnumerable<T> input, int groupCount)
        {
            if (input == null) throw new ArgumentException("input");
            if (groupCount < 1) throw new ArgumentException("groupCount");

            using (var e = input.GetEnumerator())
            {
                while (true)
                {
                    var l = new List<T>();
                    for (var n = 0; n < groupCount; ++n)
                    {
                        if (!e.MoveNext())
                        {
                            if (n != 0)
                            {
                                yield return l;
                            }
                            yield break;
                        }
                        l.Add(e.Current);
                    }
                    yield return l;
                }
            }
        }
    }
}