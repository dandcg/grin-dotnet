using System;
using Secp256k1Proxy;

namespace Grin.Core.Core
{
    public static class TransactionConstants
    {
        /// The size to use for the stored blake2 hash of a switch_commitment
        public const uint SWITCH_COMMIT_HASH_SIZE = 20;
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
        public KernelFeatures features { get; }

        /// Fee originally included in the transaction this proof is for.
        public ulong fee { get; }

        /// This kernel is not valid earlier than lock_height blocks
        /// The max lock_height of all *inputs* to this transaction
        public ulong lock_height { get; }

        /// Remainder of the sum of all transaction commitments. If the transaction
        /// is well formed, amounts components should sum to zero and the excess
        /// is hence a valid public key.
        public Commitment excess { get; }

        /// The signature proving the excess is a valid public key, which signs
        /// the transaction fee.
        public byte[] excess_sig { get; }
    }

    /// A transaction
    public class Transaction
    {
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
        private byte[] hash { get; } //: [u8; SWITCH_COMMIT_HASH_SIZE],
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
        public OutputFeatures features { get; }

        /// The homomorphic commitment representing the output's amount
        public Commitment commit { get; }

        /// The switch commitment hash, a 160 bit length blake2 hash of blind*J
        public SwitchCommitHash switch_commit_hash { get; }

        /// A proof that the commitment is in the right range
        public RangeProof proof { get; }
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