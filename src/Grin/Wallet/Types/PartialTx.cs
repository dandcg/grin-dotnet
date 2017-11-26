using System.IO;
using Common;
using Grin.Core;
using Grin.Core.Core;
using Grin.Keychain;
using Newtonsoft.Json;

namespace Grin.Wallet
{
    public class PartialTx
    {
        public ulong amount { get; private set; }
        public string blind_sum { get; private set; }
        public string tx { get; private set; }

        /// Encodes the information for a partial transaction (not yet completed by the
        /// receiver) into JSON.
        public static string partial_tx_to_json(ulong receive_amount,
            BlindingFactor blind_sum,
            Transaction tx)
        {
            var partial_tx = new PartialTx
            {
                amount = receive_amount,
                blind_sum = HexUtil.to_hex(blind_sum.Key.Value),
                tx = HexUtil.to_hex(Ser.ser_vec(tx))
            };

            return"";

//            partial_tx
        }

        /// Reads a partial transaction encoded as JSON into the amount, sum of blinding
        /// factors and the transaction itself.
        public (ulong, BlindingFactor, Transaction) partial_tx_from_json(Keychain.Keychain keychain, string json_str)
        {
            var partial_tx = JsonConvert.DeserializeObject<PartialTx>(json_str);
            
            var blind_bin = HexUtil.from_hex(partial_tx.blind_sum);
            
            // TODO - turn some data into a blinding factor here somehow
            var blinding = BlindingFactor.from_slice(keychain.Secp, blind_bin);
            
            var tx_bin = HexUtil.from_hex(partial_tx.tx);
            Stream stream = new MemoryStream(tx_bin);
            
            var tx = Ser.deserialize(stream, Transaction.Empty());

            return (partial_tx.amount, blinding, tx);
        }
    }
}