using Grin.CoreImpl;

namespace Grin.WalletImpl.WalletTypes
{
    public class Types
    {
        public const string DatFile = "wallet.dat";
        public const string LockFile = "wallet.lock";
        public const string SeedFile = "wallet.seed";

        public const ulong DefaultBaseFee = Consensus.MilliGrin;

        /// Transaction fee calculation
        public static ulong tx_fee(uint inputLen, uint outputLen, uint? baseFee)
        {
            var useBaseFee = baseFee ?? DefaultBaseFee;



           var txWeight = -1 * (int) inputLen + 4 * (int)outputLen + 1;
            if (txWeight< 1) {
                txWeight = 1;
            }

            return (ulong) txWeight * useBaseFee;
        }


    }



}