using System.IO;
using Konscious.Security.Cryptography;

namespace Grin.Core.Core
{
    public class HashWriter : WriterBase
    {
        public MemoryStream stream { get; private set; }
        public HMACBlake2B state { get; private set; }

        public static HashWriter Default()
        {
            var state = new HMACBlake2B(null, 32 * 8);
            var stream = new MemoryStream();

            return new HashWriter {state = state, stream = stream};
        }

        /// Consume the `HashWriter`, outputting its current hash into a 32-byte
        /// array
        public byte[] finalize()
        {
            return state.ComputeHash(stream.ToArray());
        }

        /// Consume the `HashWriter`, outputting a `Hash` corresponding to its
        /// current state
        public Hash into_hash()
        {
            var res = finalize();
            return new Hash(res);
        }


        public override SerializationMode serialization_mode()
        {
            return SerializationMode.Hash;
        }

        public override void write_fixed_bytes(byte[] bs)
        {
            stream.Write(bs, 0, bs.Length);
        }
    }
}