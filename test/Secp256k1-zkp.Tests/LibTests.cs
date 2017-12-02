using System;
using System.Linq;
using System.Security.Cryptography;
using Common;
using Secp256k1Proxy.Key;
using Secp256k1Proxy.Lib;
using Xunit;


namespace Secp256k1Proxy.Tests
{
    public class LibTests
    {
        [Fact]
        public void Capabilities()
        {
            var none = Secp256k1.WithCaps(ContextFlag.None);
            var sign = Secp256k1.WithCaps(ContextFlag.SignOnly);
            var vrfy = Secp256k1.WithCaps(ContextFlag.VerifyOnly);
            var full = Secp256k1.WithCaps(ContextFlag.Full);

            var msgBytes = ByteUtil.get_bytes(0, 32);
            var msg = Message.from_slice(msgBytes);
            var rng = RandomNumberGenerator.Create();

            Exception ex = null;

            // Try key generation
            ex = Assert.Throws<Exception>(() => { none.generate_keypair(rng); });
            Assert.Equal("IncapableContext", ex.Message);
            ex = Assert.Throws<Exception>(() => { vrfy.generate_keypair(rng); });
            Assert.Equal("IncapableContext", ex.Message);

            sign.generate_keypair(rng);

            var fullKp = full.generate_keypair(rng);
            var sk = fullKp.secretKey;
            var pk = fullKp.publicKey;

            // Try signing
            ex = Assert.Throws<Exception>(() => { none.Sign(msg, sk); });
            Assert.Equal("IncapableContext", ex.Message);
            ex = Assert.Throws<Exception>(() => { vrfy.Sign(msg, sk); });
            Assert.Equal("IncapableContext", ex.Message);

            var ss = sign.Sign(msg, sk);
            var fs = full.Sign(msg, sk);
            Assert.Equal(ss.Value, fs.Value);

            ex = Assert.Throws<Exception>(() => { none.sign_recoverable(msg, sk); });
            Assert.Equal("IncapableContext", ex.Message);
            ex = Assert.Throws<Exception>(() => { vrfy.sign_recoverable(msg, sk); });
            Assert.Equal("IncapableContext", ex.Message);

            var srs = sign.sign_recoverable(msg, sk);
            var fsr = full.sign_recoverable(msg, sk);
            Assert.Equal(srs.Value, fsr.Value);

            var sig = full.Sign(msg, sk);
            var sigr = full.sign_recoverable(msg, sk);

            // Try verifying
            ex = Assert.Throws<Exception>(() => { none.Verify(msg, sig, pk); });
            Assert.Equal("IncapableContext", ex.Message);
            ex = Assert.Throws<Exception>(() => { sign.Verify(msg, sig, pk); });
            Assert.Equal("IncapableContext", ex.Message);

            vrfy.Verify(msg, sig, pk);
            full.Verify(msg, sig, pk);

            // Try pk recovery
            ex = Assert.Throws<Exception>(() => { none.Recover(msg, sigr); });
            Assert.Equal("IncapableContext", ex.Message);
            ex = Assert.Throws<Exception>(() => { sign.Recover(msg, sigr); });
            Assert.Equal("IncapableContext", ex.Message);

            var vrc = vrfy.Recover(msg, sigr);
            var frc = full.Recover(msg, sigr);

            Assert.Equal(vrc.Value, frc.Value);
            Assert.Equal(frc.Value, pk.Value);

            // Check that we can produce keys from slices with no precomputation
            var pk_slice = pk.serialize_vec(none, false);
            var sk_slice = sk.Value;
            var new_pk = PublicKey.from_slice(none, pk_slice);
            var new_sk = SecretKey.From_slice(none, sk_slice);

            Assert.Equal(sk.Value, new_sk.Value);
            Assert.Equal(pk.Value, new_pk.Value);

            none.Dispose();
            sign.Dispose();
            vrfy.Dispose();
            full.Dispose();
        }

        [Fact]
        public void recid_sanity_check()
        {
            var one = RecoveryId.from_i32(1);
            Assert.Equal(one.Value, one.clone().Value);
        }

