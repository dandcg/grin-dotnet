using System;
using Grin.Keychain;
using Konscious.Security.Cryptography;
using Secp256k1Proxy;

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


            Array.Copy(feeBytes, 1, bytes, 16, 8);
            Array.Copy(lockHeightBytes, 1, bytes, 24, 8);


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
    public class TxKernel
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
    }

    /// A transaction
    public class Transaction
    {
        private Transaction(Input[] inputs, Output[] outputs, ulong fee, ulong lockHeight, byte[] excessSig)
        {
            this.inputs = inputs;
            this.outputs = outputs;
            this.fee = fee;
            lock_height = lockHeight;
            excess_sig = excessSig;
        }

        /// Set of inputs spent by the transaction.
        public Input[] inputs { get; }

        /// Set of outputs the transaction produces.
        public Output[] outputs { get; }

        /// Fee paid by the transaction.
        public ulong fee { get; }

        /// Transaction is not valid before this block height.
        /// It is invalid for this to be less than the lock_height of any UTXO being spent.
        public ulong lock_height { get; }

        /// The signature proving the excess is a valid public key, which signs
        /// the transaction fee.
        public byte[] excess_sig { get; }

        /// Creates a new empty transaction (no inputs or outputs, zero fee).
        public static Transaction Empty()
        {
            return new Transaction(new Input[] { }, new Output[] { }, 0, 0, new byte[] { });
        }

        /// Creates a new transaction initialized with
        /// the provided inputs, outputs, fee and lock_height.
        public static Transaction New()
        {
            return new Transaction(new Input[] { }, new Output[] { }, 0, 0, new byte[] { });
        }
    }


    /// A transaction input, mostly a reference to an output being spent by the
    /// transaction.
    public class Input
    {
        public Commitment Value { get; }
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
    public class SwitchCommitHash
    {
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
    public class Output
    {
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
    }


    /// Wrapper to Output commitments to provide the Summable trait.
    public class SumCommit
    {
        /// Output commitment
        public Commitment commit { get; }

        /// Secp256k1 used to sum
        public Secp256k1 secp { get; }
    }
}