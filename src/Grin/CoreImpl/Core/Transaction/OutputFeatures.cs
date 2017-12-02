using System;

namespace Grin.CoreImpl.Core.Transaction
{
    /// Options for block validation
    [Flags]
    public enum OutputFeatures : byte
    {
        /// No flags
        DefaultOutput = 0b00000000,

        /// Output is a coinbase output, must not be spent until maturity
        CoinbaseOutput = 0b00000001
    }
}