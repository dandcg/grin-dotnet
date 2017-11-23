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
       public static Commitment sum_commitments(Secp256k1 secp, Output[] outputs, Input[] inputs, Int64 overage)
        {

            // first, verify each range proof
          
            foreach (var output in outputs)
            {
                output.Verify_proof(secp);
            }

            // then gather the commitments

            var inputCommits = new List<Commitment>();

     
            foreach (var input in inputs)
            {
               inputCommits.Add(input.Value);

            }
            var outputCommits = new List<Commitment>();
            foreach (var output in outputs)
            {
              outputCommits.Add(output.commit);

            }


            // add the overage as output commitment if positive, as an input commitment if
            // negative
     
            if (overage != 0)
            {
                var over_commit = secp.commit_value((UInt64)Math.Abs(overage));
                if (overage < 0)
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


    public class Proof
    {
        /// The nonces
        public UInt32[] nonces { get; }
        /// The proof size
        public int proof_size { get; set; }

        public static Proof Zero(uint proofSize)
        {
            throw new NotImplementedException();
        }





    }
}