        [Fact]
        public void invalid_pubkey()
        {
            var s = Secp256k1.New();
            var sig = RecoverableSigniture.From_compact(s, ByteUtil.get_bytes(1, 64), RecoveryId.from_i32(0));
            var pk = PublicKey.New();
            var msgBytes = ByteUtil.get_random_bytes(RandomNumberGenerator.Create());
            var msg = Message.from_slice(msgBytes);
            var ex = Assert.Throws<Exception>(() => { s.Verify(msg, sig.To_standard(s), pk); });
            Assert.Equal("InvalidPublicKey", ex.Message);
        }


        [Fact]
        public void sign()
        {
            var s = Secp256k1.New();
            s.Randomize(RandomNumberGenerator.Create());

            var one = new byte[]
            {
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1
            };

            var sk = SecretKey.From_slice(s, one);
            var msg = Message.from_slice(one);

            var sig = s.sign_recoverable(msg, sk);
            var rsig = RecoverableSigniture.From_compact(s, new byte[]
                {
                    0x66, 0x73, 0xff, 0xad, 0x21, 0x47, 0x74, 0x1f,
                    0x04, 0x77, 0x2b, 0x6f, 0x92, 0x1f, 0x0b, 0xa6,
                    0xaf, 0x0c, 0x1e, 0x77, 0xfc, 0x43, 0x9e, 0x65,
                    0xc3, 0x6d, 0xed, 0xf4, 0x09, 0x2e, 0x88, 0x98,
                    0x4c, 0x1a, 0x97, 0x16, 0x52, 0xe0, 0xad, 0xa8,
                    0x80, 0x12, 0x0e, 0xf8, 0x02, 0x5e, 0x70, 0x9f,
                    0xff, 0x20, 0x80, 0xc4, 0xa3, 0x9a, 0xae, 0x06,
                    0x8d, 0x12, 0xee, 0xd0, 0x09, 0xb6, 0x8c, 0x89
                },
                RecoveryId.from_i32(1));
            Assert.Equal(sig.Value, rsig.Value);
        }


        [Fact]
        public void signature_serialize_roundtrip()
        {
            var s = Secp256k1.New();
            s.Randomize(RandomNumberGenerator.Create());


            for (var i = 0; i <= 100; i++)
            {
                var msgBytes = ByteUtil.get_random_bytes(RandomNumberGenerator.Create());
                var msg = Message.from_slice(msgBytes);

                var kp = s.generate_keypair(RandomNumberGenerator.Create());
                var sk = kp.secretKey;

                var sig1 = s.Sign(msg, sk);
                var der = sig1.serialize_der(s);
                var sig2 = Signiture.from_der(s, der);
                Assert.Equal(sig1.Value, sig2.Value);

                var compact = sig1.serialize_compact(s);
                var sig3 = Signiture.from_compact(s, compact);

                Assert.Equal(sig1.Value, sig3.Value);

                //round_trip_serde!(sig1);

                var ex = Assert.Throws<Exception>(() => { Signiture.from_compact(s, der); });
                Assert.Equal("InvalidSignature", ex.Message);

                ex = Assert.Throws<Exception>(() => { Signiture.from_compact(s, compact.Take(5).ToArray()); });
                Assert.Equal("InvalidSignature", ex.Message);


                ex = Assert.Throws<Exception>(() => { Signiture.from_der(s, compact); });
                Assert.Equal("InvalidSignature", ex.Message);


                ex = Assert.Throws<Exception>(() => { Signiture.from_der(s, der.Take(5).ToArray()); });
                Assert.Equal("InvalidSignature", ex.Message);
            }
        }


