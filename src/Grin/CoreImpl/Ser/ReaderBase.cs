using System;
using Common;

namespace Grin.CoreImpl.Ser
{
    public abstract class ReaderBase : IReader
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
            var bs = read_fixed_bytes(4);
            bs.BigEndian();
            return BitConverter.ToUInt32(bs, 0);
        }

        public ulong read_u64()
        {
            var bs = read_fixed_bytes(8);
            bs.BigEndian();
            return BitConverter.ToUInt64(bs, 0);
        }

        public long read_i64()
        {
            var bs = read_fixed_bytes(8);
            bs.BigEndian();
            return BitConverter.ToInt64(bs, 0);
        }

        public byte[] read_vec()
        {
            var len = (uint) read_u64();
            var bs = read_fixed_bytes(len);
            return bs;
        }

        public byte[] read_limited_vec(uint max)
        {
            var len = Math.Min(max, (uint) read_u64());
            var bs = read_fixed_bytes(len);
            return bs;
        }

        public abstract byte[] read_fixed_bytes(uint length);

        public byte expect_u8(byte val)
        {
            var b = read_u8();
            if (b == val)
            {
                return b;
            }
            throw new Exception("Unexpected data");

            //Err(Error::UnexpectedData {
            //    expected: vec![val],
            //    received: vec![b],
            //})
        }
    }
}