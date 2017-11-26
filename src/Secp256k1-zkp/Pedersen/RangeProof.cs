﻿using System;
using System.Linq;

namespace Secp256k1Proxy
{
    public class RangeProof
    {
        public byte[] Proof { get; }
        public int Plen { get; }

        public RangeProof(byte[] proof, int plen)
        {
            Proof = proof;
            Plen = plen;
        }

        public static RangeProof zero()
        {
            return new RangeProof(
                new byte[Constants.MAX_PROOF_SIZE],
                0);
        }

        /// The range proof as a byte slice.
        public byte[] Bytes()
        {
            var ret = new byte[Plen];
            Array.Copy(Proof, ret, Plen);
            return ret;
        }

        /// Length of the range proof in bytes.
        public int Len()
        {
            return Plen;
        }

        public RangeProof Clone()
        {
            return new RangeProof(Proof.ToArray(),Plen);
        }
    }
}