        [Fact]
        public void signature_lax_der()
        {
            // not implemented in underlying library
            check_lax_sig(
                "304402204c2dd8a9b6f8d425fcd8ee9a20ac73b619906a6367eac6cb93e70375225ec0160220356878eff111ff3663d7e6bf08947f94443845e0dcc54961664d922f7660b80c");
            check_lax_sig(
                "304402202ea9d51c7173b1d96d331bd41b3d1b4e78e66148e64ed5992abd6ca66290321c0220628c47517e049b3e41509e9d71e480a0cdc766f8cdec265ef0017711c1b5336f");
            check_lax_sig(
                "3045022100bf8e050c85ffa1c313108ad8c482c4849027937916374617af3f2e9a881861c9022023f65814222cab09d5ec41032ce9c72ca96a5676020736614de7b78a4e55325a");
            check_lax_sig(
                "3046022100839c1fbc5304de944f697c9f4b1d01d1faeba32d751c0f7acb21ac8a0f436a72022100e89bd46bb3a5a62adc679f659b7ce876d83ee297c7a5587b2011c4fcc72eab45");
            check_lax_sig(
                "3046022100eaa5f90483eb20224616775891397d47efa64c68b969db1dacb1c30acdfc50aa022100cf9903bbefb1c8000cf482b0aeeb5af19287af20bd794de11d82716f9bae3db1");
            check_lax_sig(
                "3045022047d512bc85842ac463ca3b669b62666ab8672ee60725b6c06759e476cebdc6c102210083805e93bd941770109bcc797784a71db9e48913f702c56e60b1c3e2ff379a60");
            check_lax_sig(
                "3044022023ee4e95151b2fbbb08a72f35babe02830d14d54bd7ed1320e4751751d1baa4802206235245254f58fd1be6ff19ca291817da76da65c2f6d81d654b5185dd86b8acf");
        }


        private void check_lax_sig(string expr)
        {
            var secp = Secp256k1.WithoutCaps();
            var sigBytes = HexUtil.from_hex(expr);
            var sig = Signiture.from_der /*_lax*/(secp, sigBytes);
            Assert.NotEmpty(sig.Value);
        }


        [Fact]
        public void sign_and_verify()
        {
            var s = Secp256k1.New();
            s.Randomize(RandomNumberGenerator.Create());

            for (var i = 0; i <= 100; i++)
            {
                var msgBytes = ByteUtil.get_random_bytes(RandomNumberGenerator.Create());
                var msg = Message.from_slice(msgBytes);

                var (sk, pk) = s.generate_keypair(RandomNumberGenerator.Create());


                var sig = s.Sign(msg, sk);

                s.Verify(msg, sig, pk);
            }
        }

        [Fact]
        public void sign_and_verify_extreme()
        {
            var s = Secp256k1.New();
            s.Randomize(RandomNumberGenerator.Create());

            // Wild keys: 1, CURVE_ORDER - 1
            // Wild msgs: 0, 1, CURVE_ORDER - 1, CURVE_ORDER
            var  wild_keys =new byte[32,2];// [[0; 32]; 2];
            var  wild_msgs =new byte[32,4];// [[0; 32]; 4];

            wild_keys[0,0] = 1;
            wild_msgs[1,0] = 1;

            //TODO: Finish here
            //use constants;
            //wild_keys[1][..].copy_from_slice(&constants::CURVE_ORDER[..]);
            //wild_msgs[1][..].copy_from_slice(&constants::CURVE_ORDER[..]);
            //wild_msgs[2][..].copy_from_slice(&constants::CURVE_ORDER[..]);

            //wild_keys[1][0] -= 1;
            //wild_msgs[1][0] -= 1;

            //for key in wild_keys.iter().map(| k | SecretKey::from_slice(&s, &k[..]).unwrap()) {
            //    for msg in wild_msgs.iter().map(| m | Message::from_slice(&m[..]).unwrap()) {
            //        var sig = s.sign(&msg, &key).unwrap();
            //        var pk = PublicKey::from_secret_key(&s, &key).unwrap();
            //        assert_eq!(s.verify(&msg, &sig, &pk), Ok(()));
            //    }
            //}
        }

