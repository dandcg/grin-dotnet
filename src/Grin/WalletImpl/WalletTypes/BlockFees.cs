using Common;
using Grin.KeychainImpl.ExtKey;
using Newtonsoft.Json;

namespace Grin.WalletImpl.WalletTypes
{
    public class BlockFees:ICloneable<BlockFees>
    {
        [JsonProperty("fees")]
        public ulong Fees { get; private set; }

        [JsonProperty("height")]
        public ulong Height { get; private set; }

        [JsonProperty("key_id")]
        public Identifier KeyId { get; private set; }


        public void Key_id_set(Identifier keyId)
        {
            KeyId = keyId;
        }

        public BlockFees Clone()
        {
            return new BlockFees {Fees = Fees, Height = Height, KeyId = Key_id_clone()};
        }

        public Identifier Key_id_clone()
        {
            return KeyId?.Clone();
        }
    }
}