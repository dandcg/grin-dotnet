using System;
using Grin.CoreImpl.Core.Block;
using Grin.CoreImpl.Core.Transaction;
using Secp256k1Proxy.Pedersen;

namespace Grin.ApiImpl.ApiTypes
{
    public struct ApiOutput : ICloneable
    {
        /// The type of output Coinbase|Transaction
        public ApiOutputType OutputType { get; set; }

        /// The homomorphic commitment representing the output's amount
        public Commitment Commit { get; set; }

        /// switch commit hash
        public SwitchCommitHash SwitchCommitHash { get; set; }

        /// A proof that the commitment is in the right range
        public RangeProof Proof { get; set; }

        /// The height of the block creating this output
        public ulong Height { get; set; }

        /// The lock height (earliest block this output can be spent)
        public ulong LockHeight { get; set; }


        public static ApiOutput from_output(Output output, BlockHeader blockHeader, bool includeProof,
            bool includeSwitch)
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