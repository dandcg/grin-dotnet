using System;

namespace Grin.CoreImpl.Core.Mod
{
    public static class ModHelper
    {
        /// Common method for converting an amount to a human-readable string

        public static string Amount_to_hr_string(ulong amount)
        {
            var namount = amount / (double) Consensus.GrinBase;
            var places = (int) Math.Log10(Consensus.GrinBase);

            return namount.ToString("F"+places );

        }

        public static ulong Amount_from_hr_string(string amount)
        {
        var namount = double.Parse(amount);
            return (ulong) (namount * Consensus.GrinBase);
        }
    }
}
