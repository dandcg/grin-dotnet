using System;
using Grin.CoreImpl.Core.Block;
using Grin.CoreImpl.Core.Transaction;
using Secp256k1Proxy.Pedersen;

namespace Grin.Api.ApiTypes
{
    public struct ApiOutput : ICloneable
    {
        /// The type of output Coinbase|Transaction
        public ApiOutputType output_type { get; set; }

        /// The homomorphic commitment representing the output's amount
        public Commitment commit { get; set; }

        /// switch commit hash
        public SwitchCommitHash switch_commit_hash { get; set; }

        /// A proof that the commitment is in the right range
        public RangeProof proof { get; set; }

        /// The height of the block creating this output
        public ulong height { get; set; }

        /// The lock height (earliest block this output can be spent)
        public ulong lock_height { get; set; }


        public static ApiOutput from_output(Output output, BlockHeader block_header, bool include_proof,
            bool include_switch)
        {
            throw new NotImplementedException();

            //          let(output_type, lock_height) = match output.features {
            //              x if x.contains(core::transaction::COINBASE_OUTPUT) => (
            //                  OutputType::Coinbase,
            //                  block_header.height + global::coinbase_maturity(),

            //              ),
            //	_ => (OutputType::Transaction, 0),
            //};

            //          Output {
            //              output_type: output_type,
            //	commit: output.commit,
            //	switch_commit_hash: match include_switch {
            //                  true => Some(output.switch_commit_hash),
            //		false => None,
            //	},
            //	proof: match include_proof {
            //                  true => Some(output.proof),
            //		false => None,
            //	},
            //	height: block_header.height,
            //	lock_height: lock_height,
        }

        public object Clone()
        {
            throw new NotImplementedException();
        }
    }
}