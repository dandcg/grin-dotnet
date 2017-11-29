namespace Grin.CoreImpl.Core.Block
{
    /// Errors thrown by Block validation

    public enum BlockError
    {
        /// The sum of output minus input commitments does not match the sum of
        /// kernel commitments
        KernelSumMismatch,
        /// Same as above but for the coinbase part of a block, including reward
        CoinbaseSumMismatch,
        /// Kernel fee can't be odd, due to half fee burning
        OddKernelFee,
        /// Too many inputs, outputs or kernels in the block
        WeightExceeded,
        /// Kernel not valid due to lock_height exceeding block header height
        KernelLockHeight, 
        /// The lock_height causing this validation error
        lock_height,
        /// Underlying tx related error
        Transaction,
        /// Underlying Secp256k1 error (signature validation or invalid public key typically)
        Secp,
        /// Underlying keychain related error
        Keychain,
    }
}