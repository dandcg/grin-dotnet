using System.IO;
using Common;
using Grin.Core.Core.Transaction;
using Grin.Core.Ser;
using Grin.Keychain.Blind;
using Microsoft.Azure.KeyVault.Models;
using Newtonsoft.Json;

namespace Grin.Wallet.Types
{

    /// Helper in serializing the information a receiver requires to build a
    /// transaction.
    public class PartialTx
    {

        public ulong amount { get;  set; }
        public string blind_sum { get; set; }
        public string tx { get;  set; }


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
                tx = HexUtil.to_hex(Ser.ser_vec(transaction))
            };
        }

        /// Reads a partial transaction into the amount, sum of blinding
        /// factors and the transaction itself.
        public static (ulong amount, BlindingFactor blinding, Transaction tx) read_partial_tx(
            Keychain.Keychain.Keychain keychain,
            PartialTx partial_tx
        )
        {
            var blind_bin = HexUtil.from_hex(partial_tx.blind_sum);
            var blinding = BlindingFactor.from_slice(keychain.Secp, blind_bin);
            var tx_bin = HexUtil.from_hex(partial_tx.tx);

            using (var ms = new MemoryStream(tx_bin))
            {
                var transaction = Ser.deserialize(ms, Transaction.Empty());

                //  Error::Format("Could not deserialize transaction, invalid format.".to_string())


                return (partial_tx.amount, blinding, transaction);

            }
        }
    }
}