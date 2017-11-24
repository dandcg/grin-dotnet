using System;
using System.Collections.Generic;
using System.IO;
using Grin.Core;
using Grin.Core.Core;
using Grin.Keychain;
using Secp256k1Proxy;
using Xunit;

namespace Grin.Tests.Unit.CoreTests.Core
{
    public class ModTests : IClassFixture<LoggingFixture>
    {
        [Fact]
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
            using (var vec = new MemoryStream())
            {
                Ser.serialize(vec, tx);
                Console.WriteLine(vec.Length);
                Assert.True(vec.Length > 5360);
                Assert.True(vec.Length < 5380);
            }
        }

        [Fact]
        public void simple_tx_ser_deser()
        {
            var tx = tx2i1o();
            using (var vec = new MemoryStream())
            {
                Ser.serialize(vec, tx);

                vec.Position = 0;

                var dtx = Ser.deserialize(vec, Transaction.Empty());


                Assert.Equal<ulong>(2, dtx.fee);
                Assert.Equal(2, dtx.inputs.Length);
                Assert.Single(dtx.outputs);
                Assert.Equal(tx.hash(), dtx.hash());
            }
        }


        [Fact]
        public void tx_double_ser_deser()
        {
            // checks serializing doesn't mess up the tx and produces consistent results
            var btx = tx2i1o();
            using (var vec = new MemoryStream())
            {
                Ser.serialize(vec, btx);
                vec.Position = 0;

                var dtx = Ser.deserialize(vec, Transaction.Empty());

                using (var vec2 = new MemoryStream())
                {
                    Ser.serialize(vec2, btx);
                    vec2.Position = 0;

                    var dtx2 = Ser.deserialize(vec2, Transaction.Empty());

                    Assert.Equal(btx.hash(), dtx.hash());
                    Assert.Equal(dtx.hash(), dtx2.hash());
                }
            }
        }


        [Fact]
        public void hash_output()
        {
            var keychain = Keychain.Keychain.From_random_seed();
            var key_id1 = keychain.Derive_key_id(1);
            var key_id2 = keychain.Derive_key_id(2);
            var key_id3 = keychain.Derive_key_id(3);

            var (tx, _) = Build.transaction(new Func<Context, Append>[]
                {
                    c => c.input(75, key_id1),
                    c => c.output(42, key_id2),
                    c => c.output(32, key_id3),
                    c => c.with_fee(1)
                },
                keychain);

            var h = tx.outputs[0].hash();
            Assert.NotEqual(Hash.ZERO_HASH(), h);
            var h2 = tx.outputs[1].hash();
            Assert.NotEqual(h, h2);
        }

        [Fact]
        public void blind_tx()
        {
            var keychain = Keychain.Keychain.From_random_seed();

            var btx = tx2i1o();
            btx.verify_sig(keychain.Secp);

            // checks that the range proof on our blind output is sufficiently hiding
            var outp = btx.outputs[0];
            var proof = outp.proof;
            var info = keychain.Secp.range_proof_info(proof);
            Assert.Equal<ulong>(0, info.min);
            Assert.Equal(ulong.MaxValue, info.max);
        }

        [Fact]
        public void tx_hash_diff()
        {
            var btx1 = tx2i1o();
            var btx2 = tx1i1o();

            if (btx1.hash() == btx2.hash())
            {
                throw new Exception("diff txs have same hash");
            }
        }

        /// Simulate the standard exchange between 2 parties when creating a basic
        /// 2 inputs, 2 outputs transaction.
        [Fact]
        public void tx_build_exchange()
        {
            var keychain = Keychain.Keychain.From_random_seed();
            var key_id1 = keychain.Derive_key_id(1);
            var key_id2 = keychain.Derive_key_id(2);
            var key_id3 = keychain.Derive_key_id(3);
            var key_id4 = keychain.Derive_key_id(4);

            Transaction tx_alice;
            BlindingFactor blind_sum;

            {
                // Alice gets 2 of her pre-existing outputs to send 5 coins to Bob, they
                // become inputs in the new transaction
                (Func<Context, Append> in1, Func<Context, Append> in2) =
                    ( c => c.input(4, key_id1), c => c.input(3, key_id2));


                // Alice builds her transaction, with change, which also produces the sum
                // of blinding factors before they're obscured.
                var (tx, sum) = Build.transaction(new[]

                    {
                        in1,
                        in2,
                        c => c.output(1, key_id3),
                        c => c.with_fee(2)
                    },
                    keychain);

                tx_alice = tx;
                blind_sum = sum;
            }


            // From now on, Bob only has the obscured transaction and the sum of
            // blinding factors. He adds his output, finalizes the transaction so it's
            // ready for broadcast.
            var (tx_final, _) = Build.transaction(new Func<Context, Append>[]
                {
                    c => c.initial_tx(tx_alice),
                    c => c.with_excess(blind_sum),
                    c => c.output(4, key_id4)
                },
                keychain
            );

            tx_final.validate(keychain.Secp);
        }

