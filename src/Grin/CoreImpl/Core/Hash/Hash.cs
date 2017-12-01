using System.Linq;
using Common;
using Grin.CoreImpl.Ser;

namespace Grin.CoreImpl.Core.Hash
{
    public class Hash : IReadable, IWriteable
    {
        public byte[] Value { get; private set; }

        public string Hex => HexUtil.to_hex(Value);

        public Hash(byte[] value)
        {
            Value = value;
        }

        private Hash()
        {
        }

        public override string ToString()
        {
            return Value.AsString();
        }


        /// A hash consisting of all zeroes, used as a sentinel. No known preimage.
        public static Hash ZERO_HASH()
        {
            return new Hash(new byte[32]);
        }

        public static Hash readnew(IReader reader)
        {
            var hash = new Hash();
            hash.read(reader);
            return hash;
        }


        public void read(IReader reader)
        {
            Value = reader.read_fixed_bytes(32);
      
        }


        public void write(IWriter writer)
        {
            writer.write_fixed_bytes(Value);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Hash other))
            {
                return false;
            }
            return Hex == other.Hex;
        }

        public override int GetHashCode()
        {
            return Hex.GetHashCode();
        }


        public Hash Clone()
        {
            return new Hash(Value.ToArray());
        }
    }
}