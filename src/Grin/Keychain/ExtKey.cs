
using System;
using System.Linq;
using Grin.Util;
using Konscious.Security.Cryptography;
using Microsoft.Azure.KeyVault.Models;

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


        public void from_key_id(byte[] pubKey)
        {
            var hashAlgorithm = new HMACBlake2B(pubKey, IDENTIFIER_SIZE);
            from_bytes(hashAlgorithm.Key);

        }

        public void from_hex(string hex)
        {
            var bytes = HexUtil.from_hex(hex);
            from_bytes(bytes);
        }

        public void from_bytes(byte[] bytes)
        {
       

            var min = IDENTIFIER_SIZE < bytes.Length ? IDENTIFIER_SIZE : bytes.Length;

            for (var i = 0; i < min; i++)
                Bytes[i] = bytes[i];

            Hex = Hex ?? HexUtil.to_hex(Bytes);

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
        public Int32 n_child { get; set; }
        /// Root key identifier
        public Identifier root_key_id { get; set; }
        /// Code of the derivation chain
        public byte[] chaincode { get; set; }
        /// Actual private key
        public byte[] key { get; set; }


    public void from_slice( byte[] slice)
        { 

        // TODO change when ser. ext. size is fixed
            if (slice.Length != 79)
            {
                throw new Exception("InvalidSliceSize");

            }
 
           depth = slice[0];

           var rootKeyBytes= slice.Skip(1).Take(10).ToArray();
           root_key_id = new Identifier(rootKeyBytes);

            var nchildBytes = slice.Skip(11).Take(4).ToArray();
            Array.Reverse(nchildBytes);
            n_child = BitConverter.ToInt32(nchildBytes,0);

            chaincode = slice.Skip(15).Take(32).ToArray();

            var keyBytes = slice.Skip(47).Take(32).ToArray();

            // crypto bit here

            key = keyBytes;
            //let key = match SecretKey::from_slice(secp, &slice[47..79]) {
            //    Ok(key) => key,
            //    Err(_) => return Err(Error::InvalidExtendedKey),

	}

        /// Creates a new extended master key from a seed
        public void from_seed( byte[] seed)
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


            depth = 0;
            root_key_id = new Identifier();
            n_child = 0;

            var blake2b = new  HMACBlake2B(seed, 64);
            
            var derived = blake2b.ComputeHash(seed);
            
            chaincode = derived.Skip(32).Take(32).ToArray();

            var keyBytes = derived.Take(32).ToArray();

            // crypto bit here

            key = keyBytes;

            root_key_id = identifier() ;

    }

        /// Return the identifier of the key
        /// which is the blake2b (10 byte) digest of the PublicKey
        // corresponding to the underlying SecretKey
        public Identifier identifier()
        { 
        // get public key from private
		var key_id =key;

           var identifier = new Identifier();
            identifier.from_key_id(key_id);

            return identifier;


        }

        /// Derive an extended key from an extended key
        public void derive(Int32 n)

        {
    var	 n_bytes = BitConverter.GetBytes(n);
            Array.Reverse(n_bytes);

            var seed = key;


            //seed.extend_from_slice(&n_bytes);
            var blake2b = new HMACBlake2B(seed, 64);

            var derived = blake2b.ComputeHash(seed);

       // let mut secret_key = SecretKey::from_slice(&secp, &derived.as_bytes()[0..32])
    			//.expect("Error deriving key");
       // secret_key.add_assign(secp, &self.key).expect("Error deriving key",
    

 
        // TODO check if key != 0 ?


 
            chaincode = derived.Skip(32).Take(32).ToArray();


    	}




}







}