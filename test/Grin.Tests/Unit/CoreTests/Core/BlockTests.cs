using System;
using Grin.CoreImpl.Core.Block;
using Grin.CoreImpl.Core.Build;
using Grin.CoreImpl.Core.Transaction;
using Grin.KeychainImpl;
using Grin.KeychainImpl.ExtKey;
using Xunit;

namespace Grin.Tests.Unit.CoreTests.Core
{
    public class BlockTests : IClassFixture<LoggingFixture>
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
        //    let keychain = Keychain::from_random_seed().unwrap();
        //    let max_out = MAX_BLOCK_WEIGHT / BLOCK_OUTPUT_WEIGHT;

        //    let mut pks = vec![];
        //    for n in 0..(max_out + 1) {
        //        pks.push(keychain.derive_key_id(n as u32).unwrap());
        //    }

        //    let mut parts = vec![];
        //    for _ in 0..max_out {
        //        parts.push(output(5, pks.pop().unwrap()));
        //    }

        //    let now = Instant::now();
        //    parts.append(&mut vec![input(500000, pks.pop().unwrap()), with_fee(2)]);
        //    let mut tx = build::transaction(parts, &keychain)
        //        .map(| (tx, _) | tx)
        //        .unwrap();
        //    println!("Build tx: {}", now.elapsed().as_secs());

        //    let b = new_block(vec![&mut tx], &keychain);
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

        //#[test]
        //// builds 2 different blocks with a tx spending another and check if merging
        //// occurs
        //fn mergeable_blocks()
        //{
        //    let keychain = Keychain::from_random_seed().unwrap();
        //    let key_id1 = keychain.derive_key_id(1).unwrap();
        //    let key_id2 = keychain.derive_key_id(2).unwrap();
        //    let key_id3 = keychain.derive_key_id(3).unwrap();

        //    let mut btx1 = tx2i1o();

        //    let(mut btx2, _) = build::transaction(
        //        vec![input(7, key_id1), output(5, key_id2.clone()), with_fee(2)],
        //        &keychain,

        //    ).unwrap();

        //    // spending tx2 - reuse key_id2
        //    let mut btx3 = txspend1i1o(5, &keychain, key_id2.clone(), key_id3);

        //    let b1 = new_block(vec![&mut btx1, &mut btx2], &keychain);
        //    b1.validate(&keychain.secp()).unwrap();

        //    let b2 = new_block(vec![&mut btx3], &keychain);
        //    b2.validate(&keychain.secp()).unwrap();

        //    // block should have been automatically compacted and should still be valid
        //    let b3 = b1.merge(b2);
        //    assert_eq!(b3.inputs.len(), 3);
        //    assert_eq!(b3.outputs.len(), 4);
        //}

        //#[test]
        //fn empty_block_with_coinbase_is_valid()
        //{
        //    let keychain = Keychain::from_random_seed().unwrap();
        //    let b = new_block(vec![], &keychain);

        //    assert_eq!(b.inputs.len(), 0);
        //    assert_eq!(b.outputs.len(), 1);
        //    assert_eq!(b.kernels.len(), 1);

        //    let coinbase_outputs = b.outputs
        //        .iter()
        //        .filter(|out| out.features.contains(COINBASE_OUTPUT))
        //        .map(| o | o.clone())
        //        .collect::< Vec < _ >> ();
        //    assert_eq!(coinbase_outputs.len(), 1);

        //    let coinbase_kernels = b.kernels
        //        .iter()
        //        .filter(|out| out.features.contains(COINBASE_KERNEL))
        //        .map(| o | o.clone())
        //        .collect::< Vec < _ >> ();
        //    assert_eq!(coinbase_kernels.len(), 1);

        //    // the block should be valid here (single coinbase output with corresponding
        //    // txn kernel)
        //    assert_eq!(b.validate(&keychain.secp()), Ok(()));
        //}

        //#[test]
        //// test that flipping the COINBASE_OUTPUT flag on the output features
        //// invalidates the block and specifically it causes verify_coinbase to fail
        //// additionally verifying the merkle_inputs_outputs also fails
        //fn remove_coinbase_output_flag()
        //{
        //    let keychain = Keychain::from_random_seed().unwrap();
        //    let mut b = new_block(vec![], &keychain);

        //    assert!(b.outputs[0].features.contains(COINBASE_OUTPUT));
        //    b.outputs[0].features.remove(COINBASE_OUTPUT);

        //    assert_eq!(
        //        b.verify_coinbase(&keychain.secp()),
        //        Err(Error::CoinbaseSumMismatch)
        //    );
        //    assert_eq!(b.verify_kernels(&keychain.secp(), false), Ok(()));

        //    assert_eq!(
        //        b.validate(&keychain.secp()),
        //        Err(Error::CoinbaseSumMismatch)
        //    );
        //}

        //#[test]
        //// test that flipping the COINBASE_KERNEL flag on the kernel features
        //// invalidates the block and specifically it causes verify_coinbase to fail
        //fn remove_coinbase_kernel_flag()
        //{
        //    let keychain = Keychain::from_random_seed().unwrap();
        //    let mut b = new_block(vec![], &keychain);

        //    assert!(b.kernels[0].features.contains(COINBASE_KERNEL));
        //    b.kernels[0].features.remove(COINBASE_KERNEL);

        //    assert_eq!(
        //        b.verify_coinbase(&keychain.secp()),
        //        Err(Error::Secp(secp::Error::IncorrectCommitSum))
        //    );
        //    assert_eq!(b.verify_kernels(&keychain.secp(), true), Ok(()));

        //    assert_eq!(
        //        b.validate(&keychain.secp()),
        //        Err(Error::Secp(secp::Error::IncorrectCommitSum))
        //    );
        //}

        //#[test]
        //fn serialize_deserialize_block()
        //{
        //    let keychain = Keychain::from_random_seed().unwrap();
        //    let b = new_block(vec![], &keychain);

        //    let mut vec = Vec::new();
        //    ser::serialize(&mut vec, &b).expect("serialization failed");
        //    let b2: Block = ser::deserialize(&mut & vec[..]).unwrap();

        //    assert_eq!(b.inputs, b2.inputs);
        //    assert_eq!(b.outputs, b2.outputs);
        //    assert_eq!(b.kernels, b2.kernels);
        //    assert_eq!(b.header, b2.header);
        //}
    }
}