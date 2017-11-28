using System;
using Grin.CoreImpl;

namespace Grin.WalletImpl.WalletTypes
{
    public class Types
    {
        public const string DAT_FILE = "wallet.dat";
        public const string LOCK_FILE = "wallet.lock";
        public const string SEED_FILE = "wallet.seed";

        public const ulong DEFAULT_BASE_FEE = Consensus.MILLI_GRIN;

        /// Transaction fee calculation
        public static UInt64 tx_fee(uint input_len, uint output_len, uint? base_fee)
        {
            var use_base_fee = base_fee ?? DEFAULT_BASE_FEE;



           var tx_weight = -1 * ((int) input_len) + 4 * ((int)output_len) + 1;
            if (tx_weight< 1) {
                tx_weight = 1;
            }

            return ((UInt64) tx_weight) * use_base_fee;
        }


    }



}