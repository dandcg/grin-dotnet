﻿using System;
using Grin.Core.Core.Block;
using Grin.Core.Core.Hash;
using Grin.Core.Core.Target;
using Grin.Core.Ser;
using Secp256k1Proxy.Pedersen;

namespace Grin.Chain
{
    /// Options for block validation
    public enum ChainOptions : uint

    {
        /// None flag
        NONE = 0b00000001,

        /// Runs without checking the Proof of Work, mostly to make testing easier.
        SKIP_POW = 0b00000010,

        /// Adds block while in syncing mode.
        SYNC = 0b00001000
    }


    public class ChainErrorException:ApplicationException
    {
        public ChainErrorException(ChainError error) : base(error.ToString())
        {
  
        }

        public ChainErrorException(ChainError error, Exception innerException) : base(error.ToString(), innerException)
        {
 

        }
    }



   /// Errors

public enum ChainError
    {
        /// The block doesn't fit anywhere in our chain
        Unfit,
        /// Special case of orphan blocks
        Orphan,
        /// Difficulty is too low either compared to ours or the block PoW hash
        DifficultyTooLow,
        /// Addition of difficulties on all previous block is wrong
        WrongTotalDifficulty,
        /// The proof of work is invalid
        InvalidPow,
        /// The block doesn't sum correctly or a tx signature is invalid
        InvalidBlockProof,
        /// Block time is too old
        InvalidBlockTime,
        /// Block height is invalid (not previous + 1)
        InvalidBlockHeight,
        /// One of the root hashes in the block is invalid
        InvalidRoot,
        /// One of the inputs in the block has already been spent
        AlreadySpent,
        /// An output with that commitment already exists (should be unique)
        DuplicateCommitment,
        /// A kernel with that excess commitment already exists (should be unique)
        DuplicateKernel,
        /// coinbase can only be spent after it has matured (n blocks)
        ImmatureCoinbase,
        /// output not found
        OutputNotFound,
        /// output spent
        OutputSpent,
        /// Invalid block version, either a mistake or outdated software
        InvalidBlockVersion,
        /// Internal issue when trying to save or load data from store
        StoreErr,
        /// Error serializing or deserializing a type
        SerErr,
        /// Error while updating the sum trees
        SumTreeErr,
        /// No chain exists and genesis block is required
        GenesisBlockRequired,
        /// Anything else
        Other
    }

    //impl From<grin_store::Error> for Error {
    //    fn from(e: grin_store::Error) -> Error {
    //        Error::StoreErr(e, "wrapped".to_owned())

    //    }
    //}
    //impl From<ser::Error> for Error {
    //    fn from(e: ser::Error) -> Error {
    //        Error::SerErr(e)

    //    }
    //}
    //impl From<io::Error> for Error {
    //    fn from(e: io::Error) -> Error {
    //        Error::SumTreeErr(e.to_string())

    //    }


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
            last_block_h = Hash.readNew(reader);
            prev_block_h = Hash.readNew(reader);
            total_difficulty = Difficulty.readnew(reader);
        }

        public static ChainTip readnew(IReader reader)
        {
            var tip = new ChainTip();
            tip.read(reader);
            return tip;
        }
    }


///// Trait the chain pipeline requires an implementor for in order to process
///// blocks.
//            pub trait ChainStore: Send + Sync {
//                /// Get the tip that's also the head of the chain
//                fn head(&self) -> Result<Tip, store::Error>;

//                /// Block header for the chain head
//                fn head_header(&self) -> Result<BlockHeader, store::Error>;

//                /// Save the provided tip as the current head of our chain
//                fn save_head(&self, t: &Tip) -> Result < (), store::Error >;

//                /// Save the provided tip as the current head of the body chain, leaving the
//                /// header chain alone.
//                fn save_body_head(&self, t: &Tip) -> Result < (), store::Error >;

//                /// Gets a block header by hash
//                fn get_block(&self, h: &Hash) -> Result<Block, store::Error>;

//                /// Gets a block header by hash
//                fn get_block_header(&self, h: &Hash) -> Result<BlockHeader, store::Error>;

//                /// Checks whether a block has been been processed and saved
//                fn check_block_exists(&self, h: &Hash) -> Result<bool, store::Error>;

//                /// Save the provided block in store
//                fn save_block(&self, b: &Block) -> Result < (), store::Error >;

//                /// Save the provided block header in store
//                fn save_block_header(&self, bh: &BlockHeader) -> Result < (), store::Error >;

//                /// Get the tip of the header chain
//                fn get_header_head(&self) -> Result<Tip, store::Error>;

//                /// Save the provided tip as the current head of the block header chain
//                fn save_header_head(&self, t: &Tip) -> Result < (), store::Error >;

//                /// Gets the block header at the provided height
//                fn get_header_by_height(&self, height: u64) -> Result<BlockHeader, store::Error>;

//                /// Gets an output by its commitment
//                fn get_output_by_commit(&self, commit: &Commitment) -> Result<Output, store::Error>;

//                /// Gets a block_header for the given input commit
//                fn get_block_header_by_output_commit(

//        &self,
//		commit: &Commitment,
//	) -> Result<BlockHeader, store::Error>;

//                /// Saves the position of an output, represented by its commitment, in the
//                /// UTXO MMR. Used as an index for spending and pruning.
//                fn save_output_pos(&self, commit: &Commitment, pos: u64) -> Result < (), store::Error >;

//                /// Gets the position of an output, represented by its commitment, in the
//                /// UTXO MMR. Used as an index for spending and pruning.
//                fn get_output_pos(&self, commit: &Commitment) -> Result<u64, store::Error>;

//                /// Saves the position of a kernel, represented by its excess, in the
//                /// UTXO MMR. Used as an index for spending and pruning.
//                fn save_kernel_pos(&self, commit: &Commitment, pos: u64) -> Result < (), store::Error >;

//                /// Gets the position of a kernel, represented by its excess, in the
//                /// UTXO MMR. Used as an index for spending and pruning.
//                fn get_kernel_pos(&self, commit: &Commitment) -> Result<u64, store::Error>;

//                /// Saves the provided block header at the corresponding height. Also check
//                /// the consistency of the height chain in store by assuring previous
//                /// headers are also at their respective heights.
//                fn setup_height(&self, bh: &BlockHeader) -> Result < (), store::Error >;
//            }

//            /// Bridge between the chain pipeline and the rest of the system. Handles
//            /// downstream processing of valid blocks by the rest of the system, most
//            /// importantly the broadcasting of blocks to our peers.
//            pub trait ChainAdapter {
//                /// The blockchain pipeline has accepted this block as valid and added
//                /// it to our chain.
//                fn block_accepted(&self, b: &Block);
//            }

//            /// Dummy adapter used as a placeholder for real implementations
//            pub struct NoopAdapter { }
//impl ChainAdapter for NoopAdapter {
//	fn block_accepted(&self, _: &Block) { }
//}
}