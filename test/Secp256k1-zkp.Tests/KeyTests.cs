using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Common;
using Secp256k1Proxy.Key;
using Secp256k1Proxy.Lib;
using Xunit;

namespace Secp256k1Proxy.Tests
{
    public class KeyTests
    {
        [Fact]
        public void Skey_from_slice()
        {
            var secp256K1 = Secp256K1.New();

            // ReSharper disable once JoinDeclarationAndInitializer
            SecretKey sk;

            var ex = Assert.Throws<Exception>(
                () => { sk = SecretKey.From_slice(secp256K1, ByteUtil.Get_bytes(1, 31)); });

            Assert.Equal("InvalidSecretKey", ex.Message);

            sk = SecretKey.From_slice(secp256K1, ByteUtil.Get_bytes(1, 32));

            Assert.NotEmpty(sk.Value);
        }


        [Fact]
        public void Pubkey_from_slice()
        {
            var secp256K1 = Secp256K1.New();

            var ex = Assert.Throws<Exception>(() => { PublicKey.from_slice(secp256K1, null); });
            Assert.Equal("InvalidPublicKey", ex.Message);

            ex = Assert.Throws<Exception>(() => { PublicKey.from_slice(secp256K1, new byte[] {1, 2, 3}); });
            Assert.Equal("InvalidPublicKey", ex.Message);


            var uncompressed = PublicKey.from_slice(secp256K1, new byte[]
            {
                4, 54, 57, 149, 239, 162, 148, 175, 246, 254, 239, 75, 154, 152, 10, 82, 234, 224, 85, 220, 40, 100, 57,
                121, 30, 162, 94, 156, 135, 67, 74, 49, 179, 57, 236, 53, 162, 124, 149, 144, 168, 77, 74, 30, 72, 211,
                229, 110, 111, 55, 96, 193, 86, 227, 183, 152, 195, 155, 51, 247, 123, 113, 60, 228, 188
            });
            Assert.NotEmpty(uncompressed.Value);


            var compressed = PublicKey.from_slice(secp256K1, new byte[]
            {
                3, 23, 183, 225, 206, 31, 159, 148, 195, 42, 67, 115, 146, 41, 248, 140, 11, 3, 51, 41, 111, 180, 110,
                143, 114, 134, 88, 73, 198, 174, 52, 184, 78
            });
            Assert.NotEmpty(compressed.Value);
        }


        [Fact]
        public void Keypair_slice_round_trip()
        {
            var secp256K1 = Secp256K1.New();

            // ReSharper disable once JoinDeclarationAndInitializer
            SecretKey sk;

            var ex = Assert.Throws<Exception>(
                () => { sk = SecretKey.From_slice(secp256K1, ByteUtil.Get_bytes(1, 31)); });

            Assert.Equal("InvalidSecretKey", ex.Message);

            sk = SecretKey.From_slice(secp256K1, ByteUtil.Get_bytes(1, 32));

            Assert.NotEmpty(sk.Value);
        }


        [Fact]
        public void Invalid_secret_key()
        {
            var s = Secp256K1.New();

            // Zero
            var ex = Assert.Throws<Exception>(() => { SecretKey.From_slice(s, ByteUtil.Get_bytes(0, 32)); });
            Assert.Equal("InvalidSecretKey", ex.Message);
            // -1
            ex = Assert.Throws<Exception>(() => { SecretKey.From_slice(s, ByteUtil.Get_bytes(0xff, 32)); });
            Assert.Equal("InvalidSecretKey", ex.Message);

            // Top of range
            var sk1 = SecretKey.From_slice(s, new byte[]
            {
                0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFE,
                0xBA, 0xAE, 0xDC, 0xE6, 0xAF, 0x48, 0xA0, 0x3B,
                0xBF, 0xD2, 0x5E, 0x8C, 0xD0, 0x36, 0x41, 0x40
            });

            Assert.NotEmpty(sk1.Value);

            // One past top of range
            Assert.Throws<Exception>(() =>
            {
                SecretKey.From_slice(s, new byte[]
                {
                    0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF,
                    0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFE,
                    0xBA, 0xAE, 0xDC, 0xE6, 0xAF, 0x48, 0xA0, 0x3B,
                    0xBF, 0xD2, 0x5E, 0x8C, 0xD0, 0x36, 0x41, 0x41
                });
            });
        }


        [Fact]
        public void Test_pubkey_from_slice_bad_context()
        {
            var s = Secp256K1.WithoutCaps();
            var sk = SecretKey.New(s, RandomNumberGenerator.Create());


            var ex = Assert.Throws<Exception>(() => { PublicKey.from_secret_key(s, sk); });
            Assert.Equal("IncapableContext", ex.Message);

            s = Secp256K1.WithCaps(ContextFlag.VerifyOnly);
            ex = Assert.Throws<Exception>(() => { PublicKey.from_secret_key(s, sk); });
            Assert.Equal("IncapableContext", ex.Message);

            s = Secp256K1.WithCaps(ContextFlag.SignOnly);
            PublicKey.from_secret_key(s, sk);

            s = Secp256K1.WithCaps(ContextFlag.Full);
            PublicKey.from_secret_key(s, sk);
        }

