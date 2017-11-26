using System.Linq;
using Grin.Wallet;
using Secp256k1Proxy;
using Serilog;

namespace Grin.Core.Core
{
    /// A transaction
    public class Transaction : IWriteable, IReadable, ICommitted
    {
        private Transaction()
        {
        }

        /// Set of inputs spent by the transaction.
        public Input[] inputs { get; private set; }

        /// Set of outputs the transaction produces.
        public Output[] outputs { get; private set; }

        /// Fee paid by the transaction.
        public ulong fee { get; private set; }

        /// Transaction is not valid before this block height.
        /// It is invalid for this to be less than the lock_height of any UTXO being spent.
        public ulong lock_height { get; private set; }

        /// The signature proving the excess is a valid public key, which signs
        /// the transaction fee.
        public byte[] excess_sig { get; set; }

        /// Creates a new empty transaction (no inputs or outputs, zero fee).
        public static Transaction Empty()
        {
            return new Transaction
            {
                inputs = new Input[] { },
                outputs = new Output[] { },
                fee = 0,
                lock_height = 0,
                excess_sig = new byte[] { }
            };
        }

        /// Creates a new transaction initialized with
        /// the provided inputs, outputs, fee and lock_height.
        public static Transaction New()
        {
            return new Transaction
            {
                inputs = new Input[] { },
                outputs = new Output[] { },
                fee = 0,
                lock_height = 0,
                excess_sig = new byte[] { }
            };
        }


        /// Builds a new transaction with the provided inputs added. Existing
        /// inputs, if any, are kept intact.
        public Transaction with_input(Input input)
        {
            var newIns = new Input[inputs.Length + 1];

            for (var i = 0; i < inputs.Length; i++)
            {
                newIns[i] = inputs[i];
            }

            newIns[newIns.Length - 1] = input;

            return new Transaction
            {
                inputs = newIns,
                outputs = outputs,
                fee = fee,
                lock_height = lock_height,
                excess_sig = excess_sig
            };
        }

        /// Builds a new transaction with the provided output added. Existing
        /// outputs, if any, are kept intact.
        public Transaction with_output(Output output)

        {
            var newOuts = new Output[outputs.Length + 1];

            for (var i = 0; i < outputs.Length; i++)
            {
                newOuts[i] = outputs[i];
            }
            newOuts[newOuts.Length - 1] =output;

            return new Transaction
            {
                inputs = inputs,
                outputs = newOuts,
                fee = fee,
                lock_height = lock_height,
                excess_sig = excess_sig
            };
        }


        public void write(IWriter writer)
        {
            writer.write_u64(fee);
            writer.write_u64(lock_height);
            writer.write_bytes(excess_sig);
            writer.write_u64((ulong) inputs.Length);
            writer.write_u64((ulong) outputs.Length);

            var inputsSorted = inputs.OrderBy(o => o.hash().Hex);
            foreach (var input in inputsSorted)
            {
                input.write(writer);
            }

            var outputsSorted = outputs.OrderBy(o => o.hash().Hex);
            foreach (var output in outputsSorted)
            {
                output.write(writer);
            }

        }

        public static Transaction readnew(IReader reader)
        {
            var res = new Transaction();
            return res;
        }

        public void read(IReader reader)
        {
            fee = reader.read_u64();
            lock_height = reader.read_u64();
            excess_sig = reader.read_vec();

            var input_len = reader.read_u64();
            var output_len = reader.read_u64();

            inputs = Ser.read_and_verify_sorted<Input>(reader, input_len);
            outputs = Ser.read_and_verify_sorted<Output>(reader, output_len);
        }


        /// Builds a new transaction with the provided fee.
        public Transaction with_fee(ulong newfee)
        {
            var res = new Transaction
            {
                inputs = inputs,
                outputs = outputs,
                fee = newfee,
                lock_height = lock_height,
                excess_sig = excess_sig
            };
            return res;
        }

        /// Builds a new transaction with the provided lock_height.
        public Transaction with_lock_height(ulong new_lock_height)
        {
            var res = new Transaction
            {
                inputs = inputs,
                outputs = outputs,
                fee = fee,
                lock_height = new_lock_height,
                excess_sig = excess_sig
            };
            return res;
        }

        /// The verification for a MimbleWimble transaction involves getting the
        /// excess of summing all commitments and using it as a public key
        /// to verify the embedded signature. The rational is that if the values
        /// sum to zero as they should in r.G + v.H then only k.G the excess
        /// of the sum of r.G should be left. And r.G is the definition of a
        /// public key generated using r as a private key.
        public TxKernel verify_sig(Secp256k1 secp)

        {
            var rsum = this.sum_commitments(secp);

            var msg = Message.from_slice(TransactionHelper.kernel_sig_msg(fee, lock_height));
            var sig = Signiture.from_der(secp, excess_sig);

            // pretend the sum is a public key (which it is, being of the form r.G) and
            // verify the transaction sig with it
            //
            // we originally converted the commitment to a key_id here (commitment to zero)
            // and then passed the key_id to secp.verify()
            // the secp api no longer allows us to do this so we have wrapped the complexity
            // of generating a public key from a commitment behind verify_from_commit

            secp.verify_from_commit(msg, sig, rsum);

            var kernel = new TxKernel
            {
                features = KernelFeatures.DEFAULT_KERNEL,
                excess = rsum,
                excess_sig = excess_sig,
                fee = fee,
                lock_height = lock_height
            };
            Log.Debug(
                "tx verify_sig: fee - {fee}, lock_height - {lock_height}",
                kernel.fee,
                kernel.lock_height
            );

            return kernel;
        }


        public void validate(Secp256k1 secp)
        {
            if ((fee & 1) != 0)
            {
                throw new OddFeeException();
            }
            foreach (var outp in outputs)
            {
                outp.Verify_proof(secp);
            }
            verify_sig(secp);
        }

        public Transaction clone()
        {
            var res = new Transaction
            {
                inputs = inputs,
                outputs = outputs,
                fee = fee,
                lock_height = lock_height,
                excess_sig = excess_sig
            };
            return res;
        }

        public Input[] inputs_commited()
        {
            return inputs;
        }

        public Output[] outputs_committed()
        {
            return outputs;
        }

        public long overage()
        {
            return (long)fee;
        }
    }
}