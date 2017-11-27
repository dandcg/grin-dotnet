namespace Grin.Core.Ser
{
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
}