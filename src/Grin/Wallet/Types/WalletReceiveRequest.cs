namespace Grin.Wallet.Types
{
    /// Amount in request to build a coinbase output.
    public class WalletReceiveRequest
    {
        public BlockFees Coinbase { get; set; }
        public string PartialTransaction { get; set; }
        public string Finalize { get; set; }
    }
}