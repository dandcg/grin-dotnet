using System;
using Grin.CoreImpl.Core.Block;
using Grin.CoreImpl.Core.Hash;
using Grin.CoreImpl.Core.Target;
using Grin.CoreImpl.Ser;

namespace Grin.ChainImpl.ChainTypes
{
    /// The tip of a fork. A handle to the fork ancestry from its leaf in the
    /// blockchain tree. References the max height and the latest and previous
    /// blocks
    /// for convenience and the total difficulty.
    public struct ChainTip : IWriteable, IReadable, ICloneable
    {
        /// Height of the tip (max height of the fork)
        public ulong height { get; set; }

        /// Last block pushed to the fork
        public Hash last_block_h { get; set; }

        /// Block previous to last
        public Hash prev_block_h { get; set; }

        /// Total difficulty accumulated on that fork
        public Difficulty total_difficulty { get; set; }

        public object Clone()
        {
            throw new NotImplementedException();
        }

        /// Creates a new tip at height zero and the provided genesis hash.
        public static ChainTip New(Hash gbh)
        {
            return new ChainTip
            {
                height = 0,
                last_block_h = gbh,
                prev_block_h = gbh,
                total_difficulty = Difficulty.One()
            };
        }

        /// Append a new block to this tip, returning a new updated tip.
        public static ChainTip from_block(BlockHeader bh)
        {
            return new ChainTip
            {
                height = bh.height,
                last_block_h = bh.hash(),
                prev_block_h = bh.previous,
                total_difficulty = bh.total_difficulty.Clone()
            };
        }


        /// Serialization of a tip, required to save to datastore.
        public void write(IWriter writer)
        {
            writer.write_u64(height);
            last_block_h.write(writer);
            prev_block_h.write(writer);
            total_difficulty.write(writer);
        }

        public void read(IReader reader)
        {
            height = reader.read_u64();
            last_block_h = Hash.readnew(reader);
            prev_block_h = Hash.readnew(reader);
            total_difficulty = Difficulty.readnew(reader);
        }

        public static ChainTip readnew(IReader reader)
        {
            var tip = new ChainTip();
            tip.read(reader);
            return tip;
        }
    }
}