using System;

namespace Grin.CoreImpl.Core.Transaction
{
    [Flags]
    public enum KernelFeatures : byte
    {
        /// Options for a kernel's structure or use
        /// No flags
        DefaultKernel = 0b00000000,

        /// Kernel matching a coinbase output
        CoinbaseKernel = 0b00000001
    }
}