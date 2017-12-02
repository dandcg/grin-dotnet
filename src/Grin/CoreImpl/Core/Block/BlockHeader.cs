using System;
using Common;
using Grin.CoreImpl.Core.Mod;
using Grin.CoreImpl.Core.Target;
using Grin.CoreImpl.Ser;

namespace Grin.CoreImpl.Core.Block
{
    public class BlockHeader : IWriteable, IReadable


    {
        private BlockHeader()
        {
        }

        public BlockHeader Clone()
        {
            return new BlockHeader
            {
                Version = Version,
                Height = Height,
                Previous = Previous.Clone(),
                Timestamp = Timestamp,
                UtxoRoot = UtxoRoot.Clone(),
                RangeProofRoot = RangeProofRoot.Clone(),
                KernelRoot = KernelRoot.Clone(),
                Nonce = Nonce,
                Pow = Pow.Clone(),
                Difficulty = Difficulty.Clone(),
                TotalDifficulty = TotalDifficulty.Clone()
            };
        }

        /// Version of the block
        public ushort Version { get; internal set; }

        /// Height of this block since the genesis block (height 0)
        public ulong Height { get; internal set; }

        /// Hash of the block previous to this in the chain.
        public Hash.Hash Previous { get; internal set; }

        /// Timestamp at which the block was built.
        public DateTime Timestamp { get; internal set; }

        /// Merklish root of all the commitments in the UTXO set
        public Hash.Hash UtxoRoot { get; internal set; }

        /// Merklish root of all range proofs in the UTXO set
        public Hash.Hash RangeProofRoot { get; internal set; }

        /// Merklish root of all transaction kernels in the UTXO set
        public Hash.Hash KernelRoot { get; internal set; }

        /// Nonce increment used to mine this block.
        public ulong Nonce { get; internal set; }

        /// Proof of work data.
        public Proof Pow { get; internal set; }

        /// Difficulty used to mine the block.
        public Difficulty Difficulty { get; internal set; }

        /// Total accumulated difficulty since genesis block
        public Difficulty TotalDifficulty { get; internal set; }

        public static BlockHeader New()
        {
            return new BlockHeader();
        }

        public static BlockHeader Default()
        {
            var proofSize = Global.Proofsize();

            return new BlockHeader
            {
                Version = 1,
                Height = 0,
                Previous = Hash.Hash.ZERO_HASH(),
                Timestamp = DateTime.UtcNow.PrecisionSeconds(),
                Difficulty = Difficulty.From_num(Consensus.MinimumDifficulty),
                TotalDifficulty = Difficulty.From_num(Consensus.MinimumDifficulty),
                UtxoRoot = Hash.Hash.ZERO_HASH(),
                RangeProofRoot = Hash.Hash.ZERO_HASH(),
                KernelRoot = Hash.Hash.ZERO_HASH(),
                Nonce = 0,
                Pow = Proof.Zero(proofSize)
            };
        }

        public void Write(IWriter writer)
        {
            writer.write_u16(Version);
            writer.write_u64(Height);
            Previous.Write(writer);
            writer.write_i64(Timestamp.ToUnixTime());
            UtxoRoot.Write(writer);
            RangeProofRoot.Write(writer);
            KernelRoot.Write(writer);
            writer.write_u64(Nonce);
            Difficulty.Write(writer);
            TotalDifficulty.Write(writer);

            if (writer.serialization_mode() != SerializationMode.Hash)
            {
                Pow.Write(writer);
            }

         
        }

        public void Read(IReader reader)
        {
            Version = reader.read_u16();
            Height = reader.read_u64();
            Previous = Hash.Hash.Readnew(reader);
            Timestamp = reader.read_i64().FromUnixTime();
            UtxoRoot = Hash.Hash.Readnew(reader);
            RangeProofRoot = Hash.Hash.Readnew(reader);
            KernelRoot = Hash.Hash.Readnew(reader);
            Nonce = reader.read_u64();
            Difficulty = Difficulty.Readnew(reader);
            TotalDifficulty = Difficulty.Readnew(reader);
            Pow = Proof.Readnew(reader);

          

        }

        public static BlockHeader Readnew(IReader reader)
        {
            var bh = new BlockHeader();
            bh.Read(reader);
            return bh;
        }
    }
}