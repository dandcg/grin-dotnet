using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Common;
using Konscious.Security.Cryptography;
using Serilog;

namespace Grin.Core.Core
{
    public class Hash:IReadable, IWriteable
    {
        public byte[] Value { get; private set; }

        public string Hex { get; private set; }

        public Hash(byte[] value)
        {
            Value = value;
            Hex = HexUtil.to_hex(value);
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

        public void read(IReader reader)
        {
   var v = reader.read_fixed_bytes(32);
            Value = v;
            Hex = HexUtil.to_hex(v);
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
            return (this.Hex == other.Hex);
        }

        public override int GetHashCode()
        {
            return Hex.GetHashCode();
        }
    }

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


    /// A trait for types that have a canonical hash
    public static class Hashed
    {
        public static Hash hash<T>(this T self) where T : IWriteable
        {
            var hasher = HashWriter.Default();

            self.write(hasher);

            var ret = hasher.finalize();

            return new Hash(ret);
        }

        public static Hash hash_with<T,T2>(this T self, T2 other) where T : IWriteable where  T2 : IWriteable
        {
            var hasher = HashWriter.Default();

            self.write(hasher);

            Log.Verbose("Hashing with additional data");

            other.write(hasher);

            var ret = hasher.finalize();

            return new Hash(ret);
        }



        public static void verify_sort_order(this byte[] self)
        {

            throw new NotImplementedException();

            //match self.iter()
            //        .map(|item| item.hash())
            //    .collect::<Vec<_>>()
            //    .windows(2)
            //    .any(|pair| pair[0] > pair[1])
            //{
            //    true => Err(ser::Error::BadlySorted),
            //    false => Ok(()),
            //}
        }


}
}