        [Fact]
        public void sign_and_verify_fail()
        {
            var s = Secp256k1.New();
            s.Randomize(RandomNumberGenerator.Create());

            var msgBytes = ByteUtil.get_random_bytes(RandomNumberGenerator.Create());
            var msg = Message.from_slice(msgBytes);

            var (sk, pk) = s.generate_keypair(RandomNumberGenerator.Create());

            var sigr = s.sign_recoverable(msg, sk);
            var sig = sigr.To_standard(s);

            var msgBytes2 = ByteUtil.get_random_bytes(RandomNumberGenerator.Create());
            var msg2 = Message.from_slice(msgBytes2);

            var ex = Assert.Throws<Exception>(() => { s.Verify(msg2, sig, pk); });
            Assert.Equal("IncorrectSignature", ex.Message);
            
            var recovered_key = s.Recover(msg2, sigr);
            Assert.NotEqual(recovered_key.Value, pk.Value);
        }

        [Fact]
        public void sign_with_recovery()
        {
            var s = Secp256k1.New();
            s.Randomize(RandomNumberGenerator.Create());

            var msgBytes = ByteUtil.get_random_bytes(RandomNumberGenerator.Create());
            var msg = Message.from_slice(msgBytes);

            var (sk, pk) = s.generate_keypair(RandomNumberGenerator.Create());

            var sig = s.sign_recoverable(msg, sk);

            Assert.Equal(s.Recover(msg,sig).Value,pk.Value);
        }

        [Fact]
        public void bad_recovery()
        {
            var s = Secp256k1.New();
            s.Randomize(RandomNumberGenerator.Create());

            var msgBytes = ByteUtil.get_bytes(0x55, 32);
            var msg = Message.from_slice(msgBytes);

            // Zero is not a valid sig
            var sig = RecoverableSigniture.From_compact(s, new byte[64], RecoveryId.from_i32(0));
            var ex =Assert.Throws<Exception>(() => { s.Recover(msg, sig); });
            Assert.Equal("InvalidSignature", ex.Message);

            // ...but 111..111 is
            var sig2 = RecoverableSigniture.From_compact(s, ByteUtil.get_bytes(1,64), RecoveryId.from_i32(0));
            s.Recover(msg, sig2);
        }

        [Fact]
        public void test_bad_slice()
        {
            var s = Secp256k1.New();

            Exception ex=null;
            ex = Assert.Throws<Exception>(() => { Signiture.from_der(s, new byte[Constants.Constants.MAX_SIGNATURE_SIZE + 1]);});
            Assert.Equal("InvalidSignature", ex.Message);

            ex = Assert.Throws<Exception>(() => { Signiture.from_der(s, new byte[Constants.Constants.MAX_SIGNATURE_SIZE]); });
            Assert.Equal("InvalidSignature", ex.Message);

            ex = Assert.Throws<Exception>(() => { Message.from_slice(new byte[Constants.Constants.MESSAGE_SIZE - 1]); });
            Assert.Equal("InvalidMessage", ex.Message);

            ex = Assert.Throws<Exception>(() => { Message.from_slice(new byte[Constants.Constants.MESSAGE_SIZE + 1]); });
            Assert.Equal("InvalidMessage", ex.Message);

            Message.from_slice(new byte[Constants.Constants.MESSAGE_SIZE]);
        }

        [Fact]
        public void test_debug_output()
        {
            var s = Secp256k1.New();
            var sig = RecoverableSigniture.From_compact(s, new byte[] {
                0x66, 0x73, 0xff, 0xad, 0x21, 0x47, 0x74, 0x1f,
                0x04, 0x77, 0x2b, 0x6f, 0x92, 0x1f, 0x0b, 0xa6,
                0xaf, 0x0c, 0x1e, 0x77, 0xfc, 0x43, 0x9e, 0x65,
                0xc3, 0x6d, 0xed, 0xf4, 0x09, 0x2e, 0x88, 0x98,
                0x4c, 0x1a, 0x97, 0x16, 0x52, 0xe0, 0xad, 0xa8,
                0x80, 0x12, 0x0e, 0xf8, 0x02, 0x5e, 0x70, 0x9f,
                0xff, 0x20, 0x80, 0xc4, 0xa3, 0x9a, 0xae, 0x06,
                0x8d, 0x12, 0xee, 0xd0, 0x09, 0xb6, 0x8c, 0x89},
                RecoveryId.from_i32(1));

            //TODO: Finish here
            //Assert.Equal("RecoverableSignature(98882e09f4ed6dc3659e43fc771e0cafa60b1f926f2b77041f744721adff7366898cb609d0ee128d06ae9aa3c48020ff9f705e02f80e1280a8ade05216971a4c01)", $"{sig}");

            var msg = Message.from_slice(new byte[]{1, 2, 3, 4, 5, 6, 7, 8,
                               9, 10, 11, 12, 13, 14, 15, 16,
                               17, 18, 19, 20, 21, 22, 23, 24,
                               25, 26, 27, 28, 29, 30, 31, 255});

            //TODO: Finish here
            //Assert.Equal("Message(0102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1fff)", $"{msg}");
        }