        [Fact]
        public void Test_add_exp_bad_context()
        {
            var s = Secp256K1.WithCaps(ContextFlag.Full);
            var (sk, pk) = s.generate_keypair(RandomNumberGenerator.Create());

            pk.add_exp_assign(s, sk);


            s = Secp256K1.WithCaps(ContextFlag.VerifyOnly);
            pk.add_exp_assign(s, sk);

            s = Secp256K1.WithCaps(ContextFlag.SignOnly);
            var ex = Assert.Throws<Exception>(() => { pk.add_exp_assign(s, sk); });
            Assert.Equal("IncapableContext", ex.Message);


            s = Secp256K1.WithCaps(ContextFlag.None);
            ex = Assert.Throws<Exception>(() => { pk.add_exp_assign(s, sk); });
            Assert.Equal("IncapableContext", ex.Message);
        }

        [Fact]
        public void Test_bad_deserialize()
        {
            //use std.io.Cursor;
            //use serialize.{ json, Decodable};

            //var zero31 = "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]".as_bytes();
            //var json31 = json.Json.from_reader(& Cursor.new(zero31)).unwrap();
            //var zero32 = "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]".as_bytes();
            //var json32 = json.Json.from_reader(& Cursor.new(zero32)).unwrap();
            //var zero65 = "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]".as_bytes();
            //var json65 = json.Json.from_reader(& Cursor.new(zero65)).unwrap();
            //var string = "\"my key\"".as_bytes();
            //var json = json.Json.from_reader(& Cursor.new(string)).unwrap();

            //// Invalid length
            //var  decoder = json.Decoder.new(json31.clone());
            //assert!(< PublicKey as Decodable >.decode(& decoder).is_err());
            //var  decoder = json.Decoder.new(json31.clone());
            //assert!(< SecretKey as Decodable >.decode(& decoder).is_err());
            //var  decoder = json.Decoder.new(json32.clone());
            //assert!(< PublicKey as Decodable >.decode(& decoder).is_err());
            //var  decoder = json.Decoder.new(json32.clone());
            //assert!(< SecretKey as Decodable >.decode(& decoder).is_ok());
            //var  decoder = json.Decoder.new(json65.clone());
            //assert!(< PublicKey as Decodable >.decode(& decoder).is_err());
            //var  decoder = json.Decoder.new(json65.clone());
            //assert!(< SecretKey as Decodable >.decode(& decoder).is_err());

            //// Syntax error
            //var  decoder = json.Decoder.new(json.clone());
            //assert!(< PublicKey as Decodable >.decode(& decoder).is_err());
            //var  decoder = json.Decoder.new(json.clone());
            //assert!(< SecretKey as Decodable >.decode(& decoder).is_err());
        }

        [Fact]
        public void Test_serialize()
        {
            //use std.io.Cursor;
            //use serialize.{ json, Decodable, Encodable};

            //macro_rules!round_trip(
            //    ($var: ident) => ({
            //    var start = $var;
            //    var  encoded = String.new();
            //    {
            //        var  encoder = json.Encoder.new(&  encoded);
            //        start.encode(& encoder).unwrap();
            //    }
            //    var json = json.Json.from_reader(& Cursor.new(encoded.as_bytes())).unwrap();
            //    var  decoder = json.Decoder.new(json);
            //    var decoded = Decodable.decode(& decoder);
            //    assert_eq!(Ok(Some(start)), decoded);
            //})
            //);

            //    var s = Secp256k1.new();
            //    for _ in 0..500 {
            //        var(sk, pk) = s.generate_keypair(& thread_rng()).unwrap();
            //        round_trip!(sk);
            //        round_trip!(pk);
            //    }
        }

