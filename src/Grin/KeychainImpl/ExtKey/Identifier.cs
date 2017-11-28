using Common;
using Konscious.Security.Cryptography;
using Secp256k1Proxy.Key;
using Secp256k1Proxy.Lib;

namespace Grin.KeychainImpl.ExtKey
{
    public class Identifier
    {

        public override string ToString()
        {

            return Hex;

        }
        // const
        public const int IdentifierSize = 10;


        // contructor
        public Identifier()
        {
            var bytes = new byte[IdentifierSize];
            Bytes = bytes;
            Hex = HexUtil.to_hex(bytes);

        }


      private Identifier(byte[] bytes)
        {
            Bytes = bytes;
            Hex = HexUtil.to_hex(bytes);
        }

        

        // data 
        public byte[] Bytes { get; }

        public string Hex { get; }

        // functions
        public static Identifier Zero()
        {
            return new Identifier(new byte[IdentifierSize]);
        }


        public static Identifier from_key_id(Secp256k1 secp, PublicKey pubKey)
        {
            var bytes = pubKey.serialize_vec(secp, true);
            var hashAlgorithm = new HMACBlake2B(null, IdentifierSize * 8);
            var key = hashAlgorithm.ComputeHash(bytes);
            return new Identifier(key);
        }

        public static Identifier from_hex(string hex)
        {
            var bytes = HexUtil.from_hex(hex);
                            return from_bytes(bytes);
        }

        public static Identifier from_bytes(byte[] bytes)
        {
            var min = IdentifierSize < bytes.Length ? IdentifierSize : bytes.Length;

            var identifierBytes = new byte[min];
            for (var i = 0; i < min; i++)
                identifierBytes[i] = bytes[i];

            return new Identifier(identifierBytes);
        }


        public Identifier Clone()
        {
            
            return from_bytes(Bytes);


        }


  
    }
}