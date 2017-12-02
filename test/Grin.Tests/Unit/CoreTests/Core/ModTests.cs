using System;
using System.IO;
using Grin.CoreImpl.Core.Block;
using Grin.CoreImpl.Core.Build;
using Grin.CoreImpl.Core.Hash;
using Grin.CoreImpl.Core.Mod;
using Grin.CoreImpl.Core.Transaction;
using Grin.CoreImpl.Ser;
using Grin.KeychainImpl;
using Grin.KeychainImpl.Blind;
using Secp256k1Proxy.Pedersen;
using Xunit;

namespace Grin.Tests.Unit.CoreTests.Core
{
    public class ModTests : IClassFixture<LoggingFixture>
    {
        [Fact]
        public void Test_amount_to_hr()
        {
            Assert.True(50123456789 == ModHelper.amount_from_hr_string("50.123456789"));
            Assert.True(50 == ModHelper.amount_from_hr_string(".000000050"));
            Assert.True(1 == ModHelper.amount_from_hr_string(".000000001"));
            Assert.True(0 == ModHelper.amount_from_hr_string(".0000000009"));
            Assert.True(500_000_000_000 == ModHelper.amount_from_hr_string("500"));
            Assert.True(5_000_000_000_000_000_000 == ModHelper.amount_from_hr_string("5000000000.00000000000"));
        }

        [Fact]
        public void Test_hr_to_amount()
        {
            Assert.Equal("50.123456789", ModHelper.amount_to_hr_string(50123456789));
            Assert.Equal("0.000000050", ModHelper.amount_to_hr_string(50));
            Assert.Equal("0.000000001", ModHelper.amount_to_hr_string(1));
            Assert.Equal("500.000000000", ModHelper.amount_to_hr_string(500_000_000_000));
            Assert.Equal("5000000000.000000000", ModHelper.amount_to_hr_string(5_000_000_000_000_000_000));
        }


