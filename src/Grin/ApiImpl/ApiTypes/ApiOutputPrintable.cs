using System;
using Grin.CoreImpl.Core.Block;
using Grin.CoreImpl.Core.Transaction;

namespace Grin.ApiImpl.ApiTypes
{
    public class ApiOutputPrintable : ICloneable
    {
        /// The type of output Coinbase|Transaction
        public ApiOutputType OutputType { get; set; }

        /// The homomorphic commitment representing the output's amount (as hex
        /// string)
        public string Commit { get; set; }

        /// switch commit hash
        public string SwitchCommitHash { get; set; }

        /// The height of the block creating this output
        public ulong Height { get; set; }

        /// The lock height (earliest block this output can be spent)
        public ulong LockHeight { get; set; }

        /// Whether the output has been spent
        public bool Spent { get; set; }

        /// Rangeproof hash  (as hex string)
        public string ProofHash { get; set; }


        public static ApiOutputPrintable from_output(Output output, BlockHeader blockHeader, bool includeProofHash)
        {
            throw new NotImplementedException();

            //let(output_type, lock_height) = match output.features {
            //    x if x.contains(core::transaction::COINBASE_OUTPUT) => (
            //        OutputType::Coinbase,
            //        block_header.height + global::coinbase_maturity(), 

            //        ),
            //    _ => (OutputType::Transaction, 0),
            //}
            //;
            //OutputPrintable {
            //    output_type:
            //    output_type,
            //    commit:
            //    util::to_hex(output.commit.0.to_vec()),
            //    switch_commit_hash:
            //    util::to_hex(output.switch_commit_hash.hash.to_vec()),
            //    height:
            //    block_header.height,
            //    lock_height:
            //    lock_height,
            //    spent:
            //    true,
            //    proof_hash:
            //    match include_proof_hash {
            //        true => Some(util::to_hex(output.proof.hash().to_vec())),
            //        false => None,
            //    }
            //}
        }

        public object Clone()
        {
            throw new NotImplementedException();
        }
    }
}