using System.Linq;
using Grin.CoreImpl.Ser;
using Secp256k1Proxy.Lib;
using Secp256k1Proxy.Pedersen;

namespace Grin.CoreImpl.Core.Transaction
{
    /// A proof that a transaction sums to zero. Includes both the transaction's
    /// Pedersen commitment and the signature, that guarantees that the commitments
    /// amount to zero.
    /// The signature signs the fee and the lock_height, which are retained for
    /// signature validation.
    public class TxKernel : IWriteable, IReadable
    {
        /// Options for a kernel's structure or use
        public KernelFeatures Features { get; set; }

        /// Fee originally included in the transaction this proof is for.
        public ulong Fee { get; set; }

        /// This kernel is not valid earlier than lock_height blocks
        /// The max lock_height of all *inputs* to this transaction
        public ulong LockHeight { get; set; }

        /// Remainder of the sum of all transaction commitments. If the transaction
        /// is well formed, amounts components should sum to zero and the excess
        /// is hence a valid public key.
        public Commitment Excess { get; set; }

        /// The signature proving the excess is a valid public key, which signs
        /// the transaction fee.
        public byte[] ExcessSig { get; set; }


        /// Verify the transaction proof validity. Entails handling the commitment
        /// as a public key and checking the signature verifies with the fee as
        /// message.
        public void Verify(Secp256K1 secp)
        {
            var msg = Message.from_slice(TransactionHelper.kernel_sig_msg(Fee, LockHeight));
            var sig = Signiture.from_der(secp, ExcessSig);
            secp.verify_from_commit(msg, sig, Excess);
        }

        public void Write(IWriter writer)
        {
            writer.write_u8((byte) Features);
            writer.write_u64(Fee);
            writer.write_u64(LockHeight);
            Excess.WriteCommitment(writer);
            writer.write_bytes(ExcessSig);
        }

        public void Read(IReader reader)
        {
            Features = (KernelFeatures) reader.read_u8();
            Fee = reader.read_u64();
            LockHeight = reader.read_u64();
            Excess = Ser.Ser.ReadCommitment(reader);
            ExcessSig = reader.read_vec();
        }

        public TxKernel Clone()
        {
            return new TxKernel()
            {
                Excess =Excess.Clone(),
                ExcessSig = ExcessSig.ToArray(),
                Features = Features,
                Fee=Fee,
                LockHeight = LockHeight


            };
        }
    }
}