using System;
using System.Security.Cryptography;

namespace Grin.Secp256k1
{


    public class KeyUtils
    {

    public static byte[] random_32_bytes()

    {
        var rw = new byte[32];

        var rng = RandomNumberGenerator.Create();
        rng.GetBytes(rw);

        return rw;


    }


    }

    public class PublicKey
    {
        public byte[] Value { get; private set; }

        private PublicKey()
        {
          
        }

        public bool is_valid()
        {
            
            throw new NotImplementedException();

        }

        /// Creates a public key directly from a slice
        public static PublicKey from_slice(byte[] data)
        {
          var pk = new PublicKey();

            if (ECDSA.PublicKeyParse( pk.Value, data, data?.Length??0) == 1)
                {
                return pk;
                }
            throw new Exception("InvalidPublicKey");
        }
    }

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
        public static SecretKey create ()
        {
            var data = KeyUtils.random_32_bytes();
            {
                while (ECDSA.VerifySecretKey(data) == 0) {
                    data = KeyUtils.random_32_bytes();
                }
            }
            return new SecretKey(data);
        }


        /// Converts a `SECRET_KEY_SIZE`-byte slice to a secret key
        public static SecretKey from_slice(byte[] data)
        { 

       

            switch (data.Length)
            {
                case Constants.SECRET_KEY_SIZE:

               
                        if (ECDSA.VerifySecretKey(data) == 0)
                        {
                            throw new Exception("InvalidSecretKey");
                        }

                    return new SecretKey(data);
         

              default:
                   throw new Exception("InvalidSecretKey");
            }
        }


        /// Adds one secret key to another, modulo the curve order
        public void add_assign(SecretKey other)
        {
            if (ECDSA.PrivateKeyTweakAdd(this.Value, other.Value) != 1)
            {
                throw new Exception("InvalidSecretKey");
            }
        }


        /// Multiplies one secret key by another, modulo the curve order
        public void mul_assign(SecretKey other)
        {
            if (ECDSA.PrivateKeyTweakMul(this.Value, other.Value) != 1)
            {
                throw new Exception("InvalidSecretKey");
            }
        }
    }
}