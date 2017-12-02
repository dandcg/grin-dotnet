using System.Security.Cryptography;
using Secp256k1Proxy.Ecdh;
using Secp256k1Proxy.Lib;
using Xunit;

namespace Secp256k1Proxy.Tests
{
    public class EcdhTests
    {
        [Fact]
        public void Ecdh()
        {
            var s = Secp256K1.WithCaps(ContextFlag.SignOnly);
            var (sk1, pk1) = s.generate_keypair(RandomNumberGenerator.Create());
            var (sk2, pk2) = s.generate_keypair(RandomNumberGenerator.Create());

            var sec1 = SharedSecret.New(s, pk1, sk2);
            var sec2 = SharedSecret.New(s, pk2, sk1);
            var secOdd = SharedSecret.New(s, pk1, sk1);

            Assert.Equal(sec1.Value, sec2.Value);
            Assert.NotEqual(secOdd.Value, sec2.Value);
        }
    }
}