        [Fact]
        public void Test_bad_serde_deserialize()
        {
            //use serde.Deserialize;
            //use json;

            //// Invalid length
            //var zero31 = "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]";
            //var  json = json.de.Deserializer.from_str(zero31);
            //assert!(< PublicKey as Deserialize >.deserialize(& json).is_err());
            //var  json = json.de.Deserializer.from_str(zero31);
            //assert!(< SecretKey as Deserialize >.deserialize(& json).is_err());

            //var zero32 = "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]";
            //var  json = json.de.Deserializer.from_str(zero32);
            //assert!(< PublicKey as Deserialize >.deserialize(& json).is_err());
            //var  json = json.de.Deserializer.from_str(zero32);
            //assert!(< SecretKey as Deserialize >.deserialize(& json).is_ok());

            //var zero33 = "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]";
            //var  json = json.de.Deserializer.from_str(zero33);
            //assert!(< PublicKey as Deserialize >.deserialize(& json).is_err());
            //var  json = json.de.Deserializer.from_str(zero33);
            //assert!(< SecretKey as Deserialize >.deserialize(& json).is_err());

            //var trailing66 = "[4,149,16,196,140,38,92,239,179,65,59,224,230,183,91,238,240,46,186,252,
            //                175,102,52,249,98,178,123,72,50,171,196,254,236,1,189,143,242,227,16,87,
            //            247,183,162,68,237,140,92,205,151,129,166,58,111,96,123,64,180,147,51,12,
            //            209,89,236,213,206,17]";
            //var  json = json.de.Deserializer.from_str(trailing66);
            //assert!(< PublicKey as Deserialize >.deserialize(& json).is_err());

            //// The first 65 bytes of trailing66 are valid
            //var valid65 = "[4,149,16,196,140,38,92,239,179,65,59,224,230,183,91,238,240,46,186,252,
            //                175,102,52,249,98,178,123,72,50,171,196,254,236,1,189,143,242,227,16,87,
            //            247,183,162,68,237,140,92,205,151,129,166,58,111,96,123,64,180,147,51,12,
            //            209,89,236,213,206]";
            //var  json = json.de.Deserializer.from_str(valid65);
            //assert!(< PublicKey as Deserialize >.deserialize(& json).is_ok());

            //// All zeroes pk is invalid
            //var zero65 = "[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0]";
            //var  json = json.de.Deserializer.from_str(zero65);
            //assert!(< PublicKey as Deserialize >.deserialize(& json).is_err());
            //var  json = json.de.Deserializer.from_str(zero65);
            //assert!(< SecretKey as Deserialize >.deserialize(& json).is_err());

            //// Syntax error
            //var string = "\"my key\"";
            //var  json = json.de.Deserializer.from_str(string);
            //assert!(< PublicKey as Deserialize >.deserialize(& json).is_err());
            //var  json = json.de.Deserializer.from_str(string);
            //assert!(< SecretKey as Deserialize >.deserialize(& json).is_err());
        }


        [Fact]
        public void Test_serialize_serde()
        {
            //var s = Secp256k1.new();
            //for _ in 0..500 {
            //    var(sk, pk) = s.generate_keypair(& thread_rng()).unwrap();
            //    round_trip_serde!(sk);
            //    round_trip_serde!(pk);
            //}
        }

        [Fact]
        public void Test_out_of_range()
        {
            //struct BadRng(u8);
            //impl Rng for BadRng
            //{
            //    fn
            //    next_u32(&self)->u32 {
            //        unimplemented!()
            //    }
            //}


            //var s = Secp256k1.new();
            //s.generate_keypair(&BadRng(0xff)).unwrap();
        }

        //private void Ext_u32()
        //{
        //}


        // This will set a secret key to a little over the
        // group order, then decrement with repeated calls
        // until it returns a valid key

        //private void Fill_bytes(byte[] data)
        //{
        //    var groupOrder = new byte[]
        //    {
        //        0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
        //        0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xfe,
        //        0xba, 0xae, 0xdc, 0xe6, 0xaf, 0x48, 0xa0, 0x3b,
        //        0xbf, 0xd2, 0x5e, 0x8c, 0xd0, 0x36, 0x41, 0x41
        //    };

        //    //assert_eq!(data.len(), 32);
        //    //data.copy_from_slice(&group_order[..]);
        //    //data[31] = self.0;
        //    //self.0 -= 1;
        //}

        [Fact]
        public void Test_pubkey_from_bad_slice()
        {
            var s = Secp256K1.New();

            // Bad sizes
            Assert.Throws<Exception>(() =>
            {
                PublicKey.from_slice(s, ByteUtil.Get_bytes(0, Constants.Constants.PublicKeySize - 1));
            });

            Assert.Throws<Exception>(() =>
            {
                PublicKey.from_slice(s, ByteUtil.Get_bytes(0, Constants.Constants.PublicKeySize + 1));
            });

            Assert.Throws<Exception>(() =>
            {
                PublicKey.from_slice(s, ByteUtil.Get_bytes(0, Constants.Constants.CompressedPublicKeySize + 1));
            });

            Assert.Throws<Exception>(() =>
            {
                PublicKey.from_slice(s,
                    ByteUtil.Get_bytes(0, Constants.Constants.UncompressedPublicKeySize - 1));
            });

            Assert.Throws<Exception>(() =>
            {
                PublicKey.from_slice(s,
                    ByteUtil.Get_bytes(0, Constants.Constants.UncompressedPublicKeySize + 1));
            });

            //// Bad parse

            Assert.Throws<Exception>(() =>
            {
                PublicKey.from_slice(s, ByteUtil.Get_bytes(0xff, Constants.Constants.UncompressedPublicKeySize));
            });

            Assert.Throws<Exception>(() =>
            {
                PublicKey.from_slice(s, ByteUtil.Get_bytes(0x55, Constants.Constants.CompressedPublicKeySize));
            });
        }

