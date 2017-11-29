using System;
using System.IO;
using System.Linq;
using Grin.CoreImpl.Core.Block;
using Grin.CoreImpl.Core.Build;
using Grin.CoreImpl.Core.Transaction;
using Grin.CoreImpl.Ser;
using Grin.KeychainImpl;
using Grin.KeychainImpl.ExtKey;
using Konscious.Security.Cryptography;
using Microsoft.Azure.KeyVault.Models;
using Newtonsoft.Json;
using Xunit;

namespace Grin.Tests.Unit.CoreTests.Core
{
    public class BlockTests //: IClassFixture<LoggingFixture>
    {
        // utility to create a block without worrying about the key or previous
        // header
        private Block new_block(Transaction[] txs, Keychain keychain)
        {
            var key_id = keychain.Derive_key_id(1);
            return Block.New(BlockHeader.Default(), txs, keychain, key_id);
        }

        // utility producing a transaction that spends an output with the provided
        // value and blinding key
        private Transaction txspend1i1o(ulong v, Keychain keychain, Identifier key_id1, Identifier key_id2)
        {
            var (tx, _) = Build.transaction(new Func<Context, Append>[]
            {
                c => c.input(v, key_id1),
                c => c.output(3, key_id2),
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
        public void compactable_block()
        {
            var keychain = Keychain.From_random_seed();
            var key_id1 = keychain.Derive_key_id(1);
            var key_id2 = keychain.Derive_key_id(2);
            var key_id3 = keychain.Derive_key_id(3);

            var btx1 = ModTests.tx2i1o();
            var(btx2, _) = Build.transaction(new Func<Context, Append>[]
                {
                    c => c.input(7, key_id1),
                    c => c.output(5, key_id2.Clone()),
                    c => c.with_fee(2)
                },
                keychain
            );

            // spending tx2 - reuse key_id2

            var btx3 = txspend1i1o(5, keychain, key_id2.Clone(), key_id3);
            var b = new_block(new[] {btx1, btx2, btx3}, keychain);

            // block should have been automatically compacted (including reward
            // output) and should still be valid
            b.validate(keychain.Secp);
            Assert.Equal(3, b.inputs.Length);
            Assert.Equal(3, b.outputs.Length);
        }

[Fact]
        // builds 2 different blocks with a tx spending another and check if merging
        // occurs
    public void mergeable_blocks()
        {
            var keychain = Keychain.From_random_seed();
            var key_id1 = keychain.Derive_key_id(1);
            var key_id2 = keychain.Derive_key_id(2);
            var key_id3 = keychain.Derive_key_id(3);

            var  btx1 = ModTests.tx2i1o();

            var( btx2, _) = Build.transaction(new Func<Context, Append>[]
                {
                    c=>c.input(7,key_id1),
                    c=>c.output(5,key_id2.Clone()),
                    c=>c.with_fee(2)


                }, 
               keychain

            );

            // spending tx2 - reuse key_id2
            var  btx3 = txspend1i1o(5, keychain, key_id2.Clone(), key_id3);

            var b1 = new_block(new []{ btx1,  btx2} ,keychain);
            b1.validate(keychain.Secp);

            var b2 = new_block(new []{ btx3}, keychain);
            b2.validate(keychain.Secp);

            // block should have been automatically compacted and should still be valid
            var b3 = b1.merge(b2);
           Assert.Equal(3,b3.inputs.Length);
            Assert.Equal(4, b3.outputs.Length);
        }

        [Fact]
        public void empty_block_with_coinbase_is_valid()
        {
            var keychain = Keychain.From_random_seed();
            var b = new_block(new Transaction[] { }, keychain);

            Assert.Empty(b.inputs);
            Assert.Single(b.outputs);
            Assert.Single(b.kernels);

            var coinbase_outputs = b.outputs.Where(w => w.features.HasFlag(OutputFeatures.COINBASE_OUTPUT))
                .Select(s => s.Clone()).ToArray();

            Assert.Single(coinbase_outputs);

            var coinbase_kernels = b.kernels.Where(w => w.features.HasFlag(KernelFeatures.COINBASE_KERNEL))
                .Select(s => s.Clone()).ToArray();

            Assert.Single(coinbase_kernels);

            // the block should be valid here (single coinbase output with corresponding
            // txn kernel)
            b.validate(keychain.Secp);

        }

        [Fact]
        // test that flipping the COINBASE_OUTPUT flag on the output features
        // invalidates the block and specifically it causes verify_coinbase to fail
        // additionally verifying the merkle_inputs_outputs also fails
        public void remove_coinbase_output_flag()
        {
            var keychain = Keychain.From_random_seed();
            var b = new_block(new Transaction[] { }, keychain);

            Assert.True(b.outputs[0].features.HasFlag(OutputFeatures.COINBASE_OUTPUT));
           b.outputs[0].features =0;// remove(COINBASE_OUTPUT);

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
        public void remove_coinbase_kernel_flag()
        {
            var keychain = Keychain.From_random_seed();
            var b = new_block(new Transaction[] { }, keychain);

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
        public void serialize_deserialize_block()
        {
            var keychain = Keychain.From_random_seed();
            var b = new_block(new Transaction[] { }, keychain);
           // Console.WriteLine(JsonConvert.SerializeObject(b.header, Formatting.Indented));
            using (var vec = new MemoryStream()) 
            {
                Ser.serialize(vec, b);
                vec.Position =1;
                var b2 = Ser.deserialize(vec, Block.Default());

      
                //Console.WriteLine(JsonConvert.SerializeObject(b2.header, Formatting.Indented));
                Assert.Equal(b.inputs, b2.inputs);
                Assert.Equal(b.outputs, b2.outputs);
                Assert.Equal(b.kernels, b2.kernels);
                Assert.Equal(b.header, b2.header);
            }
        }
    }
}