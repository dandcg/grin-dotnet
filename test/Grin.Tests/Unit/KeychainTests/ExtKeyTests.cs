using Common;
using Grin.KeychainImpl.ExtKey;
using Newtonsoft.Json;
using Secp256k1Proxy.Key;
using Secp256k1Proxy.Lib;
using Xunit;

namespace Grin.Tests.Unit.KeychainTests
{
    public class ExtKeyTests : IClassFixture<LoggingFixture>
    {
 
        public class HasAnIdentifier
        {
            [JsonProperty("identifier")]
            public string Identifier { get; set; }
        }


        [Fact]
        public void test_identifier_json_ser_deser()
        {
            var hex = "942b6c0bd43bdcb24f3edfe7fadbc77054ecc4f2";

            var identifier = Identifier.From_hex(hex);

            var hasAnIdentifier = new HasAnIdentifier {Identifier = identifier.Hex};

            var json = JsonConvert.SerializeObject(hasAnIdentifier);
            Assert.Equal("{\"identifier\":\"942b6c0bd43bdcb24f3e\"}", json);

            var deserialized = JsonConvert.DeserializeObject<HasAnIdentifier>(json);
            Assert.Equal(hasAnIdentifier.Identifier, deserialized.Identifier);
        }


        [Fact]
        public void extkey_from_seed()
        {
            // TODO More test vectors
            var s = Secp256k1.New();
            var seed = HexUtil.from_hex("000102030405060708090a0b0c0d0e0f");
            var extk = ExtendedKey.from_seed(s, seed);
            var sec = HexUtil.from_hex("c3f5ae520f474b390a637de4669c84d0ed9bbc21742577fac930834d3c3083dd");
            var secretKey = SecretKey.From_slice(s, sec);
            var chaincode = HexUtil.from_hex("e7298e68452b0c6d54837670896e1aee76b118075150d90d4ee416ece106ae72");


            var identifier = HexUtil.from_hex("83e59c48297b78b34b73");
            var depth = 0;
            uint nChild = 0;

            Assert.Equal(extk.Key.Value, secretKey.Value);
            Assert.Equal(extk.Identifier(s).Bytes, Identifier.From_bytes(identifier).Bytes);
            Assert.Equal(
                extk.RootKeyId.Bytes,
                Identifier.From_bytes(identifier).Bytes
            );
            Assert.Equal(extk.Chaincode, chaincode);
            Assert.Equal(extk.Depth, depth);
            Assert.Equal(extk.NChild, nChild);
        }

        [Fact]
        public void extkey_derivation()
        {
            // TODO More test vectors
            var s = Secp256k1.New();
            var seed = HexUtil.from_hex("000102030405060708090a0b0c0d0e0f");
            var extk = ExtendedKey.from_seed(s, seed);
            var derived = extk.Derive(s, 0);
            var sec = HexUtil.from_hex("d75f70beb2bd3b56f9b064087934bdedee98e4b5aae6280c58b4eff38847888f"
            );
            var secretKey = SecretKey.From_slice(s, sec);
            var chaincode = HexUtil.from_hex("243cb881e1549e714db31d23af45540b13ad07941f64a786bbf3313b4de1df52");
            var rootKeyId = HexUtil.from_hex("83e59c48297b78b34b73");
            var identifier = HexUtil.from_hex("0185adb4d8b730099c93");
            var depth = 1;
            uint nChild = 0;
            Assert.Equal(derived.Key.Value, secretKey.Value);
            Assert.Equal(
                derived.Identifier(s).Bytes,
                Identifier.From_bytes(identifier).Bytes
            );
            Assert.Equal(
                derived.RootKeyId.Bytes,
                Identifier.From_bytes(rootKeyId).Bytes
            );
            Assert.Equal(derived.Chaincode, chaincode);
            Assert.Equal(derived.Depth, depth);
            Assert.Equal(derived.NChild, nChild);
        }
    }
}