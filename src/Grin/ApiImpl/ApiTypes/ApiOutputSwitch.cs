using System;
using Grin.CoreImpl.Core.Block;
using Grin.CoreImpl.Core.Transaction;

namespace Grin.ApiImpl.ApiTypes
{
    public class ApiOutputSwitch : ICloneable
    {
        /// the commit
        public string Commit { get; set; }

        /// switch commit hash
        public byte[] SwitchCommitHash { get; set; } //[u8; core::SWITCH_COMMIT_HASH_SIZE],

        /// The height of the block creating this output
        public ulong Height { get; set; }


        public static ApiOutputSwitch from_output(Output output, BlockHeader blockHeader)
        {
            throw new NotImplementedException();

            //          OutputSwitch {
            //              commit: util::to_hex(output.commit.0.to_vec()),
            //	switch_commit_hash: output.switch_commit_hash.hash,
            //	height: block_header.height,
            //}
        }

        public object Clone()
        {
            throw new NotImplementedException();
        }
    }
}