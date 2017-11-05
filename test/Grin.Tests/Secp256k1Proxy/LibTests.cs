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


    }
}
