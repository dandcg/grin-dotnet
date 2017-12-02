using System;
using System.Collections.Generic;
using Grin.CoreImpl.Core.Hash;
using Grin.CoreImpl.Core.Target;
using Grin.CoreImpl.Ser;

namespace Grin.CoreImpl.Core.Mod
{
    public class Proof:IWriteable,IReadable
        
    {
        private Proof()
        {
            
        }

        /// The nonces
        public uint[] nonces { get; private set; }
        /// The proof size
        public uint proof_size { get; private set; }

        /// Builds a proof with all bytes zeroed out
        public static Proof New(uint[] in_nonces)
        {
            return new Proof
            {
                proof_size = (uint)in_nonces.Length,
                nonces = in_nonces
            };
        }

        /// Builds a proof with all bytes zeroed out
        public static Proof Zero(uint proofSize)
        {
            return new Proof
            {
                proof_size = proofSize,
                nonces = new uint[proofSize]
            };
        }


        /// Converts the proof to a vector of u64s
        public ulong[] to_u64s()
        {
            var out_nonces = new ulong[proof_size];

            for (var n = 0; n < proof_size; n++)
            {
                out_nonces[n] = (ulong) nonces[n];
            }

            return out_nonces;
        }

        /// Converts the proof to a vector of u32s
        public uint[] to_u32s()
        {
            return Clone().nonces;
        }

        /// Converts the proof to a proof-of-work Target so they can be compared.
        /// Hashes the Cuckoo Proof data.
        public Difficulty to_difficulty()
        {
            return Difficulty.From_hash(this.hash());
        }




        public void write(IWriter writer)
        {
            for (var n=0; n<proof_size; n++)
            {
                writer.write_u32(nonces[n]);
            }
        }

        public void read(IReader reader)
        {
            var proofSize =Global.proofsize();
            var pow = new uint[ proofSize];

            for (var n = 0; n < proofSize; n++)
            {
                pow[n] =reader.read_u32();
            }
        }

        public static Proof readnew(IReader reader)
        {
            var proof = new Proof();
            proof.read(reader);
            return proof;
        }


        public Proof Clone()
        {
            var out_nonces = new List<uint>();

            foreach (var n in nonces)
            {
                out_nonces.Add(n);
            }

            return new Proof() {nonces = out_nonces.ToArray(), proof_size = (uint)out_nonces.Count};

        }
    }
}