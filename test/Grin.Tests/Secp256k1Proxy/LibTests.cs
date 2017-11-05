using System;
using System.Security.Cryptography;
using Grin.Secp256k1Proxy;
using Xunit;

namespace Grin.Tests.Secp256k1Proxy
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

            var msgBytes = KeyUtils.get_bytes(0, 32);
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
            var new_sk = SecretKey.from_slice(none, sk_slice);

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
            var sig = RecoverableSigniture.from_compact(s, KeyUtils.get_bytes(1, 64), RecoveryId.from_i32(0));
            var pk = PublicKey.New();
            var msgBytes = KeyUtils.random_32_bytes(RandomNumberGenerator.Create());
            var msg = Message.from_slice(msgBytes);
            var ex = Assert.Throws<Exception>(() => { s.Verify(msg, sig.to_standard(s), pk); });
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

            var sk = SecretKey.from_slice(s, one);
            var msg = Message.from_slice(one);

            var sig = s.sign_recoverable(msg, sk);
            var rsig = RecoverableSigniture.from_compact(s, new byte[]
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
    }
}