        [Fact]
        public void Test_debug_output()
        {
            //struct DumbRng(u32);
            //impl Rng for DumbRng
            //{
            //    fn
            //    next_u32(&self)->u32 {
            //        self.0 = self.0.wrapping_add(1);
            //        self.0
            //    }
            //}

            //var s = Secp256k1.new();
            //var(sk, _) = s.generate_keypair(&DumbRng(0)).unwrap();

            //assert_eq!(&format!("{:?}", sk),
            //"SecretKey(0200000001000000040000000300000006000000050000000800000007000000)");
        }

        [Fact]
        public void Test_pubkey_serialize()
        {
            //struct DumbRng(u32);
            //impl Rng for DumbRng
            //{
            //    fn
            //    next_u32(&self)->u32 {
            //        self.0 = self.0.wrapping_add(1);
            //        self.0
            //    }
            //}

            //var s = Secp256k1.new();
            //var(_, pk1) = s.generate_keypair(&DumbRng(0)).unwrap();
            //assert_eq!(&pk1.serialize_vec(&s, false)[..],
            //&[4, 149, 16, 196, 140, 38, 92, 239, 179, 65, 59, 224, 230, 183, 91, 238, 240, 46, 186, 252, 175, 102, 52, 249, 98, 178, 123, 72, 50, 171, 196, 254, 236, 1, 189, 143, 242, 227, 16, 87, 247, 183, 162, 68, 237, 140, 92, 205, 151, 129, 166, 58, 111, 96, 123, 64, 180, 147, 51, 12, 209, 89, 236, 213, 206]
            //    [..]);
            //assert_eq!(&pk1.serialize_vec(&s, true)[..],
            //&[2, 149, 16, 196, 140, 38, 92, 239, 179, 65, 59, 224, 230, 183, 91, 238, 240, 46, 186, 252, 175, 102, 52, 249, 98, 178, 123, 72, 50, 171, 196, 254, 236]
            //    [..]);
        }

        [Fact]
        public void Test_addition()
        {
            var s = Secp256K1.New();

            var( sk1, pk1) = s.generate_keypair(RandomNumberGenerator.Create());
            var( sk2, pk2) = s.generate_keypair(RandomNumberGenerator.Create());

            Assert.Equal(PublicKey.from_secret_key(s, sk1).Value, pk1.Value);
            sk1.Add_assign(s, sk2);
            pk1.add_exp_assign(s, sk2);
            Assert.Equal(PublicKey.from_secret_key(s, sk1).Value, pk1.Value);

            Assert.Equal(PublicKey.from_secret_key(s, sk2).Value, pk2.Value);
            sk2.Add_assign(s, sk1);
            pk2.add_exp_assign(s, sk1);
            Assert.Equal(PublicKey.from_secret_key(s, sk2).Value, pk2.Value);
        }

        [Fact]
        public void Test_multiplication()
        {
            var s = Secp256K1.New();

            var( sk1, pk1) = s.generate_keypair(RandomNumberGenerator.Create());
            var( sk2, pk2) = s.generate_keypair(RandomNumberGenerator.Create());

            Assert.Equal(PublicKey.from_secret_key(s, sk1).Value, pk1.Value);
            sk1.Mul_assign(s, sk2);
            pk1.mul_assign(s, sk2);
            Assert.Equal(PublicKey.from_secret_key(s, sk1).Value, pk1.Value);

            var pp = PublicKey.from_secret_key(s, sk2);
            Assert.Equal(pp.Value, pk2.Value);
            sk2.Mul_assign(s, sk1);
            pk2.mul_assign(s, sk1);
            Assert.Equal(PublicKey.from_secret_key(s, sk2).Value, pk2.Value);
        }

        [Fact]
        public void Pubkey_hash()
        {
            var s = Secp256K1.New();
            var set = new HashSet<byte[]>();

            const int usize = 1024;
            for (var i = 0; i <= usize; i++)
            {
                var kp = s.generate_keypair(RandomNumberGenerator.Create());

                var hash = Hash(kp.publicKey.Value);

                Assert.DoesNotContain(hash, set);

                set.Add(hash);
            }
        }

        private static byte[] Hash(byte[] data)

        {
            using (var md5Hash = MD5.Create())
            {
                return md5Hash.ComputeHash(data);
            }
        }
    }
}