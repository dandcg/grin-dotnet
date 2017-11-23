using System;
using Grin.Core.Core;
using Xunit;

namespace Grin.Tests.Unit.CoreTests.Core
{
    public class ModTests : IClassFixture<LoggingFixture>
    {
        [Fact]
//#[should_panic(expected = "InvalidSecretKey")]
        public void test_zero_commit_fails()
        {
            var keychain = Keychain.Keychain.From_random_seed();
            var keyId1 = keychain.Derive_key_id(1);

            // blinding should fail as signing with a zero r*G shouldn't work
            var ex = Assert.Throws<Exception>(() =>
            {
                Build.transaction(new Func<Context, Append>[]
                    {
                        c => c.input(10, keyId1.Clone()),
                        c => c.output(9, keyId1.Clone()),
                        c => c.with_fee(1)
                    },
                    keychain
                );
            });

            Assert.Equal("InvalidSecretKey", ex.Message);
        }
    }
}