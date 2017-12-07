using System;
using System.Linq;
using System.Runtime.ExceptionServices;
using Common;
using Secp256k1Proxy.Ffi;
using Secp256k1Proxy.Lib;

namespace Secp256k1Proxy.Key
{


 

    public class PublicKey:ICloneable<PublicKey>
    {
        public byte[] Value { get; }

        public string HexValue => HexUtil.to_hex(Value);

        private PublicKey(byte[] value)
        {
            Value = value;
        }

        public bool is_valid()
        {
            return Value.Count(w => w == 0) != Value.Length;
        }

        /// Creates a public key directly from a slice
        public static PublicKey from_slice(Secp256K1 secp, byte[] data)
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
        public static PublicKey from_secret_key(Secp256K1 secp, SecretKey sk)
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
            return new PublicKey(ByteUtil.Get_bytes(0,64));
        }


        public static PublicKey From(byte[] value)
        {
            return new PublicKey(value);
        }

        /// Serialize the key as a byte-encoded pair of values. In compressed form
        /// the y-coordinate is represented by only a single bit, as x determines
        /// it up to one bit.
        
        public byte[] serialize_vec(Secp256K1 secp, bool isCompressed)
        {

            long retLen = Constants.Constants.PublicKeySize;
            var ret = new byte[Constants.Constants.PublicKeySize];
            
            var compressed = (uint) (isCompressed
                ? Secp256K1Options.Secp256K1SerCompressed
                : Secp256K1Options.Secp256K1SerUncompressed);
            
                var err = Proxy.secp256k1_ec_pubkey_serialize(secp.Ctx, ret, ref retLen, Value, compressed);
            
            if (err != 1)
            {
                throw new Exception("This should never happen!");
            }
        
            Array.Resize(ref ret,(int)retLen);

            return ret;

        }

        public void add_exp_assign(Secp256K1 secp, SecretKey other)
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

        public void mul_assign(Secp256K1 secp, SecretKey other)
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

        public PublicKey Clone()
        {
            return new PublicKey(Value.ToArray());
        }
    }
}