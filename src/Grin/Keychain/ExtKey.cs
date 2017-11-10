using System;
using System.Linq;
using System.Text;
using Grin.Util;
using Konscious.Security.Cryptography;
using Secp256k1Proxy;

namespace Grin.Keychain
{
    public class Identifier
    {
        // const
        public const int IDENTIFIER_SIZE = 10;


        // contructor
        public Identifier()
        {
            zero();
        }

        public Identifier(string hex)
        {
            from_hex(hex);
        }

        public Identifier(byte[] bytes)
        {
            from_bytes(bytes);
        }


        // data 
        public byte[] Bytes { get; } = new byte[IDENTIFIER_SIZE];

        public string Hex { get; private set; }

        // functions
        public void zero()
        {
            for (var i = 0; i < Bytes.Length; i++)
                Bytes[i] = 0;

            HexUtil.to_hex(Bytes);
        }


        public static Identifier from_key_id(Secp256k1 secp, PublicKey pubKey)
        {
            var bytes = pubKey.serialize_vec(secp, true);
            var hashAlgorithm = new HMACBlake2B(null, IDENTIFIER_SIZE*8);
            var key = hashAlgorithm.ComputeHash(bytes);
            return Identifier.from_bytes(key);
        }

        public void from_hex(string hex)
        {
            var bytes = HexUtil.from_hex(hex);
            from_bytes(bytes);
        }

        public static Identifier from_bytes(byte[] bytes)
        {
            var identifier = new Identifier();

            var min = IDENTIFIER_SIZE < bytes.Length ? IDENTIFIER_SIZE : bytes.Length;

            for (var i = 0; i < min; i++)
               identifier.Bytes[i] = bytes[i];

            identifier.Hex =  HexUtil.to_hex(identifier.Bytes);

            return identifier;


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
        public byte depth { get; set; }

        /// Child number of the key
        public uint n_child { get; set; }

        /// Root key identifier
        public Identifier root_key_id { get; set; }

        /// Code of the derivation chain
        public byte[] chaincode { get; set; }

        /// Actual private key
        public SecretKey key { get; set; }


        public static ExtendedKey from_slice(Secp256k1 secp, byte[] slice)
        {
            // TODO change when ser. ext. size is fixed
            if (slice.Length != 79)
                throw new Exception("InvalidSliceSize");

            var ext = new ExtendedKey();
            ext.depth = slice[0];

            var rootKeyBytes = slice.Skip(1).Take(10).ToArray();
            ext.root_key_id = new Identifier(rootKeyBytes);

            var nchildBytes = slice.Skip(11).Take(4).ToArray();
            Array.Reverse(nchildBytes);
            ext.n_child = BitConverter.ToUInt32(nchildBytes, 0);

            ext.chaincode = slice.Skip(15).Take(32).ToArray();

            var keyBytes = slice.Skip(47).Take(32).ToArray();


            ext.key = SecretKey.from_slice(secp, keyBytes);

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
                depth = 0,
                root_key_id = new Identifier(),
                n_child = 0
            };

            var keyData= Encoding.ASCII.GetBytes("Mimble seed");
            var blake2B = new HMACBlake2B(keyData, 512);
            
            var derived = blake2B.ComputeHash(seed);

            ext.chaincode = derived.Skip(32).Take(32).ToArray();

            var keyBytes = derived.Take(32).ToArray();

            ext.key = SecretKey.from_slice(secp, keyBytes);

            ext.root_key_id = ext.identifier(secp);

            return ext;
        }

        /// Return the identifier of the key
        /// which is the blake2b (10 byte) digest of the PublicKey
        // corresponding to the underlying SecretKey
        public Identifier identifier(Secp256k1 secp)
        {
            // get public key from private
            var key_id = PublicKey.from_secret_key(secp, key);
            
            return Identifier.from_key_id(secp, key_id);

     
        }

        /// Derive an extended key from an extended key
        public ExtendedKey derive(Secp256k1 secp, uint n)

        {
            var n_bytes = BitConverter.GetBytes(n);
            Array.Reverse(n_bytes);

            var seed = key;
            seed.extend_from_slice(n_bytes);

            var blake2b = new HMACBlake2B(chaincode, 64);

            var derived = blake2b.ComputeHash(seed.Value);

            var secret_key = SecretKey.from_slice(secp, derived);

            secret_key.add_assign(secp, key);
            //.expect("Error deriving key")


            // TODO check if key != 0 ?


            chaincode = derived.Skip(32).Take(32).ToArray();

            return new ExtendedKey
            {
                depth = depth,
                root_key_id = identifier(secp),
                n_child = n,
                chaincode = chaincode,
                key = secret_key
            };
        }
    }
}