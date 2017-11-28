namespace Grin.ChainImpl.ChainTypes
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
}