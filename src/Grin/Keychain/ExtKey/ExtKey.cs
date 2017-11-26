using System;
using System.Linq;
using System.Text;
using Common;
using Grin.Core;
using Konscious.Security.Cryptography;
using Secp256k1Proxy;

namespace Grin.Keychain
{
    public class Identifier
    {


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

    /// An ExtendedKey is a secret key which can be used to derive new
    /// secret keys to blind the commitment of a transaction output.
    /// To be usable, a secret key should have an amount assigned to it,
    /// but when the key is derived, the amount is not known and must be
    /// given.
    public class ExtendedKey
    {
    

        /// Depth of the extended key
        public byte Depth { get; set; }

        /// Child number of the key
        public uint NChild { get; set; }

        /// Root key identifier
        public Identifier RootKeyId { get; set; }

        /// Code of the derivation chain
        public byte[] Chaincode { get; set; }

        /// Actual private key
        public SecretKey Key { get; set; }


        public static ExtendedKey from_slice(Secp256k1 secp, byte[] slice)
        {
            // TODO change when ser. ext. size is fixed
            if (slice.Length != 79)
                throw new Exception("InvalidSliceSize");

            var ext = new ExtendedKey {Depth = slice[0]};

            var rootKeyBytes = slice.Skip(1).Take(10).ToArray();
            ext.RootKeyId = Grin.Keychain.Identifier.from_bytes(rootKeyBytes);

            var nchildBytes = slice.Skip(11).Take(4).ToArray();
            Array.Reverse(nchildBytes);
            ext.NChild = BitConverter.ToUInt32(nchildBytes, 0);

            ext.Chaincode = slice.Skip(15).Take(32).ToArray();

            var keyBytes = slice.Skip(47).Take(32).ToArray();


            ext.Key = SecretKey.from_slice(secp, keyBytes);

            return ext;
        }

        /// Creates a new extended master key from a seed
        public static ExtendedKey from_seed(Secp256k1 secp, byte[] seed)
        {
            switch (seed.Length)
            {
                case 16:
                case 32:
                case 64:

                    break;

                default:
                    throw new Exception("InvalidSeedSize");
            }

            var ext = new ExtendedKey
            {
                Depth = 0,
                RootKeyId = Grin.Keychain.Identifier.Zero(),
                NChild = 0
            };

            var keyData = Encoding.ASCII.GetBytes("Mimble seed");
            var blake2B = new HMACBlake2B(keyData, 512);

            var derived = blake2B.ComputeHash(seed);

            ext.Chaincode = derived.Skip(32).Take(32).ToArray();

            var keyBytes = derived.Take(32).ToArray();

            ext.Key = SecretKey.from_slice(secp, keyBytes);

            ext.RootKeyId = ext.Identifier(secp);

            return ext;
        }

        /// Return the identifier of the key
        /// which is the blake2b (10 byte) digest of the PublicKey
        // corresponding to the underlying SecretKey
        public Identifier Identifier(Secp256k1 secp)
        {
            // get public key from private
            var keyId = PublicKey.from_secret_key(secp, Key);

            return Grin.Keychain.Identifier.from_key_id(secp, keyId);
        }

        /// Derive an extended key from an extended key
        public ExtendedKey Derive(Secp256k1 secp, uint n)

        {
            var nBytes = BitConverter.GetBytes(n);

            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(nBytes);
            }

            var seed = ByteUtil.Combine(Key.Value, nBytes);

            var blake2B = new HMACBlake2B(Chaincode, 64*8);

            var derived = blake2B.ComputeHash(seed);

            var secretKey = SecretKey.from_slice(secp, derived.Take(32).ToArray());

            secretKey.add_assign(secp, Key);
            
            // TODO check if key != 0 ?
            
            var chainCode = derived.Skip(32).Take(32).ToArray();

            return new ExtendedKey
            {
                Depth = (byte)(Depth+1),
                RootKeyId = Identifier(secp),
                NChild = n,
                Chaincode = chainCode,
                Key = secretKey
            };
        }


        public ExtendedKey Clone()
        {
            return new ExtendedKey
            {
                Depth = Depth,
                RootKeyId = RootKeyId.Clone(),
                NChild = NChild,
                Chaincode = Chaincode,
                Key = Key.clone()
            };
        }

    }
}