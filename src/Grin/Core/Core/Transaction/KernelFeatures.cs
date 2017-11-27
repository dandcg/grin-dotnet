using System;

namespace Grin.Core.Core.Transaction
{
    [Flags]
    public enum KernelFeatures : byte
    {
        /// Options for a kernel's structure or use
        /// No flags
        DEFAULT_KERNEL = 0b00000000,

        /// Kernel matching a coinbase output
        COINBASE_KERNEL = 0b00000001
    }
}