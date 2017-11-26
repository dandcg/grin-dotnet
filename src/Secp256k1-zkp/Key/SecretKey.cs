using System;
using System.Security.Cryptography;
using Common;

namespace Secp256k1Proxy
{
    public class SecretKey
    {
        public static SecretKey ZERO_KEY = new SecretKey(new byte[]
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0});

        public static SecretKey ONE_KEY = new SecretKey(new byte[]
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1});

      
        private SecretKey(byte[] value)
        {
            Value = value;
        }

        public byte[] Value { get; }

        /// Creates a new random secret key
        public static SecretKey New(Secp256k1 secp, RandomNumberGenerator rng)
        {
            var data = ByteUtil.get_random_bytes(rng);
            {
                while (Proxy.secp256k1_ec_seckey_verify(secp.Ctx, data) == 0) {
                    data = ByteUtil.get_random_bytes(rng);
                }
            }
            return new SecretKey(data);
        }

        public SecretKey clone()
        {
            return new SecretKey(Value);
        }



        /// Converts a `SECRET_KEY_SIZE`-byte slice to a secret key
        public static SecretKey from_slice(Secp256k1 secp, byte[] data)
        { 
            switch (data.Length)
            {
                case Constants.SECRET_KEY_SIZE:

               
                    if (Proxy.secp256k1_ec_seckey_verify(secp.Ctx,data) == 0)
                    {
                        throw new Exception("InvalidSecretKey");
                    }

                    return new SecretKey(data);
         

                default:
                    throw new Exception("InvalidSecretKey");
            }
        }


        /// Adds one secret key to another, modulo the curve order
        public void add_assign(Secp256k1 secp, SecretKey other)
        {
            if (Proxy.secp256k1_ec_privkey_tweak_add(secp.Ctx, this.Value, other.Value) != 1)
            {
                throw new Exception("InvalidSecretKey");
            }
        }


        /// Multiplies one secret key by another, modulo the curve order
        public void mul_assign(Secp256k1 secp, SecretKey other)
        {
            if (Proxy.secp256k1_ec_privkey_tweak_mul(secp.Ctx,  this.Value, other.Value) != 1)
            {
                throw new Exception("InvalidSecretKey");
            }
        }

        public void extend_from_slice(byte[] bytes)
        {
            throw new NotImplementedException();
        }
    }
}