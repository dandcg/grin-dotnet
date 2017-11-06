using System;
using Grin.Secp256k1Proxy;
using Xunit;

namespace Secp256k1Proxy.Tests
{
    public class KeyTests
    {
        [Fact]
        public void skey_from_slice()
        {
            var secp256k1 = Secp256k1.New();

            SecretKey sk = null;

            var ex = Assert.Throws<Exception>(() => { sk = SecretKey.from_slice(secp256k1 ,KeyUtils.get_bytes(1, 31)); });

            Assert.Equal("InvalidSecretKey", ex.Message);

            sk = SecretKey.from_slice(secp256k1 ,KeyUtils.get_bytes(1, 32));

            Assert.NotEmpty(sk.Value);
        }


        [Fact]
        public void pubkey_from_slice()
        {
            var secp256k1 = Secp256k1.New();

            Exception ex = null;

            ex = Assert.Throws<Exception>(() => { PublicKey.from_slice(secp256k1,null); });
            Assert.Equal("InvalidPublicKey", ex.Message);

            ex = Assert.Throws<Exception>(() => { PublicKey.from_slice(secp256k1,new byte[] {1, 2, 3}); });
            Assert.Equal("InvalidPublicKey", ex.Message);


            var uncompressed = PublicKey.from_slice(secp256k1,new byte[]
            {
                4, 54, 57, 149, 239, 162, 148, 175, 246, 254, 239, 75, 154, 152, 10, 82, 234, 224, 85, 220, 40, 100, 57,
                121, 30, 162, 94, 156, 135, 67, 74, 49, 179, 57, 236, 53, 162, 124, 149, 144, 168, 77, 74, 30, 72, 211,
                229, 110, 111, 55, 96, 193, 86, 227, 183, 152, 195, 155, 51, 247, 123, 113, 60, 228, 188
            });
            Assert.NotEmpty(uncompressed.Value);


            var compressed = PublicKey.from_slice(secp256k1,new byte[]
            {
                3, 23, 183, 225, 206, 31, 159, 148, 195, 42, 67, 115, 146, 41, 248, 140, 11, 3, 51, 41, 111, 180, 110,
                143, 114, 134, 88, 73, 198, 174, 52, 184, 78
            });
            Assert.NotEmpty(compressed.Value);
        }


        [Fact]
        public void keypair_slice_round_trip()
        {
            var secp256k1 = Secp256k1.New();

            SecretKey sk = null;

            var ex = Assert.Throws<Exception>(() => { sk = SecretKey.from_slice(secp256k1, KeyUtils.get_bytes(1, 31)); });

            Assert.Equal("InvalidSecretKey", ex.Message);

            sk = SecretKey.from_slice(secp256k1, KeyUtils.get_bytes(1, 32));

            Assert.NotEmpty(sk.Value);
        }


        [Fact]
        public void invalid_secret_key()
        {
            var s = Secp256k1.New();

            Exception ex;

            // Zero
            ex = Assert.Throws<Exception>(() => { SecretKey.from_slice(s, KeyUtils.get_bytes(0, 32)); });
            Assert.Equal("InvalidSecretKey", ex.Message);
            // -1
            ex = Assert.Throws<Exception>(() => { SecretKey.from_slice(s, KeyUtils.get_bytes(0xff, 32)); });
            Assert.Equal("InvalidSecretKey", ex.Message);

            // Top of range
            var sk1 = SecretKey.from_slice(s, new byte[]
            {
                0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFE,
                0xBA, 0xAE, 0xDC, 0xE6, 0xAF, 0x48, 0xA0, 0x3B,
                0xBF, 0xD2, 0x5E, 0x8C, 0xD0, 0x36, 0x41, 0x40
            });

            Assert.NotEmpty(sk1.Value);

            // One past top of range
            ex = Assert.Throws<Exception>(() =>
            {

                SecretKey.from_slice(s, new byte[]
                {
                    0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                    0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFE,
                    0xBA, 0xAE, 0xDC, 0xE6, 0xAF, 0x48, 0xA0, 0x3B,
                    0xBF, 0xD2, 0x5E, 0x8C, 0xD0, 0x36, 0x41, 0x41
                });
            });
        }

    }
}