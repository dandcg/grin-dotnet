using System;
using System.IO;
using System.Linq;
using Common;
using Grin.CoreImpl.Core.Block;
using Grin.CoreImpl.Core.Build;
using Grin.CoreImpl.Core.Hash;
using Grin.CoreImpl.Core.Transaction;
using Grin.CoreImpl.Ser;
using Grin.KeychainImpl;
using Grin.KeychainImpl.ExtKey;
using Xunit;

namespace Grin.Tests.Unit.CoreTests.Core
{
    public class BlockTests //: IClassFixture<LoggingFixture>
    {
        // utility to create a block without worrying about the key or previous
        // header
        private static Block New_block(Transaction[] txs, Keychain keychain)
        {
            var keyId = keychain.Derive_key_id(1);
            return Block.New(BlockHeader.Default(), txs, keychain, keyId);
        }

        // utility producing a transaction that spends an output with the provided
        // value and blinding key
        private static Transaction Txspend1I1O(ulong v, Keychain keychain, Identifier keyId1, Identifier keyId2)
        {
            var (tx, _) = Build.transaction(new Func<Context, Append>[]
            {
                c => c.input(v, keyId1),
                c => c.output(3, keyId2),
                c => c.with_fee(2)
            }, keychain);

            return tx;
        }


        // Too slow for now #[test]
        //fn too_large_block()
        //{
        //    let keychain = Keychain::from_random_seed();
        //    let max_out = MAX_BLOCK_WEIGHT / BLOCK_OUTPUT_WEIGHT;

        //    let  pks = vec![];
        //    for n in 0..(max_out + 1) {
        //        pks.push(keychain.derive_key_id(n as u32));
        //    }

        //    let  parts = vec![];
        //    for _ in 0..max_out {
        //        parts.push(output(5, pks.pop()));
        //    }

        //    let now = Instant::now();
        //    parts.append(& vec![input(500000, pks.pop()), with_fee(2)]);
        //    let  tx = build::transaction(parts, &keychain)
        //        .map(| (tx, _) | tx)
        //        ;
        //    println!("Build tx: {}", now.elapsed().as_secs());

        //    let b = new_block(vec![& tx], &keychain);
        //    assert!(b.validate(&keychain.secp()).is_err());
        //}

        [Fact]
        // builds a block with a tx spending another and check if merging occurred
        public void Compactable_block()
        {
            var keychain = Keychain.From_random_seed();
            var keyId1 = keychain.Derive_key_id(1);
            var keyId2 = keychain.Derive_key_id(2);
            var keyId3 = keychain.Derive_key_id(3);

            var btx1 = ModTests.Tx2I1O();
            var(btx2, _) = Build.transaction(new Func<Context, Append>[]
                {
                    c => c.input(7, keyId1),
                    c => c.output(5, keyId2.Clone()),
                    c => c.with_fee(2)
                },
                keychain
            );

            // spending tx2 - reuse key_id2

            var btx3 = Txspend1I1O(5, keychain, keyId2.Clone(), keyId3);
            var b = New_block(new[] {btx1, btx2, btx3}, keychain);

            // block should have been automatically compacted (including reward
            // output) and should still be valid
            b.validate(keychain.Secp);
            Assert.Equal(3, b.inputs.Length);
            Assert.Equal(3, b.outputs.Length);
        }

[Fact]
        // builds 2 different blocks with a tx spending another and check if merging
        // occurs
    public void Mergeable_blocks()
        {
            var keychain = Keychain.From_random_seed();
            var keyId1 = keychain.Derive_key_id(1);
            var keyId2 = keychain.Derive_key_id(2);
            var keyId3 = keychain.Derive_key_id(3);

            var  btx1 = ModTests.Tx2I1O();

            var( btx2, _) = Build.transaction(new Func<Context, Append>[]
                {
                    c=>c.input(7,keyId1),
                    c=>c.output(5,keyId2.Clone()),
                    c=>c.with_fee(2)


                }, 
               keychain

            );

            // spending tx2 - reuse key_id2
            var  btx3 = Txspend1I1O(5, keychain, keyId2.Clone(), keyId3);

            var b1 = New_block(new []{ btx1,  btx2} ,keychain);
            b1.validate(keychain.Secp);

            var b2 = New_block(new []{ btx3}, keychain);
            b2.validate(keychain.Secp);

            // block should have been automatically compacted and should still be valid
            var b3 = b1.merge(b2);
           Assert.Equal(3,b3.inputs.Length);
            Assert.Equal(4, b3.outputs.Length);
        }

