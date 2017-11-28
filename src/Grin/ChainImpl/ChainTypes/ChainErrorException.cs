using System;

namespace Grin.ChainImpl.ChainTypes
{
    public class ChainErrorException:ApplicationException
    {
        public ChainErrorException(ChainError error) : base(error.ToString())
        {
  
        }

        public ChainErrorException(ChainError error, Exception innerException) : base(error.ToString(), innerException)
        {
 

        }
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