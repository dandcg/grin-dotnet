using System;

namespace Secp256k1Proxy
{
    public class SharedSecret
    {
        public byte[] Value { get; }

        private SharedSecret(byte[] value)
        {
            Value = value;
        }

        public static SharedSecret New(Secp256k1 secp, PublicKey point, SecretKey scalar)

        {
            var sharedSecretBytes = new byte[32];
            var res = Proxy.secp256k1_ecdh(secp.Ctx, sharedSecretBytes, point.Value, scalar.Value);

            if (res == 1)
                return new SharedSecret(sharedSecretBytes);

            throw new Exception("This should never happen!");
        }
    }
}