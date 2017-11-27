using Grin.Keychain.ExtKey;
using Newtonsoft.Json;

namespace Grin.Wallet.Types
{
    public class BlockFees
    {
        public BlockFees()
        {
            
        }

        [JsonProperty]
        public ulong fees { get; private set; }

        [JsonProperty]
        public ulong height { get; private set; }

        [JsonProperty]
        public Identifier key_id { get; private set; }


        public void key_id_set(Identifier keyId)
        {
            key_id = keyId;
        }

        public BlockFees Clone()
        {
            return new BlockFees {fees = fees, height = height, key_id = key_id_clone()};
        }

        public Identifier key_id_clone()
        {
            return key_id?.Clone();
        }
    }
}