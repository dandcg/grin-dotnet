using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Common
{
    public static class ByteUtil
    {


        public static byte[] get_bytes(byte value, int length)
        {
            var bytes = new byte[length];


            for (int i = 0; i < bytes.Length; i++)
                bytes[i] = value;

            return bytes;

        }


        public static byte[] get_random_bytes(RandomNumberGenerator rng, uint len=32)

        {
            var rw = new byte[len];
            rng.GetBytes(rw);
            return rw;
        }

        public static byte[] Combine(params byte[][] arrays)
        {
            byte[] rv = new byte[arrays.Sum(a => a.Length)];
            int offset = 0;
            foreach (byte[] array in arrays)
            {
                System.Buffer.BlockCopy(array, 0, rv, offset, array.Length);
                offset += array.Length;
            }
            return rv;
        }

        public static void BigEndian(this byte[] bs)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bs);
            }
            
        }



        public static string AsString(this byte[] bytes)
        {
            //var sb = new StringBuilder("{");
            //foreach (var b in bytes)
            //{
            //    sb.Append(b + ", ");
            //}
            //sb.Remove(sb.Length - 2, 2);
            //sb.Append("}");
            //return sb.ToString();

            return HexUtil.to_hex(bytes);

        }


    }
}
