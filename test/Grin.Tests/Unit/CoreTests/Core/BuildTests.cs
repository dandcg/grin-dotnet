using System;
using Grin.CoreImpl.Core.Build;
using Grin.KeychainImpl;
using Xunit;

namespace Grin.Tests.Unit.CoreTests.Core
{
    public class BuildTests : IClassFixture<LoggingFixture>
    {
        [Fact]
        public void blind_simple_tx()
        {
            var keychain = Keychain.From_random_seed();
            var key_id1 = keychain.Derive_key_id(1);
            var key_id2 = keychain.Derive_key_id(2);
            var key_id3 = keychain.Derive_key_id(3);

            var(tx, _) = Build.transaction(new Func<Context, Append>[]
            {
                c => c.input(10, key_id1),
                c => c.input(11, key_id2),
                c => c.output(20, key_id3),
                c => c.with_fee(1)
            }, keychain);

            tx.verify_sig(keychain.Secp);
        }


        [Fact]
        public void blind_simpler_tx()
        {
            var keychain = Keychain.From_random_seed();
            var key_id1 = keychain.Derive_key_id(1);
            var key_id2 = keychain.Derive_key_id(2);

            var(tx, _) = Build.transaction(new Func<Context, Append>[]
            {
                c => c.input(6, key_id1),
                c => c.output(2, key_id2),
                c => c.with_fee(4)

            }, keychain);

            tx.verify_sig(keychain.Secp);
        }
    }

}