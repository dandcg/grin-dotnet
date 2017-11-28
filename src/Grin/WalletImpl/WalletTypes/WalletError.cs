namespace Grin.WalletImpl.WalletTypes
{
    /// Wallet errors, mostly wrappers around underlying crypto or I/O errors.
    public enum WalletError
    {
        NotEnoughFunds, //(u64),

        FeeDispute, //{sender_fee: u64, recipient_fee: u64

        Keychain, //(keychain::Error),

        Transaction, //(transaction::Error),

        Secp, //(secp::Error),

        WalletData, //(String),

        /// An error in the format of the JSON structures exchanged by the wallet
        Format, //(String),

        /// An IO Error
        IOError, //(io::Error),

        /// Error when contacting a node through its API
        Node, //(api::Error),

        /// Error originating from hyper.
        Hyper, //(hyper::Error),

        /// Error originating from hyper uri parsing.
        Uri //(hyper::error::UriError),
    }
}