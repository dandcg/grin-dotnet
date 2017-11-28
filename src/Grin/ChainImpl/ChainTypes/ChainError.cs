namespace Grin.ChainImpl.ChainTypes
{
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
}