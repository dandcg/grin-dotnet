using System;
using Grin.CoreImpl.Core.Block;
using Grin.CoreImpl.Core.Transaction;

namespace Grin.Api.ApiTypes
{
    public class ApiOutputPrintable : ICloneable
    {
        /// The type of output Coinbase|Transaction
        public ApiOutputType output_type { get; set; }

        /// The homomorphic commitment representing the output's amount (as hex
        /// string)
        public string commit { get; set; }

        /// switch commit hash
        public string switch_commit_hash { get; set; }

        /// The height of the block creating this output
        public ulong height { get; set; }

        /// The lock height (earliest block this output can be spent)
        public ulong lock_height { get; set; }

        /// Whether the output has been spent
        public bool spent { get; set; }

        /// Rangeproof hash  (as hex string)
        public string proof_hash { get; set; }


        public static ApiOutputPrintable from_output(Output output, BlockHeader block_header, bool include_proof_hash)
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