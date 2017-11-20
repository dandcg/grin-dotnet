using System;
using System.Collections.Generic;
using System.IO;
using Common;
using Grin.Core.Core;
using Grin.Keychain;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Secp256k1Proxy;

namespace Grin.Core
{
    /// Signal to a serializable object how much of its data should be serialized
    public enum SerializationMode
    {
        /// Serialize everything sufficiently to fully reconstruct the object
        Full,

        /// Serialize the data that defines the object
        Hash,

        /// Serialize everything that a signer of the object should know
        SigHash
    }

    public interface IWriter
    {
        /// The mode this serializer is writing in
        SerializationMode serialization_mode();

        /// Writes a fixed number of bytes from something that can turn itself into
        void write_fixed_bytes(byte[] b);

        /// Writes a u8 as bytes
        void write_u8(byte n);

        /// Writes a u16 as bytes
        void write_u16(ushort n);

        /// Writes a u32 as bytes
        void write_u32(uint n);

        /// Writes a u64 as bytes
        void write_u64(ulong n);

        /// Writes a i64 as bytes
        void write_i64(long n);

        /// Writes a variable number of bytes. The length is encoded as a 64-bit
        /// prefix.
        void write_bytes(byte[] bytes);
    }


    /// Implementations defined how different numbers and binary structures are
    /// written to an underlying stream or container (depending on implementation).
    public abstract class WriterBase : IWriter
    {
        /// The mode this serializer is writing in
        public abstract SerializationMode serialization_mode();


        public abstract void write_fixed_bytes(byte[] b);


        /// Writes a u8 as bytes
        public void write_u8(byte n)
        {
            write_fixed_bytes(new[] {n});
        }

        /// Writes a u16 as bytes
        public void write_u16(ushort n)
        {
            var bytes = BitConverter.GetBytes(n);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }


            write_fixed_bytes(bytes);
        }

        /// Writes a u32 as bytes
        public void write_u32(uint n)
        {
            var bytes = BitConverter.GetBytes(n);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }


            write_fixed_bytes(bytes);
        }

        /// Writes a u64 as bytes
        public void write_u64(ulong n)
        {
            var bytes = BitConverter.GetBytes(n);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }


            write_fixed_bytes(bytes);
        }

        /// Writes a i64 as bytes
        public void write_i64(long n)
        {
            var bytes = BitConverter.GetBytes(n);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
        }

        /// Writes a variable number of bytes. The length is encoded as a 64-bit
        /// prefix.
        public void write_bytes(byte[] bytes)
        {
            write_u64((ulong) bytes.Length);
            write_fixed_bytes(bytes);
        }
    }

    /// Implementations defined how different numbers and binary structures are
    /// read from an underlying stream or container (depending on implementation).
    public interface IReader
    {
        /// Read a u8 from the underlying Read
        byte read_u8();

        /// Read a u16 from the underlying Read
        ushort read_u16();

        /// Read a u32 from the underlying Read
        uint read_u32();

        /// Read a u64 from the underlying Read
        ulong read_u64();

        /// Read a i32 from the underlying Read
        long read_i64();

        /// first before the data bytes.
        byte[] read_vec();

        /// first before the data bytes limited to max bytes.
        byte[] read_limited_vec(uint max);

        /// Read a fixed number of bytes from the underlying reader.
        byte[] read_fixed_bytes(uint length);

        /// Consumes a byte from the reader, producing an error if it doesn't have
        /// the expected value
        byte expect_u8(byte val);
    }

    public abstract class ReaderBase:IReader
    {


        public byte read_u8()
        {
            var bs = read_fixed_bytes(1);
            return bs[0];
        }

        public ushort read_u16()
        {
            var bs = read_fixed_bytes(2);
         bs.BigEndian();
            return BitConverter.ToUInt16(bs, 0);


        }

        public uint read_u32()
        {
            throw new NotImplementedException();
        }

        public ulong read_u64()
        {
            throw new NotImplementedException();
        }

        public long read_i64()
        {
            throw new NotImplementedException();
        }

        public byte[] read_vec()
        {
            throw new NotImplementedException();
        }

        public byte[] read_limited_vec(uint max)
        {
            throw new NotImplementedException();
        }

        public abstract byte[] read_fixed_bytes(uint length);

        public byte expect_u8(byte val)
        {
            throw new NotImplementedException();
        }
    }

    public interface IWriteable
    {
        void write(IWriter writer);
    }

    public interface IWriteableSorted
    {
        void write_sorted(IWriter writer);
    }



    /// Trait that every type that can be deserialized from binary must implement.
    /// Reads directly to a Reader, a utility type thinly wrapping an
    /// underlying Read implementation.
    public interface IReadable
    {
        /// Reads the data necessary to this Readable from the provided reader
        void read(IReader reader);
    }



public static class Ser
    {


        /// Reads a collection of serialized items into a Vec
        /// and verifies they are lexicographically ordered.
        ///
        /// A consensus rule requires everything is sorted lexicographically to avoid
        /// leaking any information through specific ordering of items.
        public static T[] read_and_verify_sorted<T>(IReader reader, UInt64 count) 
            where T: IReadable, IHashed , IWriteable, new()
        {

            var result = new List<T>();

            for (UInt64 i = 0; i <= count; i++)
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
        public static T deserialize<T>(Stream source, T t) where T:IReadable
        {
            var reader = new BinReader(source);

            t.read(reader);

            return t;
        }

        /// Serializes a Writeable into any std::io::Write implementation.
        public static void serialize<T>(Stream sink, T thing) where T:IWriteable
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

        public static void WriteCommitment(this Commitment commitment, IWriter writer )
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


    /// Utility wrapper for an underlying byte Writer. Defines higher level methods
    /// to write numbers, byte vectors, hashes, etc.
    public class BinWriter : WriterBase
    {
        public Stream Sink { get; }

        public BinWriter(Stream sink)
        {
            Sink = sink;
        }

        public override SerializationMode serialization_mode()
        {
            return SerializationMode.Full;
        }

        public override void write_fixed_bytes(byte[] bs)
        {
            Sink.Write(bs, 0, bs.Length);
        }
    }



    public class BinReader : ReaderBase
    {
        private Stream source;

        public BinReader(Stream source)
        {
            this.source = source;
        }


        public override byte[] read_fixed_bytes(uint length)
        {
            var bs = new byte[length];
            source.Read(bs,0,(int)length);
            return bs;
        }
    }

}