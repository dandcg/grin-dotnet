namespace Grin.Core.Ser
{
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
}