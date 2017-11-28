using System.Collections.Generic;
using System.IO;
using Grin.KeychainImpl.ExtKey;
using Secp256k1Proxy.Constants;
using Secp256k1Proxy.Pedersen;

namespace Grin.CoreImpl.Ser
{
    public static class Ser
    {
        /// Reads a collection of serialized items into a Vec
        /// and verifies they are lexicographically ordered.
        /// 
        /// A consensus rule requires everything is sorted lexicographically to avoid
        /// leaking any information through specific ordering of items.
        public static T[] read_and_verify_sorted<T>(IReader reader, ulong count)
            where T : IReadable, IWriteable, new()
        {
            var result = new List<T>();

            for (ulong i = 0; i < count; i++)
            {
                var t = new T();
                t.read(reader);
                result.Add(t);
            }

            //let result:
            //Vec < T > =  try
            //!((0..count).map( | _ | T::read(reader)).collect());
            //result.verify_sort_order() ?;
            return result.ToArray();
        }


        /// Deserializes a Readeable from any std::io::Read implementation.
        public static T deserialize<T>(Stream source, T t) where T : IReadable
        {
            var reader = new BinReader(source);

            t.read(reader);

            return t;
        }

        /// Serializes a Writeable into any std::io::Write implementation.
        public static void serialize<T>(Stream sink, T thing) where T : IWriteable
        {
            var writer = new BinWriter(sink);
            thing.write(writer);
        }

        /// Utility function to serialize a writeable directly in memory using a
        /// Vec
        /// <u8>.
        public static byte[] ser_vec(IWriteable thing)

        {
            var stream = new MemoryStream();

            serialize(stream, thing);

            return stream.ToArray();
        }


        // Helper Utilities for secp classes

        public static Commitment ReadCommitment(IReader reader)
        {
            var a = reader.read_fixed_bytes(Constants.PEDERSEN_COMMITMENT_SIZE);
            return Commitment.from_vec(a);
        }

        public static void WriteCommitment(this Commitment commitment, IWriter writer)
        {
            writer.write_fixed_bytes(commitment.Value);
        }


        public static RangeProof ReadRangeProof(IReader reader)
        {
            var p = reader.read_limited_vec(Constants.MAX_PROOF_SIZE);
            return new RangeProof(p, p.Length);
        }

        public static void WriteRangeProof(this RangeProof rangeProof, IWriter writer)
        {
            writer.write_fixed_bytes(rangeProof.Proof);
        }

        public static Identifier ReadIdentifier(IReader reader)
        {
            var bytes = reader.read_fixed_bytes(Identifier.IdentifierSize);
            return Identifier.from_bytes(bytes);
        }

        public static void WriteIdentifier(this Identifier identifier, IWriter writer)
        {
            writer.write_fixed_bytes(identifier.Bytes);
        }


  
    }
}