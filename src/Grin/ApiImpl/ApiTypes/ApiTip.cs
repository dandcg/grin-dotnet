using System;
using Common;
using Grin.ChainImpl.ChainTypes;

namespace Grin.Api.ApiTypes
{
    /// The state of the current fork tip
    public class ApiTip : ICloneable
    {
        /// Height of the tip (max height of the fork)
        public ulong height { get; set; }

        // Last block pushed to the fork
        public string last_block_pushed { get; set; }

        // Block previous to last
        public string prev_block_to_last { get; set; }

        // Total difficulty accumulated on that fork
        public ulong total_difficulty { get; set; }


        public static ApiTip from_tip(ChainTip tip)
        {
            return new ApiTip
            {
                height = tip.height,
                last_block_pushed = HexUtil.to_hex(tip.last_block_h.Value),
                prev_block_to_last = HexUtil.to_hex(tip.prev_block_h.Value),
                total_difficulty = tip.total_difficulty.into_num()
            };
        }


        public object Clone()
        {
            return new ApiTip
            {
                height = height,
                last_block_pushed = last_block_pushed,
                prev_block_to_last = prev_block_to_last,
                total_difficulty = total_difficulty
            };
        }
    }


    // As above, except formatted a bit better for human viewing

    // As above, except just the info needed for wallet reconstruction

    // Just the information required for wallet reconstruction

// For wallet reconstruction, include the header info along with the
// transactions in the block
}