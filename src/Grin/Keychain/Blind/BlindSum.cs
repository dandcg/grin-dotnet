using System.Collections.Generic;
using Grin.Keychain.ExtKey;

namespace Grin.Keychain.Blind
{
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