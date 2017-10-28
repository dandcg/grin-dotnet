using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Grin.Util;
using Microsoft.AspNetCore.Server.Kestrel.Internal.System.Collections.Sequences;
using Microsoft.WindowsAzure.Storage.Blob.Protocol;

namespace Grin.Keychain
{
    public class Identifier
    {

        public const  int IDENTIFIER_SIZE = 10;

        public Identifier()
        {
        Value = new byte[IDENTIFIER_SIZE];
        }
       public byte[] Value { get; private set; }

        public void Serialize()
        {
           
        }

        public void Deserialize()
        {

        }


        //pub fn zero() -> Identifier {
        //    Identifier::from_bytes(&[0; IDENTIFIER_SIZE])
        //}

        //pub fn from_bytes(bytes: &[u8]) -> Identifier {
        //    let mut identifier = [0; IDENTIFIER_SIZE];
        //    for i in 0..min(IDENTIFIER_SIZE, bytes.len())
        //    {
        //        identifier[i] = bytes[i];
        //    }

        //    Identifier(identifier)

        //}

        //pub fn from_key_id(secp: &Secp256k1, pubkey: &PublicKey) -> Identifier {
        //    let bytes = pubkey.serialize_vec(secp, true);
        //    let identifier = blake2b(IDENTIFIER_SIZE, &[], &bytes[..]);
        //    Identifier::from_bytes(&identifier.as_bytes())
        //}

    public static Identifier from_hex(string hex)
    {
        var bytes = HexUtil.from_hex(hex);
        return Identifier.from_bytes(bytes);
    }

        //pub fn to_hex(&self) -> String {
        //    util::to_hex(self.0.to_vec())
        //}


       public static Identifier from_bytes(byte[] bytes)
        {
            var identifier = new Identifier();

            var min = IDENTIFIER_SIZE < bytes.Length ? IDENTIFIER_SIZE : bytes.Length;

            for (int i=0; i<min; i++)
            {
                identifier.Value[i] = bytes[i];
            }
            return identifier;

        }




}


    public class IdentifierVisitor
    {
        
    }



}
