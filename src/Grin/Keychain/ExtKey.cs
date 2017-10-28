
using Grin.Util;
using Konscious.Security.Cryptography;

namespace Grin.Keychain
{
    public class Identifier
    {
        // const
        public const int IDENTIFIER_SIZE = 10;


        // contructor
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


   

        //pub fn from_key_id(secp: &Secp256k1, pubkey: &PublicKey) -> Identifier {
        //    let bytes = pubkey.serialize_vec(secp, true);
        //    let identifier = blake2b(IDENTIFIER_SIZE, &[], &bytes[..]);
        //    Identifier::from_bytes(&identifier.as_bytes())
        //}


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








}