        [Fact]
        public void Test_zero_commit_fails()
        {
            var keychain = Keychain.From_random_seed();
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
        public void Simple_tx_ser()
        {
            var tx = Tx2I1O();
            using (var vec = new MemoryStream())
            {
                Ser.Serialize(vec, tx);
                Console.WriteLine(vec.Length);
                Assert.True(vec.Length > 5360);
                Assert.True(vec.Length < 5380);
            }
        }

        [Fact]
        public void Simple_tx_ser_deser()
        {
            var tx = Tx2I1O();
            using (var vec = new MemoryStream())
            {
                Ser.Serialize(vec, tx);

                vec.Position = 0;

                var dtx = Ser.Deserialize(vec, Transaction.Empty());


                Assert.Equal<ulong>(2, dtx.fee);
                Assert.Equal(2, dtx.inputs.Length);
                Assert.Single(dtx.outputs);
                Assert.Equal(tx.hash(), dtx.hash());
            }
        }


        [Fact]
        public void Tx_double_ser_deser()
        {
            // checks serializing doesn't mess up the tx and produces consistent results
            var btx = Tx2I1O();
            using (var vec = new MemoryStream())
            {
                Ser.Serialize(vec, btx);
                vec.Position = 0;

                var dtx = Ser.Deserialize(vec, Transaction.Empty());

                using (var vec2 = new MemoryStream())
                {
                    Ser.Serialize(vec2, btx);
                    vec2.Position = 0;

                    var dtx2 = Ser.Deserialize(vec2, Transaction.Empty());

                    Assert.Equal(btx.hash(), dtx.hash());
                    Assert.Equal(dtx.hash(), dtx2.hash());
                }
            }
        }


        [Fact]
        public void Hash_output()
        {
            var keychain = Keychain.From_random_seed();
            var keyId1 = keychain.Derive_key_id(1);
            var keyId2 = keychain.Derive_key_id(2);
            var keyId3 = keychain.Derive_key_id(3);

            var (tx, _) = Build.transaction(new Func<Context, Append>[]
                {
                    c => c.input(75, keyId1),
                    c => c.output(42, keyId2),
                    c => c.output(32, keyId3),
                    c => c.with_fee(1)
                },
                keychain);

            var h = tx.outputs[0].hash();
            Assert.NotEqual(Hash.ZERO_HASH(), h);
            var h2 = tx.outputs[1].hash();
            Assert.NotEqual(h, h2);
        }

        [Fact]
        public void Blind_tx()
        {
            var keychain = Keychain.From_random_seed();

            var btx = Tx2I1O();
            btx.verify_sig(keychain.Secp);

            // checks that the range proof on our blind output is sufficiently hiding
            var outp = btx.outputs[0];
            var proof = outp.Proof;
            var info = keychain.Secp.range_proof_info(proof);
            Assert.Equal<ulong>(0, info.min);
            Assert.Equal(ulong.MaxValue, info.max);
        }

        [Fact]
        public void Tx_hash_diff()
        {
            var btx1 = Tx2I1O();
            var btx2 = Tx1I1O();

            if (btx1.hash().Hex == btx2.hash().Hex)
            {
                throw new Exception("diff txs have same hash");
            }
        }

        /// Simulate the standard exchange between 2 parties when creating a basic
        /// 2 inputs, 2 outputs transaction.
        [Fact]
        public void Tx_build_exchange()
        {
            var keychain = Keychain.From_random_seed();
            var keyId1 = keychain.Derive_key_id(1);
            var keyId2 = keychain.Derive_key_id(2);
            var keyId3 = keychain.Derive_key_id(3);
            var keyId4 = keychain.Derive_key_id(4);

            Transaction txAlice;
            BlindingFactor blindSum;

            {
                // Alice gets 2 of her pre-existing outputs to send 5 coins to Bob, they
                // become inputs in the new transaction
                (Func<Context, Append> in1, Func<Context, Append> in2) =
                    ( c => c.input(4, keyId1), c => c.input(3, keyId2));


                // Alice builds her transaction, with change, which also produces the sum
                // of blinding factors before they're obscured.
                var (tx, sum) = Build.transaction(new[]

                    {
                        in1,
                        in2,
                        c => c.output(1, keyId3),
                        c => c.with_fee(2)
                    },
                    keychain);

                txAlice = tx;
                blindSum = sum;
            }


            // From now on, Bob only has the obscured transaction and the sum of
            // blinding factors. He adds his output, finalizes the transaction so it's
            // ready for broadcast.
            var (txFinal, _) = Build.transaction(new Func<Context, Append>[]
                {
                    c => c.initial_tx(txAlice),
                    c => c.with_excess(blindSum),
                    c => c.output(4, keyId4)
                },
                keychain
            );

            txFinal.validate(keychain.Secp);
        }

        [Fact]
        public void Reward_empty_block()
        {
            var keychain = Keychain.From_random_seed();
            var keyId = keychain.Derive_key_id(1);

            var b = Block.New(BlockHeader.Default(), new Transaction[0], keychain, keyId);
            b.compact().validate(keychain.Secp);
        }

        [Fact]
        public void Reward_with_tx_block()
        {
            var keychain = Keychain.From_random_seed();
            var keyId = keychain.Derive_key_id(1);

            var tx1 = Tx2I1O();
            tx1.verify_sig(keychain.Secp);

            var b = Block.New(BlockHeader.Default(), new[] {tx1}, keychain, keyId);
            b.compact().validate(keychain.Secp);
        }

        [Fact]
        public void Simple_block()
        {
            var keychain = Keychain.From_random_seed();
            var keyId = keychain.Derive_key_id(1);

            var tx1 = Tx2I1O();
            var tx2 = Tx1I1O();

            var b = Block.New(
                BlockHeader.Default(),
                new[] {tx1, tx2},
                keychain,
                keyId
            );
            b.validate(keychain.Secp);
        }

        [Fact]
        public void Test_block_with_timelocked_tx()
        {
            var keychain = Keychain.From_random_seed();

            var keyId1 = keychain.Derive_key_id(1);
            var keyId2 = keychain.Derive_key_id(2);
            var keyId3 = keychain.Derive_key_id(3);

            // first check we can add a timelocked tx where lock height matches current block height
            // and that the resulting block is valid
            var (tx1, _) = Build.transaction(new Func<Context, Append>[]
                {
                    c => c.input(5, keyId1.Clone()),
                    c => c.output(3, keyId2.Clone()),
                    c => c.with_fee(2),
                    c => c.with_lock_height(1)
                },
                keychain
            );
            
            var b = Block.New(
                BlockHeader.Default(),
                new[] {tx1},
                keychain,
                keyId3.Clone()
            );
            b.validate(keychain.Secp);

            // now try adding a timelocked tx where lock height is greater than current block height
            (tx1, _) = Build.transaction(new Func<Context, Append>[]
            {
                c => c.input(5, keyId1.Clone()),
                c => c.output(3, keyId2.Clone()),
                c => c.with_fee(2),
                c => c.with_lock_height(2)
            }, keychain);
            
            b = Block.New(BlockHeader.Default(),
                new[] {tx1},
                keychain,
                keyId3.Clone()
            );

            var ex = Assert.Throws<BlockErrorException>(() => b.validate(keychain.Secp));

            Assert.Equal(BlockError.KernelLockHeight, ex.Error);
        }

        [Fact]
        public void Test_verify_1i1o_sig()
        {
            var keychain = Keychain.From_random_seed();
            var tx = Tx1I1O();
            tx.verify_sig(keychain.Secp);
        }

        [Fact]
        public void Test_verify_2i1o_sig()
        {
            var keychain = Keychain.From_random_seed();
            var tx = Tx2I1O();
            tx.verify_sig(keychain.Secp);
        }


        // utility producing a transaction with 2 inputs and a single outputs
        public static Transaction Tx2I1O()
        {
            var keychain = Keychain.From_random_seed();
            var keyId1 = keychain.Derive_key_id(1);
            var keyId2 = keychain.Derive_key_id(2);
            var keyId3 = keychain.Derive_key_id(3);

            var (tx, _) = Build.transaction(new Func<Context, Append>[]

                {
                    c => c.input(10, keyId1),
                    c => c.input(11, keyId2),
                    c => c.output(19, keyId3),
                    c => c.with_fee(2)
                },
                keychain
            );

            return tx;
        }

        // utility producing a transaction with a single input and output
        public static Transaction Tx1I1O()
        {
            var keychain = Keychain.From_random_seed();
            var keyId1 = keychain.Derive_key_id(1);
            var keyId2 = keychain.Derive_key_id(2);

            var (tx, _) = Build.transaction(new Func<Context, Append>[]

                {
                    c => c.input(5, keyId1),
                    c => c.output(3, keyId2),
                    c => c.with_fee(2)
                },
                keychain
            );

            return tx;
        }
    }
}