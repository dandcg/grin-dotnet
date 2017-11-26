using System;
using System.Linq;
using Common;

namespace Secp256k1Proxy
{
    public class Commitment
    {

        public override string ToString()
        {

            return Value.AsString();

        }

        public byte[] Value { get; }

        public string Hex { get; }

        internal Commitment(byte[] value)
        {
            Value = value;
            Hex = HexUtil.to_hex(value);
        }

        public static Commitment from(byte[] value)
        {
            return new Commitment(value.ToArray());
        }
        
        /// Builds a Hash from a byte vector. If the vector is too short, it will be
        /// completed by zeroes. If it's too long, it will be truncated.
        public static Commitment from_vec(byte[] v)
        {
            var h = new byte[Constants.PEDERSEN_COMMITMENT_SIZE];

            for (var i = 0; i < Math.Min(v.Length, Constants.PEDERSEN_COMMITMENT_SIZE); i++)
                h[i] = v[i];
            return new Commitment(h);
        }

        /// Uninitialized commitment, use with caution
        public static Commitment blank()
        {
            return new Commitment(new byte[Constants.PEDERSEN_COMMITMENT_SIZE]);
        }

        /// Converts a commitment into two "candidate" public keys
        /// one of these will be valid, the other has the incorrect parity
        /// we just don't know which is which...
        /// once secp provides the necessary api we will no longer need this hack
        /// grin uses the public key to verify signatures (hopefully one of these keys works)
        public PublicKey[] to_two_pubkeys(Secp256k1 secp)
        {
            var pks = new PublicKey[2];

            var pk1 = new byte[Constants.COMPRESSED_PUBLIC_KEY_SIZE];

            for (var i = 0; i < Value.Length; i++)

                if (i == 0) pk1[i] = 0x02;
                else
                    pk1[i] = Value[i];
            // TODO - we should not unwrap these here, and handle errors better
            pks[0] = PublicKey.from_slice(secp, pk1);

            var pk2 = new byte[Constants.COMPRESSED_PUBLIC_KEY_SIZE];
            for (var i = 0; i < Value.Length; i++)

                if (i == 0) pk2[i] = 0x03;
                else
                    pk2[i] = Value[i];


            pks[1] = PublicKey.from_slice(secp, pk2);

            return pks;
        }


        /// Converts a commitment to a public key
        /// TODO - we need an API in secp to convert commitments to public keys safely
        /// a commitment is prefixed 08/09 and public keys are prefixed 02/03
        /// see to_two_pubkeys() for a short term workaround
        public PublicKey to_pubkey(Secp256k1 secp)

        {
            return PublicKey.from_slice(secp, Value);
        }


        public Commitment Clone()
        {
            return new Commitment(Value.ToArray());
        }

    }
}