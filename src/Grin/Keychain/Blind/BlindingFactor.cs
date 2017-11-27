﻿using Secp256k1Proxy;
using Secp256k1Proxy.Key;
using Secp256k1Proxy.Lib;

namespace Grin.Keychain.Blind
{
    /// Encapsulate a secret key for the blind_sum operation
    public class BlindingFactor
    {
        public SecretKey Key { get; }


      

        private BlindingFactor(SecretKey key)
        {
            Key = key;
        }

        public static BlindingFactor New(SecretKey key)
        {
            return new BlindingFactor(key);
        }

        public static BlindingFactor from_slice(Secp256k1 secp, byte[] data)
        {
            return new BlindingFactor(SecretKey.from_slice(secp, data));
        }

        public BlindingFactor clone()
        {
            return new BlindingFactor(Key.clone());
        }
    }
}