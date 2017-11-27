using System;
using Secp256k1Proxy.Ffi;
using Secp256k1Proxy.Key;
using Secp256k1Proxy.Lib;

namespace Secp256k1Proxy.Ecdh
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