        [Fact]
        public void Empty_block_with_coinbase_is_valid()
        {
            var keychain = Keychain.From_random_seed();
            var b = New_block(new Transaction[] { }, keychain);

            Assert.Empty(b.inputs);
            Assert.Single(b.outputs);
            Assert.Single(b.kernels);

            var coinbaseOutputs = b.outputs.Where(w => w.Features.HasFlag(OutputFeatures.COINBASE_OUTPUT))
                .Select(s => s.Clone()).ToArray();

            Assert.Single(coinbaseOutputs);

            var coinbaseKernels = b.kernels.Where(w => w.features.HasFlag(KernelFeatures.COINBASE_KERNEL))
                .Select(s => s.Clone()).ToArray();

            Assert.Single(coinbaseKernels);

            // the block should be valid here (single coinbase output with corresponding
            // txn kernel)
            b.validate(keychain.Secp);

        }

        [Fact]
        // test that flipping the COINBASE_OUTPUT flag on the output features
        // invalidates the block and specifically it causes verify_coinbase to fail
        // additionally verifying the merkle_inputs_outputs also fails
        public void Remove_coinbase_output_flag()
        {
            var keychain = Keychain.From_random_seed();
            var b = New_block(new Transaction[] { }, keychain);

            Assert.True(b.outputs[0].Features.HasFlag(OutputFeatures.COINBASE_OUTPUT));
           b.outputs[0].Features =0;// remove(COINBASE_OUTPUT);

            var ex=Assert.Throws<BlockErrorException>(
                ()=>b.verify_coinbase()
            );

            Assert.Equal(BlockError.CoinbaseSumMismatch, ex.Error);


           b.verify_kernels(keychain.Secp, false);


           ex = Assert.Throws<BlockErrorException>(
                () => b.validate(keychain.Secp)
            );

            Assert.Equal(BlockError.CoinbaseSumMismatch, ex.Error);

      
        }

[Fact]
        // test that flipping the COINBASE_KERNEL flag on the kernel features
        // invalidates the block and specifically it causes verify_coinbase to fail
        public void Remove_coinbase_kernel_flag()
        {
            var keychain = Keychain.From_random_seed();
            var b = New_block(new Transaction[] { }, keychain);

            Assert.True(b.kernels[0].features.HasFlag(KernelFeatures.COINBASE_KERNEL));
            b.kernels[0].features = 0;

      
            var ex = Assert.Throws<BlockErrorException>(
                () => b.verify_coinbase()
            );

            Assert.Equal(BlockError.Secp, ex.Error);  //IncorrectCommitSum

            b.verify_kernels(keychain.Secp, false);
          

            ex = Assert.Throws<BlockErrorException>(
                () => b.validate(keychain.Secp)
            );

            Assert.Equal(BlockError.Secp, ex.Error);   //IncorrectCommitSum

        }


        [Fact]
        public void Serialize_deserialize_blockheader()
        {
            var keychain = Keychain.From_random_seed();
            var b = New_block(new Transaction[] { }, keychain);
            // Console.WriteLine(JsonConvert.SerializeObject(b.header, Formatting.Indented));
            using (var vec = new MemoryStream())
            {
                var bw=new BinWriter(vec);
                var bh1 = b.header;
                b.header.write(bw);

                vec.Position = 0;
       
                var br=new BinReader(vec);
                var bh2 = BlockHeader.readnew(br);

                Assert.Equal(bh1.version, bh2.version);
                Assert.Equal(bh1.height, bh2.height);
                Assert.Equal(bh1.previous.Value, bh2.previous.Value);
                Assert.Equal(bh1.timestamp.PrecisionSeconds(), bh2.timestamp);
                Assert.Equal(bh1.utxo_root.Value, bh2.utxo_root.Value);
                Assert.Equal(bh1.range_proof_root.Value, bh2.range_proof_root.Value);
                Assert.Equal(bh1.kernel_root.Value, bh2.kernel_root.Value);
                Assert.Equal(bh1.nonce, bh2.nonce);


                Assert.Equal(bh1.difficulty.num, bh2.difficulty.num);
                Assert.Equal(bh1.total_difficulty.num, bh2.total_difficulty.num);



                }
        }



        [Fact]
        public void Serialize_deserialize_block()
        {
            var keychain = Keychain.From_random_seed();
            var b = New_block(new Transaction[] { }, keychain);

            using (var vec = new MemoryStream()) 
            {
                Ser.Serialize(vec, b);
                vec.Position = 0;
                var b2 = Ser.Deserialize(vec, Block.Default());

                Assert.Equal(b.inputs.Length, b2.inputs.Length);
                Assert.Equal(b.outputs.Length, b2.outputs.Length);
                Assert.Equal(b.kernels.Length, b2.kernels.Length);
               Assert.Equal(b.header.hash().Hex, b2.header.hash().Hex);
            }
        }
    }
}