        [Fact]
        public void test_recov_sig_serialize_compact()
        {
            var s = Secp256k1.New();

            var recid_in = RecoveryId.from_i32(1);
            var bytes_in = new byte[64]
            {
                0x66, 0x73, 0xff, 0xad, 0x21, 0x47, 0x74, 0x1f,
                0x04, 0x77, 0x2b, 0x6f, 0x92, 0x1f, 0x0b, 0xa6,
                0xaf, 0x0c, 0x1e, 0x77, 0xfc, 0x43, 0x9e, 0x65,
                0xc3, 0x6d, 0xed, 0xf4, 0x09, 0x2e, 0x88, 0x98,
                0x4c, 0x1a, 0x97, 0x16, 0x52, 0xe0, 0xad, 0xa8,
                0x80, 0x12, 0x0e, 0xf8, 0x02, 0x5e, 0x70, 0x9f,
                0xff, 0x20, 0x80, 0xc4, 0xa3, 0x9a, 0xae, 0x06,
                0x8d, 0x12, 0xee, 0xd0, 0x09, 0xb6, 0x8c, 0x89
            };
            var sig = RecoverableSigniture.From_compact(
                s, bytes_in, recid_in);
            var (recid_out, bytes_out) = sig.Serialize_compact(s);
            Assert.Equal(recid_in.Value, recid_out.Value);
            Assert.Equal(bytes_in, bytes_out);
        }

        [Fact]
        public void test_recov_id_conversion_between_i32()
        {

            Assert.Throws<Exception>(() => { RecoveryId.from_i32(-1); });
            RecoveryId.from_i32(0);
            RecoveryId.from_i32(1);
            RecoveryId.from_i32(2);
            RecoveryId.from_i32(3);
            Assert.Throws<Exception>(() => { RecoveryId.from_i32(4); });

            var id0 = RecoveryId.from_i32(0);
            Assert.Equal(0, id0.Value);

            var id1 = RecoveryId.from_i32(1);
            Assert.Equal(1, id1.Value);
        }

        [Fact]
        public void test_low_s()
        {
            // nb this is a transaction on testnet
            // txid 8ccc87b72d766ab3128f03176bb1c98293f2d1f85ebfaf07b82cc81ea6891fa9
            // input number 3
            var sigBytes =HexUtil.from_hex( "3046022100839c1fbc5304de944f697c9f4b1d01d1faeba32d751c0f7acb21ac8a0f436a72022100e89bd46bb3a5a62adc679f659b7ce876d83ee297c7a5587b2011c4fcc72eab45");
            var pkBytes = HexUtil.from_hex("031ee99d2b786ab3b0991325f2de8489246a6a3fdb700f6d0511b1d80cf5f4cd43");
            var msgBytes = HexUtil.from_hex("a4965ca63b7d8562736ceec36dfa5a11bf426eb65be8ea3f7a49ae363032da0d");

            var secp = Secp256k1.New();
            var sig = Signiture.from_der(secp, sigBytes);
            var pk = PublicKey.from_slice(secp, pkBytes);
            var msg = Message.from_slice(msgBytes);

            // without normalization we expect this will fail
            var ex = Assert.Throws<Exception>(() => { secp.Verify(msg, sig, pk); });
            Assert.Equal("IncorrectSignature", ex.Message);
            // after normalization it should pass
            sig.normalize_s(secp);
            secp.Verify(msg, sig, pk);
        }



    }
}