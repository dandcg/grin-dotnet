using System.Linq;
using Grin.CoreImpl.Core.Hash;
using Grin.CoreImpl.Core.Mod;
using Grin.CoreImpl.Core.Transaction.Errors;
using Grin.CoreImpl.Ser;
using Secp256k1Proxy.Lib;
using Secp256k1Proxy.Pedersen;
using Serilog;

namespace Grin.CoreImpl.Core.Transaction
{
    /// A transaction
    public class Transaction : IWriteable, IReadable, ICommitted
    {
        private Transaction()
        {
        }

        /// Set of inputs spent by the transaction.
        public Input[] Inputs { get; private set; }

        /// Set of outputs the transaction produces.
        public Output[] Outputs { get; private set; }

        /// Fee paid by the transaction.
        public ulong Fee { get; private set; }

        /// Transaction is not valid before this block height.
        /// It is invalid for this to be less than the lock_height of any UTXO being spent.
        public ulong LockHeight { get; private set; }

        /// The signature proving the excess is a valid public key, which signs
        /// the transaction fee.
        public byte[] ExcessSig { get; set; }

        /// Creates a new empty transaction (no inputs or outputs, zero fee).
        public static Transaction Empty()
        {
            return new Transaction
            {
                Inputs = new Input[] { },
                Outputs = new Output[] { },
                Fee = 0,
                LockHeight = 0,
                ExcessSig = new byte[] { }
            };
        }

        /// Creates a new transaction initialized with
        /// the provided inputs, outputs, fee and lock_height.
        public static Transaction New()
        {
            return new Transaction
            {
                Inputs = new Input[] { },
                Outputs = new Output[] { },
                Fee = 0,
                LockHeight = 0,
                ExcessSig = new byte[] { }
            };
        }


        /// Builds a new transaction with the provided inputs added. Existing
        /// inputs, if any, are kept intact.
        public Transaction with_input(Input input)
        {
            var newIns = new Input[Inputs.Length + 1];

            for (var i = 0; i < Inputs.Length; i++)
            {
                newIns[i] = Inputs[i];
            }

            newIns[newIns.Length - 1] = input;

            return new Transaction
            {
                Inputs = newIns,
                Outputs = Outputs,
                Fee = Fee,
                LockHeight = LockHeight,
                ExcessSig = ExcessSig
            };
        }

        /// Builds a new transaction with the provided output added. Existing
        /// outputs, if any, are kept intact.
        public Transaction with_output(Output output)

        {
            var newOuts = new Output[Outputs.Length + 1];

            for (var i = 0; i < Outputs.Length; i++)
            {
                newOuts[i] = Outputs[i];
            }
            newOuts[newOuts.Length - 1] =output;

            return new Transaction
            {
                Inputs = Inputs,
                Outputs = newOuts,
                Fee = Fee,
                LockHeight = LockHeight,
                ExcessSig = ExcessSig
            };
        }


        public void Write(IWriter writer)
        {
            writer.write_u64(Fee);
            writer.write_u64(LockHeight);
            writer.write_bytes(ExcessSig);
            writer.write_u64((ulong) Inputs.Length);
            writer.write_u64((ulong) Outputs.Length);

            var inputsSorted = Inputs.OrderBy(o => o.Hash().Hex);
            foreach (var input in inputsSorted)
            {
                input.Write(writer);
            }

            var outputsSorted = Outputs.OrderBy(o => o.Hash().Hex);
            foreach (var output in outputsSorted)
            {
                output.Write(writer);
            }

        }

        public static Transaction Readnew(IReader reader)
        {
            var res = new Transaction();
            return res;
        }

        public void Read(IReader reader)
        {
            Fee = reader.read_u64();
            LockHeight = reader.read_u64();
            ExcessSig = reader.read_vec();

            var inputLen = reader.read_u64();
            var outputLen = reader.read_u64();

            Inputs = Ser.Ser.Read_and_verify_sorted<Input>(reader, inputLen);
            Outputs = Ser.Ser.Read_and_verify_sorted<Output>(reader, outputLen);
        }


        /// Builds a new transaction with the provided fee.
        public Transaction with_fee(ulong newfee)
        {
            var res = new Transaction
            {
                Inputs = Inputs,
                Outputs = Outputs,
                Fee = newfee,
                LockHeight = LockHeight,
                ExcessSig = ExcessSig
            };
            return res;
        }

        /// Builds a new transaction with the provided lock_height.
        public Transaction with_lock_height(ulong newLockHeight)
        {
            var res = new Transaction
            {
                Inputs = Inputs,
                Outputs = Outputs,
                Fee = Fee,
                LockHeight = newLockHeight,
                ExcessSig = ExcessSig
            };
            return res;
        }

        /// The verification for a MimbleWimble transaction involves getting the
        /// excess of summing all commitments and using it as a public key
        /// to verify the embedded signature. The rational is that if the values
        /// sum to zero as they should in r.G + v.H then only k.G the excess
        /// of the sum of r.G should be left. And r.G is the definition of a
        /// public key generated using r as a private key.
        public TxKernel verify_sig(Secp256K1 secp)

        {
            var rsum = this.sum_commitments(secp);

            var msg = Message.from_slice(TransactionHelper.kernel_sig_msg(Fee, LockHeight));
            var sig = Signiture.from_der(secp, ExcessSig);

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
                Features = KernelFeatures.DefaultKernel,
                Excess = rsum,
                ExcessSig = ExcessSig,
                Fee = Fee,
                LockHeight = LockHeight
            };
            Log.Debug(
                "tx verify_sig: fee - {fee}, lock_height - {lock_height}",
                kernel.Fee,
                kernel.LockHeight
            );

            return kernel;
        }


        public void Validate(Secp256K1 secp)
        {
            if ((Fee & 1) != 0)
            {
                throw new OddFeeException();
            }
            if (Inputs.Length > Consensus.MaxBlockInputs)
            {
                throw new ToManyInputsException();
            }


            foreach (var outp in Outputs)
            {
                outp.Verify_proof(secp);
            }
            verify_sig(secp);
        }

        public Transaction Clone()
        {
            var res = new Transaction
            {
                Inputs = Inputs,
                Outputs = Outputs,
                Fee = Fee,
                LockHeight = LockHeight,
                ExcessSig = ExcessSig
            };
            return res;
        }

        public Input[] inputs_commited()
        {
            return Inputs;
        }

        public Output[] outputs_committed()
        {
            return Outputs;
        }

        public long Overage()
        {
            return (long)Fee;
        }
    }
}