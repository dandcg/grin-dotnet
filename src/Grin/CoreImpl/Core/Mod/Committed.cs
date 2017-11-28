using System;
using System.Collections.Generic;
using Grin.CoreImpl.Ser;
using Secp256k1Proxy.Lib;
using Secp256k1Proxy.Pedersen;

namespace Grin.CoreImpl.Core.Mod
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
               inputCommits.Add(input.Commitment);

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
}
