using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Grin.CoreImpl.Core.Mod
{
    public static class ModHelper
    {
        /// Common method for converting an amount to a human-readable string

        public static string amount_to_hr_string(ulong amount)
        {
            var namount = (double)((double) amount / (double) Consensus.GRIN_BASE);
            var places = (int) Math.Log10(((double) Consensus.GRIN_BASE));

            return namount.ToString("F"+places );

        }

        public static ulong amount_from_hr_string(string amount)
        {
        var namount = double.Parse(amount);
            return (ulong) ((namount * (double) Consensus.GRIN_BASE));
        }
    }
}
