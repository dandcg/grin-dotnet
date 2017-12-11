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
        public ulong Amount { get; set; }
        [JsonProperty("blind_sum")]
        public string BlindSum { get; set; }
        [JsonProperty("tx")]
        public string Tx { get; set; }


        /// Builds a PartialTx from data sent by a sender (not yet completed by the receiver).
        public static PartialTx build_partial_tx(
            ulong receiveAmount,
            BlindingFactor blindSum,
            Transaction transaction
        )
        {
            return new PartialTx
            {
                Amount = receiveAmount,
                BlindSum = HexUtil.to_hex(blindSum.Key.Value),
                Tx = HexUtil.to_hex(Ser.Ser_vec(transaction))
            };
        }

        /// Reads a partial transaction into the amount, sum of blinding
        /// factors and the transaction itself.
        public static (ulong amount, BlindingFactor blinding, Transaction tx) read_partial_tx(
            Keychain keychain,
            PartialTx partialTx
        )
        {
            var blindBin = HexUtil.from_hex(partialTx.BlindSum);
            var blinding = BlindingFactor.from_slice(keychain.Secp, blindBin);
            var txBin = HexUtil.from_hex(partialTx.Tx);

            Transaction transaction;
            using (var ms = new MemoryStream(txBin))
            {

                transaction = Ser.Deserialize(ms, Transaction.Empty());
               ;
            }

            return (partialTx.Amount, blinding, transaction);
        }
    }
}