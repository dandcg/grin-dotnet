using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Grin.WalletImpl.WalletTypes
{
    /// Response to build a coinbase output.
    public class CbData
    {
        public CbData(string output, string kernel, string keyId)
        {
            Output = output;
            Kernel = kernel;
            KeyId = keyId;
        }
        [JsonProperty(PropertyName = "output")]
        public string Output { get; }
        [JsonProperty(PropertyName = "kernel")]
        public string Kernel { get; }
        [JsonProperty(PropertyName = "key_id")]
        public string KeyId { get; }
    }
}