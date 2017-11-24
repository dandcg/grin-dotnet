using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Microsoft.AspNetCore.Server.Kestrel.Internal.System.Collections.Sequences;
using Secp256k1Proxy;

namespace Grin.Core.Core
{

    /// Implemented by types that hold inputs and outputs including Pedersen
    /// commitments. Handles the collection of the commitments as well as their
    /// summing, taking potential explicit overages of fees into account.
    public static class Committed
    {

  

        /// Gathers commitments and sum them.
       public static Commitment sum_commitments(this ICommitted committed ,Secp256k1 secp)
        {

            // first, verify each range proof
          
            foreach (var output in committed.outputs_committed())
            {
                output.Verify_proof(secp);
            }

            // then gather the commitments

            var inputCommits = new List<Commitment>();

     
            foreach (var input in committed.inputs_commited())
            {
               inputCommits.Add(input.Value);

            }
            var outputCommits = new List<Commitment>();
            foreach (var output in committed.outputs_committed())
            {
              outputCommits.Add(output.commit);

            }


            // add the overage as output commitment if positive, as an input commitment if
            // negative
     
            if (committed.overage() != 0)
            {
                var over_commit = secp.commit_value((UInt64)Math.Abs(committed.overage()));
                if (committed.overage() < 0)
                {
                    inputCommits.Add(over_commit);
                }
                else
                {
                    outputCommits.Add(over_commit);
                }
            }

            // sum all that stuff
          return  secp.commit_sum(outputCommits.ToArray(), inputCommits.ToArray());
        }

  
    }


    public class Proof:IWriteable,IReadable
        
    {
        private Proof()
        {
            
        }

        /// The nonces
        public UInt32[] nonces { get; private set; }
        /// The proof size
        public uint proof_size { get; private set; }

        /// Builds a proof with all bytes zeroed out
        public static Proof New(UInt32[] in_nonces)
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
                nonces = new UInt32[] {}
            };
        }


        /// Converts the proof to a vector of u64s
        public UInt64[] to_u64s()
        {
        var out_nonces = new UInt64[proof_size];

            for (var n = 0; n < proof_size; n++)
            {
                out_nonces[n] = (UInt64) nonces[n];
            }

            return out_nonces;
        }

        /// Converts the proof to a vector of u32s
        public UInt32[] to_u32s()
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
            var proof_size =Global.proofsize();
            var pow = new uint[ proof_size];

            for (var n = 0; n < proof_size; n++)
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
