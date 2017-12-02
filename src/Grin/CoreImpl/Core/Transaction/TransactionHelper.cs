using System;

namespace Grin.CoreImpl.Core.Transaction
{
    public static class TransactionHelper
    {
        /// The size to use for the stored blake2 hash of a switch_commitment
        public const uint SwitchCommitHashSize = 20;


        /// Construct msg bytes from tx fee and lock_height
        public static byte[] kernel_sig_msg(ulong fee, ulong lockHeight)
        {
            var bytes = new byte[32];

            var feeBytes = BitConverter.GetBytes(fee);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(feeBytes);
            }

            var lockHeightBytes = BitConverter.GetBytes(lockHeight);
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