using System;
using Grin.CoreImpl.Core.Block;

namespace Grin.Api.ApiTypes
{
    public class ApiBlockHeaderInfo : ICloneable
    {
        /// Hash
        public string hash { get; set; }

        /// Previous block hash
        public string previous { get; set; }

        /// Height
        public ulong height { get; set; }


        public static ApiBlockHeaderInfo from_header(BlockHeader block_header)
        {
            throw new NotImplementedException();

            //          BlockHeaderInfo {
            //              hash: util::to_hex(block_header.hash().to_vec()),
            //	previous: util::to_hex(block_header.previous.to_vec()),
            //	height: block_header.height,
            //}
        }

        public object Clone()
        {
            throw new NotImplementedException();
        }
    }
}