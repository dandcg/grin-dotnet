using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grin.CoreImpl.Core.Mod
{
    public static class ModHelper
    {
        /// Common method for converting an amount to a human-readable string

        public static string amount_to_hr_string(ulong amount)
        {
            var namount = ((double) amount / (double) Consensus.GRIN_BASE);
            var places = (int) Math.Log10(((double) Consensus.GRIN_BASE)) + 1;
            return string.Format("{:.*}", places, amount);

        }
    }
}
