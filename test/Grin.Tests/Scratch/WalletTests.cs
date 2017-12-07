using System;
using Common;
using Grin.KeychainImpl;
using Grin.KeychainImpl.ExtKey;
using Grin.Tests.Unit;
using Grin.WalletImpl.WalletTypes;
using Newtonsoft.Json;
using Secp256k1Proxy.Lib;
using Xunit;

namespace Grin.Tests.Scratch
{
    public class WalletTests : IClassFixture<LoggingFixture>
    {
        [Fact]
        public void TestRootKeyFromWalletSeed()
        {
            var walletSeed = WalletSeed.from_hex("51bdfa6f60d86643d3ec9042dcbcc8f1c84cc9493e5590aaa3d8166a6e737cda");
            var keychain = Keychain.From_seed(walletSeed.Value);
            Assert.Equal("fb9685eb47140b4c8c51", keychain.Extkey.RootKeyId.HexValue);
        }

        [Fact]
        public void TestExtendedKeyRootKeyFromSeed()
        {
            var s = Secp256K1.New();
            var seed = HexUtil.from_hex("51bdfa6f60d86643d3ec9042dcbcc8f1c84cc9493e5590aaa3d8166a6e737cda");
            var extk = ExtendedKey.from_seed(s, seed);
            Console.WriteLine(JsonConvert.SerializeObject(extk, Formatting.Indented));
            Assert.Equal("fb9685eb47140b4c8c51", extk.RootKeyId.HexValue);
        }
    }
}