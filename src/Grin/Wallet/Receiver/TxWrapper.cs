using Newtonsoft.Json;

namespace Grin.Wallet.Receiver
{
    public class TxWrapper
    {
        [JsonProperty("tx_hex")]
        public string tx_hex { get; set; }
    }
}