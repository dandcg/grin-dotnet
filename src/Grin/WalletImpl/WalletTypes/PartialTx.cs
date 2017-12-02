using System.IO;
using Common;
using Grin.CoreImpl.Core.Transaction;
using Grin.CoreImpl.Ser;
using Grin.KeychainImpl;
using Grin.KeychainImpl.Blind;
using Newtonsoft.Json;

namespace Grin.WalletImpl.WalletTypes
{
    /// Helper in serializing the information a receiver requires to build a
    /// transaction.
    public class PartialTx
    {
        [JsonProperty("amount")]
        public ulong amount { get; set; }
        [JsonProperty("blind_sum")]
        public string blind_sum { get; set; }
        [JsonProperty("tx")]
        public string tx { get; set; }


        /// Builds a PartialTx from data sent by a sender (not yet completed by the receiver).
        public static PartialTx build_partial_tx(
            ulong receiveAmount,
            BlindingFactor blindSum,
            Transaction transaction
        )
        {
            return new PartialTx
            {
                amount = receiveAmount,
                blind_sum = HexUtil.to_hex(blindSum.Key.Value),
                tx = HexUtil.to_hex(Ser.Ser_vec(transaction))
            };
        }

        /// Reads a partial transaction into the amount, sum of blinding
        /// factors and the transaction itself.
        public static (ulong amount, BlindingFactor blinding, Transaction tx) read_partial_tx(
            Keychain keychain,
            PartialTx partial_tx
        )
        {
            var blind_bin = HexUtil.from_hex(partial_tx.blind_sum);
            var blinding = BlindingFactor.from_slice(keychain.Secp, blind_bin);
            var tx_bin = HexUtil.from_hex(partial_tx.tx);

            using (var ms = new MemoryStream(tx_bin))
            {
                var transaction = Ser.Deserialize(ms, Transaction.Empty());

                //  Error::Format("Could not deserialize transaction, invalid format.".to_string())


                return (partial_tx.amount, blinding, transaction);
            }
        }
    }
}