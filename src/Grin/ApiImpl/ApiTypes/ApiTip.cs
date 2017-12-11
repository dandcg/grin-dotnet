using System;
using Common;
using Grin.ChainImpl.ChainTypes;
using Newtonsoft.Json;

namespace Grin.ApiImpl.ApiTypes
{
    /// The state of the current fork tip
    public class ApiTip : ICloneable
    {
        /// Height of the tip (max height of the fork)
        [JsonProperty(PropertyName = "height")]
        public ulong Height { get; set; }

        // Last block pushed to the fork
        [JsonProperty(PropertyName = "last_block_pushed")]
        public string LastBlockPushed { get; set; }

        // Block previous to last
        [JsonProperty(PropertyName = "prev_block_to_last")]
        public string PrevBlockToLast { get; set; }

        // Total difficulty accumulated on that fork
        [JsonProperty(PropertyName = "total_difficulty")]
        public ulong TotalDifficulty { get; set; }


        public static ApiTip from_tip(ChainTip tip)
        {
            return new ApiTip
            {
                Height = tip.Height,
                LastBlockPushed = HexUtil.to_hex(tip.LastBlockH.Value),
                PrevBlockToLast = HexUtil.to_hex(tip.PrevBlockH.Value),
                TotalDifficulty = tip.TotalDifficulty.into_num()
            };
        }


        public object Clone()
        {
            return new ApiTip
            {
                Height = Height,
                LastBlockPushed = LastBlockPushed,
                PrevBlockToLast = PrevBlockToLast,
                TotalDifficulty = TotalDifficulty
            };
        }
    }


    // As above, except formatted a bit better for human viewing

    // As above, except just the info needed for wallet reconstruction

    // Just the information required for wallet reconstruction

// For wallet reconstruction, include the header info along with the
// transactions in the block
}