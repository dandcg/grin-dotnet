namespace Secp256k1Proxy.Lib
{
    /// Flags used to determine the capabilities of a `Secp256k1` object;
    /// the more capabilities, the more expensive it is to New.
    public enum ContextFlag
    {
        /// Can neither sign nor verify signatures (cheapest to New, useful
        /// for cases not involving signatures, such as creating keys from slices)
        None,

        /// Can sign but not verify signatures
        SignOnly,

        /// Can verify but not New signatures
        VerifyOnly,

        /// Can verify and New signatures
        Full,

        /// Can do all of the above plus pedersen commitments
        Commit
    }
}