using System.Collections.Generic;
using Secp256k1Proxy;

namespace Grin.Keychain
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

    public class BlindSum
    {
        private BlindSum()
        {
        }

        public List<Identifier> positive_key_ids { get; } = new List<Identifier>();
        public List<Identifier> negative_key_ids { get; } = new List<Identifier>();
        public List<BlindingFactor> positive_blinding_factors { get; } = new List<BlindingFactor>();
        public List<BlindingFactor>negative_blinding_factors { get; } = new List<BlindingFactor>();


        public static BlindSum New()
        {
            return new BlindSum();
        }

        public BlindSum add_key_id(Identifier key_id)
        {
            positive_key_ids.Add(key_id);
            return this;
        }

        public BlindSum sub_key_id(Identifier key_id)
        {
            negative_key_ids.Add(key_id);
            return this;
        }

        /// Adds the provided key to the sum of blinding factors.
        public BlindSum add_blinding_factor(BlindingFactor blind)
        {
            positive_blinding_factors.Add(blind);
            return this;
        }

        /// Subtractss the provided key to the sum of blinding factors.
        public BlindSum sub_blinding_factor(BlindingFactor blind)
        {
            negative_blinding_factors.Add(blind);
            return this;
        }
    }
}