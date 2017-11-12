using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grin.Core.Model
{
    public class Hash
    {
        public byte[] Value { get; }




        private Hash(byte[] value)
        {
            Value = value;
        }

        /// A hash consisting of all zeroes, used as a sentinel. No known preimage.
        public static Hash ZERO_HASH()
        {
            return new Hash(new byte[32]);
        }
    }




}
