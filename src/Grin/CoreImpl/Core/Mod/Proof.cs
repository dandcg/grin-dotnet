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
        public uint[] Nonces { get; private set; }
        /// The proof size
        public uint ProofSize { get; private set; }

        /// Builds a proof with all bytes zeroed out
        public static Proof New(uint[] inNonces)
        {
            return new Proof
            {
                ProofSize = (uint)inNonces.Length,
                Nonces = inNonces
            };
        }

        /// Builds a proof with all bytes zeroed out
        public static Proof Zero(uint proofSize)
        {
            return new Proof
            {
                ProofSize = proofSize,
                Nonces = new uint[proofSize]
            };
        }


        /// Converts the proof to a vector of u64s
        public ulong[] To_u64s()
        {
            var outNonces = new ulong[ProofSize];

            for (var n = 0; n < ProofSize; n++)
            {
                outNonces[n] = Nonces[n];
            }

            return outNonces;
        }

        /// Converts the proof to a vector of u32s
        public uint[] To_u32s()
        {
            return Clone().Nonces;
        }

        /// Converts the proof to a proof-of-work Target so they can be compared.
        /// Hashes the Cuckoo Proof data.
        public Difficulty To_difficulty()
        {
            return Difficulty.From_hash(this.Hash());
        }




        public void Write(IWriter writer)
        {
            for (var n=0; n<ProofSize; n++)
            {
                writer.write_u32(Nonces[n]);
            }
        }

        public void Read(IReader reader)
        {
            var proofSize =Global.Proofsize();
            var pow = new uint[ proofSize];

            for (var n = 0; n < proofSize; n++)
            {
                pow[n] =reader.read_u32();
            }
        }

        public static Proof Readnew(IReader reader)
        {
            var proof = new Proof();
            proof.Read(reader);
            return proof;
        }


        public Proof Clone()
        {
            var outNonces = new List<uint>();

            foreach (var n in Nonces)
            {
                outNonces.Add(n);
            }

            return new Proof() {Nonces = outNonces.ToArray(), ProofSize = (uint)outNonces.Count};

        }
    }
}