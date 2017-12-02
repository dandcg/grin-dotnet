using System;
using Grin.CoreImpl.Core.Build;
using Grin.KeychainImpl;
using Xunit;

namespace Grin.Tests.Unit.CoreTests.Core
{
    public class BuildTests : IClassFixture<LoggingFixture>
    {
        [Fact]
        public void Blind_simple_tx()
        {
            var keychain = Keychain.From_random_seed();
            var keyId1 = keychain.Derive_key_id(1);
            var keyId2 = keychain.Derive_key_id(2);
            var keyId3 = keychain.Derive_key_id(3);

            var(tx, _) = Build.Transaction(new Func<Context, Append>[]
            {
                c => c.Input(10, keyId1),
                c => c.Input(11, keyId2),
                c => c.Output(20, keyId3),
                c => c.with_fee(1)
            }, keychain);

            tx.verify_sig(keychain.Secp);
        }


        [Fact]
        public void Blind_simpler_tx()
        {
            var keychain = Keychain.From_random_seed();
            var keyId1 = keychain.Derive_key_id(1);
            var keyId2 = keychain.Derive_key_id(2);

            var(tx, _) = Build.Transaction(new Func<Context, Append>[]
            {
                c => c.Input(6, keyId1),
                c => c.Output(2, keyId2),
                c => c.with_fee(4)

            }, keychain);

            tx.verify_sig(keychain.Secp);
        }
    }

}