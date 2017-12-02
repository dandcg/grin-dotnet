using System.Linq;
using Common;
using Grin.CoreImpl.Ser;
using Konscious.Security.Cryptography;
using Secp256k1Proxy.Pedersen;

namespace Grin.CoreImpl.Core.Transaction
{
    /// Definition of the switch commitment hash
    public class SwitchCommitHash : IReadable, IWriteable
    {

        public override string ToString()
        {

            return Hash.AsString();

        }

        /// simple hash
        public byte[] Hash { get; private set; } //: [u8; SWITCH_COMMIT_HASH_SIZE],

        public static SwitchCommitHash From_switch_commit(Commitment switchCommit)
        {
            var hashAlgorithm = new HMACBlake2B(null, (int) TransactionHelper.SwitchCommitHashSize * 8);
            var switchCommitHash = hashAlgorithm.ComputeHash(switchCommit.Value);


            var h = new byte[TransactionHelper.SwitchCommitHashSize];
            for (var i = 0; i < TransactionHelper.SwitchCommitHashSize; i++)
            {
                h[i] = switchCommitHash[i];
            }
            return new SwitchCommitHash {Hash = h};
        }

        public static SwitchCommitHash Readnew(IReader reader)

        {
            var sch = new SwitchCommitHash();
            sch.Read(reader);
            return sch;
        }


        public void Read(IReader reader)
        {
            Hash = reader.read_fixed_bytes(TransactionHelper.SwitchCommitHashSize);
        }

        public void Write(IWriter writer)
        {
            writer.write_fixed_bytes(Hash);
        }

        public SwitchCommitHash Clone()
        {
            return new SwitchCommitHash(){Hash=Hash.ToArray()};
        }

    }
}