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
        public ulong Height { get; set; }

        /// Last block pushed to the fork
        public Hash LastBlockH { get; set; }

        /// Block previous to last
        public Hash PrevBlockH { get; set; }

        /// Total difficulty accumulated on that fork
        public Difficulty TotalDifficulty { get; set; }

        public object Clone()
        {
            throw new NotImplementedException();
        }

        /// Creates a new tip at height zero and the provided genesis hash.
        public static ChainTip New(Hash gbh)
        {
            return new ChainTip
            {
                Height = 0,
                LastBlockH = gbh,
                PrevBlockH = gbh,
                TotalDifficulty = Difficulty.One()
            };
        }

        /// Append a new block to this tip, returning a new updated tip.
        public static ChainTip from_block(BlockHeader bh)
        {
            return new ChainTip
            {
                Height = bh.Height,
                LastBlockH = bh.Hash(),
                PrevBlockH = bh.Previous,
                TotalDifficulty = bh.TotalDifficulty.Clone()
            };
        }


        /// Serialization of a tip, required to save to datastore.
        public void Write(IWriter writer)
        {
            writer.write_u64(Height);
            LastBlockH.Write(writer);
            PrevBlockH.Write(writer);
            TotalDifficulty.Write(writer);
        }

        public void Read(IReader reader)
        {
            Height = reader.read_u64();
            LastBlockH = Hash.Readnew(reader);
            PrevBlockH = Hash.Readnew(reader);
            TotalDifficulty = Difficulty.Readnew(reader);
        }

        public static ChainTip Readnew(IReader reader)
        {
            var tip = new ChainTip();
            tip.Read(reader);
            return tip;
        }
    }
}