using System;
using System.Linq;

namespace Grin.Util
{
    public static class HexUtil
    {
        
        /// Decode a hex string into bytes.
        public static byte[] from_hex(string hex_str)
        {
            return Enumerable.Range(0, hex_str.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex_str.Substring(x, 2), 16))
                .ToArray();
        }


    }
}
