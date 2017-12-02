using System.Collections.Generic;
using Grin.KeychainImpl.ExtKey;

namespace Grin.KeychainImpl.Blind
{
    public class BlindSum
    {
        private BlindSum()
        {
        }

        public List<Identifier> PositiveKeyIds { get; } = new List<Identifier>();
        public List<Identifier> NegativeKeyIds { get; } = new List<Identifier>();
        public List<BlindingFactor> PositiveBlindingFactors { get; } = new List<BlindingFactor>();
        public List<BlindingFactor>NegativeBlindingFactors { get; } = new List<BlindingFactor>();


        public static BlindSum New()
        {
            return new BlindSum();
        }

        public BlindSum add_key_id(Identifier keyId)
        {
            PositiveKeyIds.Add(keyId);
            return this;
        }

        public BlindSum sub_key_id(Identifier keyId)
        {
            NegativeKeyIds.Add(keyId);
            return this;
        }

        /// Adds the provided key to the sum of blinding factors.
        public BlindSum add_blinding_factor(BlindingFactor blind)
        {
            PositiveBlindingFactors.Add(blind);
            return this;
        }

        /// Subtractss the provided key to the sum of blinding factors.
        public BlindSum sub_blinding_factor(BlindingFactor blind)
        {
            NegativeBlindingFactors.Add(blind);
            return this;
        }
    }
}