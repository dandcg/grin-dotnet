using System;
using System.Linq;
using System.Security.Cryptography;
using Common;
using Secp256k1Proxy.Ffi;
using Secp256k1Proxy.Lib;

namespace Secp256k1Proxy.Key
{
    public class SecretKey:ICloneable<SecretKey>
    {
        public static SecretKey ZeroKey = new SecretKey(new byte[]
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0});

        public static SecretKey OneKey = new SecretKey(new byte[]
            {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1});

      
        private SecretKey(byte[] value)
        {
            Value = value;
        }

        public byte[] Value { get; }

        public string HexValue => HexUtil.to_hex(Value);


        /// Creates a new random secret key
        public static SecretKey New(Secp256K1 secp, RandomNumberGenerator rng)
        {
            var data = ByteUtil.Get_random_bytes(rng,32);
            {
                while (Proxy.secp256k1_ec_seckey_verify(secp.Ctx, data) == 0) {
                    data = ByteUtil.Get_random_bytes(rng,32);
                }
            }
            return new SecretKey(data);
        }

        public SecretKey Clone()
        {
            return new SecretKey(Value.ToArray());
        }



        /// Converts a `SECRET_KEY_SIZE`-byte slice to a secret key
        public static SecretKey From_slice(Secp256K1 secp, byte[] data)
        { 
            switch (data.Length)
            {
                case Constants.Constants.SecretKeySize:

               
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
        public void Add_assign(Secp256K1 secp, SecretKey other)
        {
            if (Proxy.secp256k1_ec_privkey_tweak_add(secp.Ctx, Value, other.Value) != 1)
            {
                throw new Exception("InvalidSecretKey");
            }
        }


        /// Multiplies one secret key by another, modulo the curve order
        public void Mul_assign(Secp256K1 secp, SecretKey other)
        {
            if (Proxy.secp256k1_ec_privkey_tweak_mul(secp.Ctx,  Value, other.Value) != 1)
            {
                throw new Exception("InvalidSecretKey");
            }
        }

        public void Extend_from_slice(byte[] bytes)
        {
            throw new NotImplementedException();
        }
    }
}