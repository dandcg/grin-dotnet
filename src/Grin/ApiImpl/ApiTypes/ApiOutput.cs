using System;
using Grin.CoreImpl.Core.Block;
using Grin.CoreImpl.Core.Transaction;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Secp256k1Proxy.Pedersen;

namespace Grin.ApiImpl.ApiTypes
{
    public struct ApiOutput : ICloneable
    {

        /// The type of output Coinbase|Transaction
        [JsonProperty(PropertyName = "output_type")]
        public string OutputType { get; set; }

        /// The homomorphic commitment representing the output's amount
        [JsonProperty(PropertyName = "commit")]
        public byte[] Commit { get; set; }

        /// switch commit hash
        [JsonProperty(PropertyName = "switch_commit_hash")]
        public byte[] SwitchCommitHash { get; set; }


        /// A proof that the commitment is in the right range
        [JsonProperty(PropertyName = "proof")]
        public byte[] Proof { get; set; }

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