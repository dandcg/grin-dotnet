using System.IO;
using Grin.CoreImpl.Ser;
using Konscious.Security.Cryptography;

namespace Grin.CoreImpl.Core.Hash
{
    public class HashWriter : WriterBase
    {
        public MemoryStream Stream { get; private set; }
        public HMACBlake2B State { get; private set; }

        public static HashWriter Default()
        {
            var state = new HMACBlake2B(null, 32 * 8);
            var stream = new MemoryStream();

            return new HashWriter {State = state, Stream = stream};
        }

        /// Consume the `HashWriter`, outputting its current hash into a 32-byte
        /// array
        public byte[] FinalizeHash()
        {
            return State.ComputeHash(Stream.ToArray());
        }

        /// Consume the `HashWriter`, outputting a `Hash` corresponding to its
        /// current state
        public Hash into_hash()
        {
            var res = FinalizeHash();
            return new Hash(res);
        }


        public override SerializationMode serialization_mode()
        {
            return SerializationMode.Hash;
        }

        public override void write_fixed_bytes(byte[] bs)
        {
            Stream.Write(bs, 0, bs.Length);
        }
    }
}