using System;
using System.Diagnostics;
using Grin.Keychain;
using Microsoft.AspNetCore.ResponseCaching.Internal;
using Newtonsoft.Json;
using Xunit;

namespace Grin.Tests
{
    public class ExtKeyTests
    {

        //#[cfg(test)]
        //        mod test
        //        {
        //            use serde_json;

        //            use secp::Secp256k1;
        //            use secp::key::SecretKey;
        //            use super::{ ExtendedKey, Identifier};
        //            use util;

        //            fn from_hex(hex_str: &str) -> Vec<u8> {
        //		util::from_hex(hex_str.to_string()).unwrap()

        //    }


        public class HasAnIdentifier
        {
           public Identifier Identifier { get; set; }
        }


        [Fact]
        public void test_identifier_json_ser_deser()
        {
            var hex = "942b6c0bd43bdcb24f3edfe7fadbc77054ecc4f2";
            var identifier = Identifier.from_hex(hex);

            Console.WriteLine(hex);
            foreach (var v in identifier.Value)
            {
                Console.WriteLine(v);
            }
        

            //var has_an_identifier = new HasAnIdentifier {Identifier = identifier};

            var json = JsonConvert.SerializeObject(identifier);
            Assert.Equal("{\"value\":\"942b6c0bd43bdcb24f3e\"}", json);

            //var deserialized = JsonConvert.DeserializeObject<HasAnIdentifier>(json);
            //Assert.(has_an_identifier, deserialized);
        }




        [Fact]
        public void extkey_from_seed()
{
    //// TODO More test vectors
    //let s = Secp256k1::new();
    //let seed = from_hex("000102030405060708090a0b0c0d0e0f");
    //let extk = ExtendedKey::from_seed(&s, &seed.as_slice()).unwrap();
    //let sec = from_hex(
    //    "c3f5ae520f474b390a637de4669c84d0ed9bbc21742577fac930834d3c3083dd",

    //);
    //let secret_key = SecretKey::from_slice(&s, sec.as_slice()).unwrap();
    //let chaincode = from_hex(
    //    "e7298e68452b0c6d54837670896e1aee76b118075150d90d4ee416ece106ae72",

    //);
    //let identifier = from_hex("83e59c48297b78b34b73");
    //let depth = 0;
    //let n_child = 0;
    //assert_eq!(extk.key, secret_key);
    //assert_eq!(
    //    extk.identifier(&s).unwrap(),
    //    Identifier::from_bytes(identifier.as_slice())
    //);
    //assert_eq!(
    //    extk.root_key_id,
    //    Identifier::from_bytes(identifier.as_slice())
    //);
    //assert_eq!(extk.chaincode, chaincode.as_slice());
    //assert_eq!(extk.depth, depth);
    //assert_eq!(extk.n_child, n_child);
}

        [Fact]
        public void extkey_derivation()
{
    //// TODO More test vectors
    //let s = Secp256k1::new();
    //let seed = from_hex("000102030405060708090a0b0c0d0e0f");
    //let extk = ExtendedKey::from_seed(&s, &seed.as_slice()).unwrap();
    //let derived = extk.derive(&s, 0).unwrap();
    //let sec = from_hex(
    //    "d75f70beb2bd3b56f9b064087934bdedee98e4b5aae6280c58b4eff38847888f",

    //);
    //let secret_key = SecretKey::from_slice(&s, sec.as_slice()).unwrap();
    //let chaincode = from_hex(
    //    "243cb881e1549e714db31d23af45540b13ad07941f64a786bbf3313b4de1df52",

    //);
    //let root_key_id = from_hex("83e59c48297b78b34b73");
    //let identifier = from_hex("0185adb4d8b730099c93");
    //let depth = 1;
    //let n_child = 0;
    //assert_eq!(derived.key, secret_key);
    //assert_eq!(
    //    derived.identifier(&s).unwrap(),
    //    Identifier::from_bytes(identifier.as_slice())
    //);
    //assert_eq!(
    //    derived.root_key_id,
    //    Identifier::from_bytes(root_key_id.as_slice())
    //);
    //assert_eq!(derived.chaincode, chaincode.as_slice());
    //assert_eq!(derived.depth, depth);
    //assert_eq!(derived.n_child, n_child);
}
    }
}
