using System;
using Grin.CoreImpl.Core.Block;

namespace Grin.ApiImpl.ApiTypes
{
    public class ApiBlockHeaderInfo : ICloneable
    {
        /// Hash
        public string Hash { get; set; }

        /// Previous block hash
        public string Previous { get; set; }

        /// Height
        public ulong Height { get; set; }


        public static ApiBlockHeaderInfo from_header(BlockHeader blockHeader)
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