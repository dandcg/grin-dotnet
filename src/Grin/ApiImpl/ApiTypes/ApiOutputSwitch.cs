using System;
using Grin.CoreImpl.Core.Block;
using Grin.CoreImpl.Core.Transaction;

namespace Grin.Api.ApiTypes
{
    public class ApiOutputSwitch : ICloneable
    {
        /// the commit
        public string commit { get; set; }

        /// switch commit hash
        public byte[] switch_commit_hash { get; set; } //[u8; core::SWITCH_COMMIT_HASH_SIZE],

        /// The height of the block creating this output
        public ulong height { get; set; }


        public static ApiOutputSwitch from_output(Output output, BlockHeader block_header)
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