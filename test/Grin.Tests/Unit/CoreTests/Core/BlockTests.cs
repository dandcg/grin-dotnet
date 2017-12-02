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
            var (tx, _) = Build.Transaction(new Func<Context, Append>[]
            {
                c => c.Input(v, keyId1),
                c => c.Output(3, keyId2),
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
            var(btx2, _) = Build.Transaction(new Func<Context, Append>[]
                {
                    c => c.Input(7, keyId1),
                    c => c.Output(5, keyId2.Clone()),
                    c => c.with_fee(2)
                },
                keychain
            );

            // spending tx2 - reuse key_id2

            var btx3 = Txspend1I1O(5, keychain, keyId2.Clone(), keyId3);
            var b = New_block(new[] {btx1, btx2, btx3}, keychain);

            // block should have been automatically compacted (including reward
            // output) and should still be valid
            b.Validate(keychain.Secp);
            Assert.Equal(3, b.Inputs.Length);
            Assert.Equal(3, b.Outputs.Length);
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

            var( btx2, _) = Build.Transaction(new Func<Context, Append>[]
                {
                    c=>c.Input(7,keyId1),
                    c=>c.Output(5,keyId2.Clone()),
                    c=>c.with_fee(2)


                }, 
               keychain

            );

            // spending tx2 - reuse key_id2
            var  btx3 = Txspend1I1O(5, keychain, keyId2.Clone(), keyId3);

            var b1 = New_block(new []{ btx1,  btx2} ,keychain);
            b1.Validate(keychain.Secp);

            var b2 = New_block(new []{ btx3}, keychain);
            b2.Validate(keychain.Secp);

            // block should have been automatically compacted and should still be valid
            var b3 = b1.Merge(b2);
           Assert.Equal(3,b3.Inputs.Length);
            Assert.Equal(4, b3.Outputs.Length);
        }

        [Fact]
        public void Empty_block_with_coinbase_is_valid()
        {
            var keychain = Keychain.From_random_seed();
            var b = New_block(new Transaction[] { }, keychain);

            Assert.Empty(b.Inputs);
            Assert.Single(b.Outputs);
            Assert.Single(b.Kernels);

            var coinbaseOutputs = b.Outputs.Where(w => w.Features.HasFlag(OutputFeatures.CoinbaseOutput))
                .Select(s => s.Clone()).ToArray();

            Assert.Single(coinbaseOutputs);

            var coinbaseKernels = b.Kernels.Where(w => w.Features.HasFlag(KernelFeatures.CoinbaseKernel))
                .Select(s => s.Clone()).ToArray();

            Assert.Single(coinbaseKernels);

            // the block should be valid here (single coinbase output with corresponding
            // txn kernel)
            b.Validate(keychain.Secp);

        }

        [Fact]
        // test that flipping the COINBASE_OUTPUT flag on the output features
        // invalidates the block and specifically it causes verify_coinbase to fail
        // additionally verifying the merkle_inputs_outputs also fails
        public void Remove_coinbase_output_flag()
        {
            var keychain = Keychain.From_random_seed();
            var b = New_block(new Transaction[] { }, keychain);

            Assert.True(b.Outputs[0].Features.HasFlag(OutputFeatures.CoinbaseOutput));
           b.Outputs[0].Features =0;// remove(COINBASE_OUTPUT);

            var ex=Assert.Throws<BlockErrorException>(
                ()=>b.verify_coinbase()
            );

            Assert.Equal(BlockError.CoinbaseSumMismatch, ex.Error);


           b.verify_kernels(keychain.Secp, false);


           ex = Assert.Throws<BlockErrorException>(
                () => b.Validate(keychain.Secp)
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

            Assert.True(b.Kernels[0].Features.HasFlag(KernelFeatures.CoinbaseKernel));
            b.Kernels[0].Features = 0;

      
            var ex = Assert.Throws<BlockErrorException>(
                () => b.verify_coinbase()
            );

            Assert.Equal(BlockError.Secp, ex.Error);  //IncorrectCommitSum

            b.verify_kernels(keychain.Secp, false);
          

            ex = Assert.Throws<BlockErrorException>(
                () => b.Validate(keychain.Secp)
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
                var bh1 = b.Header;
                b.Header.Write(bw);

                vec.Position = 0;
       
                var br=new BinReader(vec);
                var bh2 = BlockHeader.Readnew(br);

                Assert.Equal(bh1.Version, bh2.Version);
                Assert.Equal(bh1.Height, bh2.Height);
                Assert.Equal(bh1.Previous.Value, bh2.Previous.Value);
                Assert.Equal(bh1.Timestamp.PrecisionSeconds(), bh2.Timestamp);
                Assert.Equal(bh1.UtxoRoot.Value, bh2.UtxoRoot.Value);
                Assert.Equal(bh1.RangeProofRoot.Value, bh2.RangeProofRoot.Value);
                Assert.Equal(bh1.KernelRoot.Value, bh2.KernelRoot.Value);
                Assert.Equal(bh1.Nonce, bh2.Nonce);


                Assert.Equal(bh1.Difficulty.Num, bh2.Difficulty.Num);
                Assert.Equal(bh1.TotalDifficulty.Num, bh2.TotalDifficulty.Num);



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

                Assert.Equal(b.Inputs.Length, b2.Inputs.Length);
                Assert.Equal(b.Outputs.Length, b2.Outputs.Length);
                Assert.Equal(b.Kernels.Length, b2.Kernels.Length);
               Assert.Equal(b.Header.Hash().Hex, b2.Header.Hash().Hex);
            }
        }
    }
}