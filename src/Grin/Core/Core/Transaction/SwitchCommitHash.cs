using System.Linq;
using Common;
using Grin.Core.Ser;
using Konscious.Security.Cryptography;
using Secp256k1Proxy;
using Secp256k1Proxy.Pedersen;

namespace Grin.Core.Core.Transaction
{
    /// Definition of the switch commitment hash
    public class SwitchCommitHash : IReadable, IWriteable
    {

        public override string ToString()
        {

            return hash.AsString();

        }

        /// simple hash
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
}