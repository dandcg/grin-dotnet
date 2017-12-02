using System;

namespace Secp256k1Proxy.Ffi
{
    [Flags]
    public enum Secp256K1Options : uint

    {
        /// Flag for context to enable no precomputation
        Secp256K1StartNone = (1 << 0) | 0,

        /// Flag for context to enable verification precomputation
        Secp256K1StartVerify = (1 << 0) | (1 << 8),

        /// Flag for context to enable signing precomputation
        Secp256K1StartSign = (1 << 0) | (1 << 9),

        /// Flag for keys to indicate uncompressed serialization format
        Secp256K1SerUncompressed = (1 << 1) | 0,

        /// Flag for keys to indicate compressed serialization format
        Secp256K1SerCompressed = (1 << 1) | (1 << 8)
    }
}