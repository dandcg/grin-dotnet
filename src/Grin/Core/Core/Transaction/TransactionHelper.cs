using System;

namespace Grin.Core.Core.Transaction
{
    public static class TransactionHelper
    {
        /// The size to use for the stored blake2 hash of a switch_commitment
        public const uint SWITCH_COMMIT_HASH_SIZE = 20;


        /// Construct msg bytes from tx fee and lock_height
        public static byte[] kernel_sig_msg(ulong fee, ulong lock_height)
        {
            var bytes = new byte[32];

            var feeBytes = BitConverter.GetBytes(fee);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(feeBytes);
            }

            var lockHeightBytes = BitConverter.GetBytes(lock_height);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(lockHeightBytes);
            }


            Array.Copy(feeBytes, 0, bytes, 15, 8);
            Array.Copy(lockHeightBytes, 0, bytes, 23, 8);


            return bytes;
        }
    }
}