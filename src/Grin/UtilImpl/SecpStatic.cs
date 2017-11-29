using System.Security.Cryptography;
using Secp256k1Proxy.Lib;

namespace Grin.UtilImpl
{
    public static class SecpStatic
    {
        private static readonly object SyncRoot = new object();
        private  static readonly Secp256k1 SecpInst = Secp256k1.WithCaps(ContextFlag.Commit);

        /// Returns the static instance, but calls randomize on it as well
        /// (Recommended to avoid side channel attacks

        public static Secp256k1 Instance
        {
            get
            {
                lock (SyncRoot)
                {
                    SecpInst.Randomize(RandomNumberGenerator.Create());
           
                    return SecpInst;
                }
            }

        }

    }
}
