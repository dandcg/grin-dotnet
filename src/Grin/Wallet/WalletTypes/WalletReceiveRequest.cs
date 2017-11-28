using Newtonsoft.Json;

namespace Grin.Wallet.Types
{
    /// Amount in request to build a coinbase output.
    public class WalletReceiveRequest
    {
        [JsonProperty("coinbase")]
        public BlockFees Coinbase { get; set; }
        [JsonProperty("partialtransaction")]
        public string PartialTransaction { get; set; }
        [JsonProperty("finalize")]
        public string Finalize { get; set; }
    }
}