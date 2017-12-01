using System;
using System.Diagnostics;

namespace Grin.CoreImpl.Ser
{
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

            write_fixed_bytes(bytes);
        }

        /// Writes a variable number of bytes. The length is encoded as a 64-bit
        /// prefix.
        public void write_bytes(byte[] bytes)
        {

            write_u64((ulong) bytes.Length);
            write_fixed_bytes(bytes);
        }
    }
}