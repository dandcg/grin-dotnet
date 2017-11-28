using System.Linq;
using Grin.KeychainImpl.ExtKey;

namespace Grin.WalletImpl.WalletTypes
{
    /// Information about an output that's being tracked by the wallet. Must be
    /// enough to reconstruct the commitment associated with the ouput when the
    /// root private key is known.
    public class OutputData
    {


        public OutputData(Identifier root_key_id, Identifier key_id, uint n_child, ulong value, OutputStatus status,
            ulong height, ulong lock_height, bool is_coinbase)
        {
            this.root_key_id = root_key_id;
            this.key_id = key_id;
            this.n_child = n_child;
            this.value = value;
            this.status = status;
            this.height = height;
            this.lock_height = lock_height;
            this.is_coinbase = is_coinbase;
        }

        /// Root key_id_set that the key for this output is derived from
        public Identifier root_key_id { get; set; }

        /// Derived key for this output
        public Identifier key_id { get; set; }

        /// How many derivations down from the root key
        public uint n_child { get; set; }

        /// Commitment of the output, necessary to rebuild the commitment
        public ulong value { get; set; }

        /// Current status of the output
        public OutputStatus status { get;  set; }

        /// Height of the output
        public ulong height { get; set; }

        /// Height we are locked until
        public ulong lock_height { get; set; }

        /// Is this a coinbase output? Is it subject to coinbase locktime?
        public bool is_coinbase { get; set; }

        public OutputData clone()
        {
            return new OutputData(root_key_id.Clone(),key_id.Clone(),n_child,value,status,height, lock_height,is_coinbase);
        }

        /// Lock a given output to avoid conflicting use
        public void Lock()
        {
            status = OutputStatus.Locked;
        }

        /// How many confirmations has this output received?
        /// If height == 0 then we are either Unconfirmed or the output was
        /// cut-through
        /// so we do not actually know how many confirmations this output had (and
        /// never will).
        public ulong num_confirmations(ulong current_height)
        {
            if (status == OutputStatus.Unconfirmed)
            {
                return 0;
            }
            if (status == OutputStatus.Spent && height == 0)
            {
                return 0;
            }
            return 1+(current_height - height);
        }

        /// Check if output is eligible to spend based on state and height and confirmations
        public bool eligible_to_spend(
            ulong current_height,
            ulong minimum_confirmations
        )
        {
            if (new[]
            {
                OutputStatus.Spent,
                OutputStatus.Locked
            }.Contains(status))
            {
                return false;
            }
            if (status == OutputStatus.Unconfirmed && is_coinbase)
            {
                return false;
            }
            if (lock_height > current_height)
            {
                return false;
            }
            if (status == OutputStatus.Unspent && height + num_confirmations(current_height) >= minimum_confirmations)
            {
                return true;
            }
            if (status == OutputStatus.Unconfirmed && minimum_confirmations == 0)
            {
                return true;
            }
            return false;
        }
    }
}