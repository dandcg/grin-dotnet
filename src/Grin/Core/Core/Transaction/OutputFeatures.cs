using System;

namespace Grin.Core.Core
{
    /// Options for block validation
    [Flags]
    public enum OutputFeatures : byte
    {
        /// No flags
        DEFAULT_OUTPUT = 0b00000000,

        /// Output is a coinbase output, must not be spent until maturity
        COINBASE_OUTPUT = 0b00000001
    }
}