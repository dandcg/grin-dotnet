using System;
using System.IO;
using Grin.Core;
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

        [Fact]
        public void simple_tx_ser()
        {
            var tx = tx2i1o();
            var vec = new MemoryStream();
            Ser.serialize(vec, tx);
            Console.WriteLine(vec.Length);
            Assert.True(vec.Length > 5360);
            Assert.True(vec.Length < 5380);
        }


        // utility producing a transaction with 2 inputs and a single outputs
        public Transaction tx2i1o()
        {
            var keychain = Keychain.Keychain.From_random_seed();
            var key_id1 = keychain.Derive_key_id(1);
            var key_id2 = keychain.Derive_key_id(2);
            var key_id3 = keychain.Derive_key_id(3);

            var (tx, _) = Build.transaction(new Func<Context, Append>[]

                {
                    c => c.input(10, key_id1),
                    c => c.input(11, key_id2),
                    c => c.output(19, key_id3),
                    c => c.with_fee(2)
                },
                keychain
            );

            return tx;
        }

        // utility producing a transaction with a single input and output
        public Transaction tx1i1o()
        {
            var keychain = Keychain.Keychain.From_random_seed();
            var key_id1 = keychain.Derive_key_id(1);
            var key_id2 = keychain.Derive_key_id(2);

            var (tx, _) = Build.transaction(new Func<Context, Append>[]

                {
                    c => c.input(5, key_id1),
                    c => c.output(3, key_id2),
                    c => c.with_fee(2)
                },
                keychain
            );

            return tx;
        }
    }
}