        //[Fact]
        //public void reward_empty_block()
        //{
        //    var keychain = Keychain.Keychain.From_random_seed();
        //    var key_id = keychain.Derive_key_id(1);

        //    var b = Block.New(BlockHeader.Default(), new Transaction[0], keychain, key_id);
        //    b.Compact().validate(keychain.Secp);
        //}

        //[Fact]
        //        public void reward_with_tx_block()
        //        {
        //            var keychain = keychain.Keychain.from_random_seed();
        //            var key_id = keychain.derive_key_id(1);

        //            var  tx1 = tx2i1o();
        //            tx1.verify_sig(keychain.secp());

        //            var b = Block.new(& BlockHeader.default(), vec![& tx1], &keychain, key_id);
        //            b.compact().validate(keychain.secp());
        //        }

        //[Fact]
        //        public void simple_block()
        //        {
        //            var keychain = keychain.Keychain.from_random_seed();
        //            var key_id = keychain.derive_key_id(1);

        //            var  tx1 = tx2i1o();
        //            var  tx2 = tx1i1o();

        //            var b = Block.new(

        //                & BlockHeader.default(),
        //			vec![& tx1, & tx2],
        //			&keychain,
        //			key_id,
        //		);
        //            b.validate(keychain.secp());
        //        }

        //[Fact]
        //        public void test_block_with_timelocked_tx()
        //        {
        //            var keychain = keychain.Keychain.from_random_seed();

        //            var key_id1 = keychain.derive_key_id(1);
        //            var key_id2 = keychain.derive_key_id(2);
        //            var key_id3 = keychain.derive_key_id(3);

        //            // first check we can add a timelocked tx where lock height matches current block height
        //            // and that the resulting block is valid
        //            var tx1 = build.transaction(
        //                vec![
        //                    input(5, key_id1.clone()),
        //                    output(3, key_id2.clone()),
        //                    with_fee(2),
        //                    with_lock_height(1),

        //                ],
        //                &keychain,

        //            ).map(| (tx, _) | tx)
        //                ;

        //            var b = Block.new(

        //                & BlockHeader.default(),
        //			vec![&tx1],
        //			&keychain,
        //			key_id3.clone(),
        //		);
        //            b.validate(keychain.secp());

        //            // now try adding a timelocked tx where lock height is greater than current block height
        //            var tx1 = build.transaction(
        //                vec![
        //                    input(5, key_id1.clone()),
        //                    output(3, key_id2.clone()),
        //                    with_fee(2),
        //                    with_lock_height(2),

        //                ],
        //                &keychain,

        //            ).map(| (tx, _) | tx)
        //                ;

        //            var b = Block.new(

        //                & BlockHeader.default(),
        //			vec![&tx1],
        //			&keychain,
        //			key_id3.clone(),
        //		);
        //            match b.validate(keychain.secp()) {
        //                Err(KernelLockHeight { lock_height: height }) => {
        //                    Assert.Equal(height, 2);
        //                }
        //                _ => panic!("expecting KernelLockHeight error here"),
        //		}
        //        }

        [Fact]
        public void test_verify_1i1o_sig()
        {
            var keychain = Keychain.Keychain.From_random_seed();
            var tx = tx1i1o();
            tx.verify_sig(keychain.Secp);
        }

        [Fact]
        public void test_verify_2i1o_sig()
        {
            var keychain = Keychain.Keychain.From_random_seed();
            var tx = tx2i1o();
            tx.verify_sig(keychain.Secp);
        }


        // utility producing a transaction with 2 inputs and a single outputs
        public static Transaction tx2i1o()
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
        public static Transaction tx1i1o()
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