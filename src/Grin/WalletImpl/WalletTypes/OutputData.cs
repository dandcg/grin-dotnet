using System.Linq;
using Grin.KeychainImpl.ExtKey;

namespace Grin.WalletImpl.WalletTypes
{
    /// Information about an output that's being tracked by the wallet. Must be
    /// enough to reconstruct the commitment associated with the ouput when the
    /// root private key is known.
    public class OutputData
    {


        public OutputData(Identifier rootKeyId, Identifier keyId, uint nChild, ulong value, OutputStatus status,
            ulong height, ulong lockHeight, bool isCoinbase)
        {
            RootKeyId = rootKeyId;
            KeyId = keyId;
            NChild = nChild;
            Value = value;
            Status = status;
            Height = height;
            LockHeight = lockHeight;
            IsCoinbase = isCoinbase;
        }

        /// Root key_id_set that the key for this output is derived from
        public Identifier RootKeyId { get; set; }

        /// Derived key for this output
        public Identifier KeyId { get; set; }

        /// How many derivations down from the root key
        public uint NChild { get; set; }

        /// Commitment of the output, necessary to rebuild the commitment
        public ulong Value { get; set; }

        /// Current status of the output
        public OutputStatus Status { get;  set; }

        /// Height of the output
        public ulong Height { get; set; }

        /// Height we are locked until
        public ulong LockHeight { get; set; }

        /// Is this a coinbase output? Is it subject to coinbase locktime?
        public bool IsCoinbase { get; set; }

        public OutputData Clone()
        {
            return new OutputData(RootKeyId.Clone(),KeyId.Clone(),NChild,Value,Status,Height, LockHeight,IsCoinbase);
        }

        /// Lock a given output to avoid conflicting use
        public void Lock()
        {
            Status = OutputStatus.Locked;
        }

        /// How many confirmations has this output received?
        /// If height == 0 then we are either Unconfirmed or the output was
        /// cut-through
        /// so we do not actually know how many confirmations this output had (and
        /// never will).
        public ulong num_confirmations(ulong currentHeight)
        {
            if (Status == OutputStatus.Unconfirmed)
            {
                return 0;
            }
            if (Status == OutputStatus.Spent && Height == 0)
            {
                return 0;
            }
            return 1+(currentHeight - Height);
        }

        /// Check if output is eligible to spend based on state and height and confirmations
        public bool eligible_to_spend(
            ulong currentHeight,
            ulong minimumConfirmations
        )
        {
            if (new[]
            {
                OutputStatus.Spent,
                OutputStatus.Locked
            }.Contains(Status))
            {
                return false;
            }
            if (Status == OutputStatus.Unconfirmed && IsCoinbase)
            {
                return false;
            }
            if (LockHeight > currentHeight)
            {
                return false;
            }
            if (Status == OutputStatus.Unspent && Height + num_confirmations(currentHeight) >= minimumConfirmations)
            {
                return true;
            }
            if (Status == OutputStatus.Unconfirmed && minimumConfirmations == 0)
            {
                return true;
            }
            return false;
        }
    }
}