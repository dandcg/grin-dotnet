using System.Linq;
using Grin.Core.Ser;
using Secp256k1Proxy;
using Secp256k1Proxy.Lib;
using Secp256k1Proxy.Pedersen;

namespace Grin.Core.Core.Transaction
{
    /// A proof that a transaction sums to zero. Includes both the transaction's
    /// Pedersen commitment and the signature, that guarantees that the commitments
    /// amount to zero.
    /// The signature signs the fee and the lock_height, which are retained for
    /// signature validation.
    public class TxKernel : IWriteable, IReadable
    {
        /// Options for a kernel's structure or use
        public KernelFeatures features { get; set; }

        /// Fee originally included in the transaction this proof is for.
        public ulong fee { get; set; }

        /// This kernel is not valid earlier than lock_height blocks
        /// The max lock_height of all *inputs* to this transaction
        public ulong lock_height { get; set; }

        /// Remainder of the sum of all transaction commitments. If the transaction
        /// is well formed, amounts components should sum to zero and the excess
        /// is hence a valid public key.
        public Commitment excess { get; set; }

        /// The signature proving the excess is a valid public key, which signs
        /// the transaction fee.
        public byte[] excess_sig { get; set; }


        /// Verify the transaction proof validity. Entails handling the commitment
        /// as a public key and checking the signature verifies with the fee as
        /// message.
        public void verify(Secp256k1 secp)
        {
            var msg = Message.from_slice(TransactionHelper.kernel_sig_msg(fee, lock_height));
            var sig = Signiture.from_der(secp, excess_sig);
            secp.verify_from_commit(msg, sig, excess);
        }

        public void write(IWriter writer)
        {
            writer.write_u8((byte) features);
            writer.write_u64(fee);
            writer.write_u64(lock_height);
            excess.WriteCommitment(writer);
            writer.write_bytes(excess_sig);
        }

        public void read(IReader reader)
        {
            features = (KernelFeatures) reader.read_u8();
            fee = reader.read_u64();
            lock_height = reader.read_u64();
            excess = Ser.Ser.ReadCommitment(reader);
            excess_sig = reader.read_vec();
        }

        public TxKernel Clone()
        {
            return new TxKernel()
            {
                excess =excess.Clone(),
                excess_sig = excess_sig.ToArray(),
                features = features,
                fee=fee,
                lock_height = lock_height


            };
        }
    }
}