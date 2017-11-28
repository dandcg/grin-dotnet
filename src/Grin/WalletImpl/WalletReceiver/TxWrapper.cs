using Newtonsoft.Json;

namespace Grin.WalletImpl.WalletReceiver
{
    public class TxWrapper
    {
        [JsonProperty("tx_hex")]
        public string tx_hex { get; set; }
    }
}