using System.Linq;
using Common;
using Konscious.Security.Cryptography;
using Secp256k1Proxy.Key;
using Secp256k1Proxy.Lib;

namespace Grin.KeychainImpl.ExtKey
{
    public class Identifier : ICloneable<Identifier>
    {
        public override string ToString()
        {
            return HexValue;
        }

        // const
        public const int IdentifierSize = 10;
        
        // contructor
        public Identifier()
        {
            var bytes = new byte[IdentifierSize];
            Value = bytes;
        }
        private Identifier(byte[] value)
        {
            Value = value;
        }

        // data 
        public byte[] Value { get; private set; }

        public string HexValue
        {
            get => HexUtil.to_hex(Value);
            set => Value = HexUtil.from_hex(value);
        }

        // functions
        public static Identifier Zero()
        {
            return new Identifier(new byte[IdentifierSize]);
        }

        public static Identifier From_key_id(Secp256K1 secp, PublicKey pubKey)
        {
            var bytes = pubKey.serialize_vec(secp, true);
            var hashAlgorithm = new HMACBlake2B(null, IdentifierSize * 8);
            var key = hashAlgorithm.ComputeHash(bytes);
            return new Identifier(key);
        }

        public static Identifier From_hex(string hex)
        {
            var bytes = HexUtil.from_hex(hex);
            return From_bytes(bytes);
        }

        public static Identifier From_bytes(byte[] bytes)
        {
            var min = IdentifierSize < bytes.Length ? IdentifierSize : bytes.Length;

            var identifierBytes = new byte[min];
            for (var i = 0; i < min; i++)
                identifierBytes[i] = bytes[i];

            return new Identifier(identifierBytes);
        }
        
        public Identifier Clone()
        {
            return From_bytes(Value.ToArray());
        }
    }
}