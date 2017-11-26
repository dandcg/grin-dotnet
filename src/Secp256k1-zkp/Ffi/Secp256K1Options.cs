using System;

namespace Secp256k1Proxy
{
    [Flags]
    public enum Secp256K1Options : uint

    {
        /// Flag for context to enable no precomputation
        SECP256K1_START_NONE = (1 << 0) | 0,

        /// Flag for context to enable verification precomputation
        SECP256K1_START_VERIFY = (1 << 0) | (1 << 8),

        /// Flag for context to enable signing precomputation
        SECP256K1_START_SIGN = (1 << 0) | (1 << 9),

        /// Flag for keys to indicate uncompressed serialization format
        SECP256K1_SER_UNCOMPRESSED = (1 << 1) | 0,

        /// Flag for keys to indicate compressed serialization format
        SECP256K1_SER_COMPRESSED = (1 << 1) | (1 << 8)
    }
}