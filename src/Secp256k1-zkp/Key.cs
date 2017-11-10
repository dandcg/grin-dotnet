using System;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography;

namespace Secp256k1Proxy
{


    public class KeyUtils
    {


        public static byte[] get_bytes(byte value, int length)
        {
            var bytes= new byte[length];


            for (int i = 0; i < bytes.Length; i++)
                bytes[i] = value;

            return bytes;

        }


    public static byte[] random_32_bytes(RandomNumberGenerator rng)

    {
        var rw = new byte[32];
        rng.GetBytes(rw);
        return rw;
    }


    }

    public class PublicKey
    {
        public byte[] Value { get; private set; }

        private PublicKey(byte[] value)
        {
            Value = value;
        }

        public bool is_valid()
        {
            return Value.Count(w => w == 0) != Value.Length;
        }

        /// Creates a public key directly from a slice
        public static PublicKey from_slice(Secp256k1 secp, byte[] data)
        {
            if (data != null)

            {
                var pkBytes= new byte[64];

                if (Proxy.secp256k1_ec_pubkey_parse(secp.Ctx, pkBytes, data, data.Length) == 1)
                {
                    return new PublicKey(pkBytes);

                }

            }
            throw new Exception("InvalidPublicKey");
        }
        /// Creates a new public key from a secret key.
        public static PublicKey from_secret_key(Secp256k1 secp, SecretKey sk)
        {

            if (secp.Caps == ContextFlag.VerifyOnly || secp.Caps == ContextFlag.None)
            {
                throw new Exception ("IncapableContext");
            }

            var pkBytes = new byte[64];

     

                if (Proxy.secp256k1_ec_pubkey_create(secp.Ctx, pkBytes, sk.Value) == 1)
                {
                    return new PublicKey(pkBytes);

                }
    

            throw new Exception("Should never happen!");



        }

        public static PublicKey New()
        {
            return new PublicKey(KeyUtils.get_bytes(0,64));
        }


        public static PublicKey from(byte[] value)
        {
            return new PublicKey(value);
        }

        /// Serialize the key as a byte-encoded pair of values. In compressed form
        /// the y-coordinate is represented by only a single bit, as x determines
        /// it up to one bit.
        
        [HandleProcessCorruptedStateExceptions]
        public byte[] serialize_vec(Secp256k1 secp, bool isCompressed)
        {

            Int64 ret_len = Constants.PUBLIC_KEY_SIZE;
            var ret = new byte[Constants.PUBLIC_KEY_SIZE];
            
            var compressed = (uint) (isCompressed==true
                ? Secp256K1Options.SECP256K1_SER_COMPRESSED
                : Secp256K1Options.SECP256K1_SER_UNCOMPRESSED);
            
                var err = Proxy.secp256k1_ec_pubkey_serialize(secp.Ctx, ret, ref ret_len, this.Value, compressed);
            
            if (err != 1)
            {
                throw new Exception("This should never happen!");
            }
        
            Array.Resize(ref ret,(int)ret_len);

            return ret;

        }

        public void add_exp_assign(Secp256k1 secp, SecretKey other)
        {
            if (secp.Caps == ContextFlag.SignOnly || secp.Caps == ContextFlag.None)
            {
                throw new Exception("IncapableContext");
            }

            if (Proxy.secp256k1_ec_pubkey_tweak_add(secp.Ctx, Value, other.Value) == 1)
            {
                return;
            }

            throw new Exception("InvalidSecretKey");


        }

        public void mul_assign(Secp256k1 secp, SecretKey other)
        {
             if (secp.Caps == ContextFlag.SignOnly || secp.Caps == ContextFlag.None)
            {
              throw new Exception("IncapableContext");
            }
   
                if (Proxy.secp256k1_ec_pubkey_tweak_mul(secp.Ctx, Value , other.Value) == 1) {
                return;
            }

            throw new Exception("InvalidSecretKey");


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
        public static SecretKey New(Secp256k1 secp, RandomNumberGenerator rng)
        {
            var data = KeyUtils.random_32_bytes(rng);
            {
                while (Proxy.secp256k1_ec_seckey_verify(secp.Ctx, data) == 0) {
                    data = KeyUtils.random_32_bytes(rng);
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