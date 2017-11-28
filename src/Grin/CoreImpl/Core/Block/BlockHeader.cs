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
                version = version,
                height = height,
                previous = previous.Clone(),
                timestamp = timestamp,
                utxo_root = utxo_root.Clone(),
                range_proof_root = range_proof_root.Clone(),
                kernel_root = kernel_root.Clone(),
                nonce = nonce,
                pow = pow.Clone(),
                difficulty = difficulty.Clone(),
                total_difficulty = total_difficulty.Clone()
            };
        }

        /// Version of the block
        public ushort version { get; internal set; }

        /// Height of this block since the genesis block (height 0)
        public ulong height { get; internal set; }

        /// Hash of the block previous to this in the chain.
        public Hash.Hash previous { get; internal set; }

        /// Timestamp at which the block was built.
        public DateTime timestamp { get; internal set; }

        /// Merklish root of all the commitments in the UTXO set
        public Hash.Hash utxo_root { get; internal set; }

        /// Merklish root of all range proofs in the UTXO set
        public Hash.Hash range_proof_root { get; internal set; }

        /// Merklish root of all transaction kernels in the UTXO set
        public Hash.Hash kernel_root { get; internal set; }

        /// Nonce increment used to mine this block.
        public ulong nonce { get; internal set; }

        /// Proof of work data.
        public Proof pow { get; internal set; }

        /// Difficulty used to mine the block.
        public Difficulty difficulty { get; internal set; }

        /// Total accumulated difficulty since genesis block
        public Difficulty total_difficulty { get; internal set; }

        public static BlockHeader New()
        {
            return new BlockHeader();
        }

        public static BlockHeader Default()
        {
            var proofSize = Global.proofsize();

            return new BlockHeader
            {
                version = 1,
                height = 0,
                previous = Hash.Hash.ZERO_HASH(),
                timestamp = DateTime.UtcNow,
                difficulty = Difficulty.From_num(Consensus.MINIMUM_DIFFICULTY),
                total_difficulty = Difficulty.From_num(Consensus.MINIMUM_DIFFICULTY),
                utxo_root = Hash.Hash.ZERO_HASH(),
                range_proof_root = Hash.Hash.ZERO_HASH(),
                kernel_root = Hash.Hash.ZERO_HASH(),
                nonce = 0,
                pow = Proof.Zero(proofSize)
            };
        }

        public void write(IWriter writer)
        {
            writer.write_u16(version);
            writer.write_u64(height);
            previous.write(writer);
            writer.write_i64(timestamp.ToUnixTime());
            utxo_root.write(writer);
            range_proof_root.write(writer);
            kernel_root.write(writer);
            writer.write_u64(nonce);
            difficulty.write(writer);
            total_difficulty.write(writer);

            if (writer.serialization_mode() != SerializationMode.Hash)
            {
                pow.write(writer);
            }
        }

        public void read(IReader reader)
        {
            version = reader.read_u16();
            height = reader.read_u64();
            previous = Hash.Hash.readNew(reader);
            timestamp = reader.read_i64().FromUnixTime();
            utxo_root = Hash.Hash.readNew(reader);
            range_proof_root = Hash.Hash.readNew(reader);
            kernel_root = Hash.Hash.readNew(reader);
            nonce = reader.read_u64();
            difficulty = Difficulty.readnew(reader);
            total_difficulty = Difficulty.readnew(reader);
            pow = Proof.readnew(reader);
        }

        public static BlockHeader readnew(IReader reader)
        {
            var bh = new BlockHeader();
            bh.read(reader);
            return bh;
        }
    }
}