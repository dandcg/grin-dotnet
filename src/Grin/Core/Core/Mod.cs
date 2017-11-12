using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grin.Core.Core
{

//    /// Implemented by types that hold inputs and outputs including Pedersen
//    /// commitments. Handles the collection of the commitments as well as their
//    /// summing, taking potential explicit overages of fees into account.
//public static trait Committed {
///// Gathers commitments and sum them.
//fn sum_commitments(&self, secp: &Secp256k1) -> Result<Commitment, secp::Error> {
//    // first, verify each range proof
//    let ref outputs = self.outputs_committed();
//    for output in * outputs
//    {
//        try!(output.verify_proof(secp))
//    }

//    // then gather the commitments
//    let mut input_commits = map_vec!(self.inputs_committed(), |inp| inp.commitment());
//    let mut output_commits = map_vec!(self.outputs_committed(), |out| out.commitment());

//    // add the overage as output commitment if positive, as an input commitment if
//    // negative
//    let overage = self.overage();
//    if overage != 0 {
//        let over_commit = secp.commit_value(overage.abs() as u64).unwrap();
//        if overage< 0 {
//            input_commits.push(over_commit);
//        } else {
//            output_commits.push(over_commit);
//        }
//    }

//    // sum all that stuff
//    secp.commit_sum(output_commits, input_commits)
//}

///// Vector of committed inputs to verify
//fn inputs_committed(&self) -> &Vec<Input>;

///// Vector of committed inputs to verify
//fn outputs_committed(&self) -> &Vec<Output>;

///// The overage amount expected over the commitments. Can be negative (a
///// fee) or positive (a reward).
//fn overage(&self) -> i64;
//}


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
