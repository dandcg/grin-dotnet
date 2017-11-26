using System;
using System.Linq;
using Common;
using Grin.Keychain;
using Grin.Wallet;
using Konscious.Security.Cryptography;
using Secp256k1Proxy;
using Serilog;

namespace Grin.Core.Core
{
    public static class TransactionHelper
    {
        /// The size to use for the stored blake2 hash of a switch_commitment
        public const uint SWITCH_COMMIT_HASH_SIZE = 20;


        /// Construct msg bytes from tx fee and lock_height
        public static byte[] kernel_sig_msg(ulong fee, ulong lock_height)
        {
            var bytes = new byte[32];

            var feeBytes = BitConverter.GetBytes(fee);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(feeBytes);
            }

            var lockHeightBytes = BitConverter.GetBytes(lock_height);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lockHeightBytes);
            }


            Array.Copy(feeBytes, 0, bytes, 15, 8);
            Array.Copy(lockHeightBytes, 0, bytes, 23, 8);


            return bytes;
        }
    }


    [Flags]
    public enum KernelFeatures : byte
    {
        /// Options for a kernel's structure or use
        /// No flags
        DEFAULT_KERNEL = 0b00000000,

        /// Kernel matching a coinbase output
        COINBASE_KERNEL = 0b00000001
    }


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
            excess = Ser.ReadCommitment(reader);
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


    /// A transaction input, mostly a reference to an output being spent by the
    /// transaction.
    public class Input : IReadable, IWriteable
    {
        public Input()
        {
            
        }

        public Input(Commitment value)
        {
            Value = value;
        }
 
        public Commitment Value { get; private set; }

        public void read(IReader reader)
        {
            Value = Ser.ReadCommitment(reader);
        }

        public void write(IWriter writer)
        {
            Value.WriteCommitment(writer);
        }

        public Input Clone()
        {
            return new Input(Value.Clone());
        }
    }


    /// Options for block validation
    [Flags]
    public enum OutputFeatures : byte
    {
        /// No flags
        DEFAULT_OUTPUT = 0b00000000,

        /// Output is a coinbase output, must not be spent until maturity
        COINBASE_OUTPUT = 0b00000001
    }


    /// Definition of the switch commitment hash
    public class SwitchCommitHash : IReadable, IWriteable
    {

        public override string ToString()
        {

            return hash.AsString();

        }
        public byte[] hash { get; private set; } //: [u8; SWITCH_COMMIT_HASH_SIZE],

        public static SwitchCommitHash From_switch_commit(Commitment switchCommit)
        {
            var hashAlgorithm = new HMACBlake2B(null, (int) TransactionHelper.SWITCH_COMMIT_HASH_SIZE * 8);
            var switch_commit_hash = hashAlgorithm.ComputeHash(switchCommit.Value);


            var h = new byte[TransactionHelper.SWITCH_COMMIT_HASH_SIZE];
            for (var i = 0; i < TransactionHelper.SWITCH_COMMIT_HASH_SIZE; i++)
            {
                h[i] = switch_commit_hash[i];
            }
            return new SwitchCommitHash {hash = h};
        }

        public static SwitchCommitHash readnew(IReader reader)

        {
            var sch = new SwitchCommitHash();
            sch.read(reader);
            return sch;
        }


        public void read(IReader reader)
        {
            hash = reader.read_fixed_bytes(TransactionHelper.SWITCH_COMMIT_HASH_SIZE);
        }

        public void write(IWriter writer)
        {
            writer.write_fixed_bytes(hash);
        }

        public SwitchCommitHash Clone()
        {
            return new SwitchCommitHash(){hash=hash.ToArray()};
        }

    }


    /// Output for a transaction, defining the new ownership of coins that are being
    /// transferred. The commitment is a blinded value for the output while the
    /// range proof guarantees the commitment includes a positive value without
    /// overflow and the ownership of the private key. The switch commitment hash
    /// provides future-proofing against quantum-based attacks, as well as provides
    /// wallet implementations with a way to identify their outputs for wallet
    /// reconstruction
    /// 
    /// The hash of an output only covers its features, lock_height, commitment,
    /// and switch commitment. The range proof is expected to have its own hash
    /// and is stored and committed to separately.
    public class Output : IReadable, IWriteable
    {
    
        public Output()
        {
            
        }

        /// Options for an output's structure or use
        public OutputFeatures features { get; set; }

        /// The homomorphic commitment representing the output's amount
        public Commitment commit { get; set; }

        /// The switch commitment hash, a 160 bit length blake2 hash of blind*J
        public SwitchCommitHash switch_commit_hash { get; set; }

        /// A proof that the commitment is in the right range
        public RangeProof proof { get; set; }


        /// Validates the range proof using the commitment
        public void Verify_proof(Secp256k1 secp)
        {
            secp.verify_range_proof(commit, proof);
        }

        /// Given the original blinding factor we can recover the
        /// value from the range proof and the commitment
        public ulong? Recover_value(Keychain.Keychain keychain, Identifier keyId)
        {
            var pi = keychain.Rewind_range_proof(keyId, commit, proof);

            if (pi.success)
            {
                return pi.value;
            }

            return null;
        }

        public void read(IReader reader)
        {
            features = (OutputFeatures) reader.read_u8();
            commit = Ser.ReadCommitment(reader);
            switch_commit_hash = SwitchCommitHash.readnew(reader);
            proof = Ser.ReadRangeProof(reader);
        }

        public void write(IWriter writer)
        {
            writer.write_u8((byte) features);
            commit.WriteCommitment(writer);
            switch_commit_hash.write(writer);

            if (writer.serialization_mode() == SerializationMode.Full)
            {
                writer.write_bytes(proof.Proof);
            }
        }


        public Output Clone()
        {
            return new Output()
            {
                features=features,
                commit=commit.Clone(),
                switch_commit_hash= switch_commit_hash.Clone(),
                proof=proof.Clone()

            };
        }
    }


    /// Wrapper to Output commitments to provide the Summable trait.
    public class SumCommit : IWriteable, IReadable
    {
        /// Output commitment
        public Commitment commit { get; private set; }

        /// Secp256k1 used to sum
        public Secp256k1 secp { get; private set; }


        public void write(IWriter writer)
        {
            commit.WriteCommitment(writer);
        }

        public void read(IReader reader)
        {
            secp = Secp256k1.WithCaps(ContextFlag.Commit);
            commit = Ser.ReadCommitment(reader);
        }


        public SumCommit add(SumCommit other)

        {
            throw new NotImplementedException();
        }
    }
}