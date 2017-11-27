using System;
using System.Security.Cryptography;
using Common;
using Secp256k1Proxy.Key;
using Secp256k1Proxy.Lib;
using Secp256k1Proxy.Pedersen;
using Xunit;

namespace Secp256k1Proxy.Tests
{
    public class PedersenTests
    {
        [Fact]
        public void test_verify_commit_sum_zero_keys()
        {

            var secp = Secp256k1.WithCaps(ContextFlag.Commit);

            Commitment Commit(ulong value)
            {
                var blinding = SecretKey.ZERO_KEY;
                return secp.commit(value, blinding);
            }

            Assert.True(secp.verify_commit_sum(
                new Commitment[] {}, 
                new Commitment[] { }
                ));

            Assert.True(secp.verify_commit_sum(
                new[] { Commit(5) }, 
                new[] { Commit(5) }));

            Assert.True(secp.verify_commit_sum(
                new[] { Commit(3), Commit(2) }, 
                new[] { Commit(5) }
                ));

            Assert.True(secp.verify_commit_sum(
                new[] { Commit(2), Commit(4) }, 
                new[] { Commit(1), Commit(5) }
                ));

        }



        [Fact]
        public void test_verify_commit_sum_one_keys()
        {
            var secp = Secp256k1.WithCaps(ContextFlag.Commit);

            Commitment Commit(ulong value, SecretKey blinding)
            {
               return secp.commit(value, blinding);
            }

            Assert.True(secp.verify_commit_sum(
                new[] { Commit(5, SecretKey.ONE_KEY) },
                new[] { Commit(5, SecretKey.ONE_KEY) }
                ));
            
            //// we expect this not to verify
            //// even though the values add up to 0
            //// the keys themselves do not add to 0

            Assert.False(secp.verify_commit_sum(
                new[] { Commit(3, SecretKey.ONE_KEY), Commit(2, SecretKey.ONE_KEY) }, 
                new[] { Commit(5, SecretKey.ONE_KEY) }));


            //// to get these to verify we need to
            //// use the same "sum" of blinding factors on both sides

            var two_key = secp.blind_sum(new []{SecretKey.ONE_KEY, SecretKey.ONE_KEY},new SecretKey[]{} );

            Assert.True(secp.verify_commit_sum(
                new[] { Commit(3, SecretKey.ONE_KEY), Commit(2, SecretKey.ONE_KEY) },
                new[] { Commit(5, two_key) }));

        }

        [Fact]
        public void test_verify_commit_sum_random_keys()
        {
            var secp = Secp256k1.WithCaps(ContextFlag.Commit);

            Commitment Commit(ulong value, SecretKey blinding)
            {
                return secp.commit(value, blinding);
            }

            var blind_pos = SecretKey.New(secp, RandomNumberGenerator.Create());
            var blind_neg = SecretKey.New(secp, RandomNumberGenerator.Create());

            // now construct blinding factor to net out appropriately
           var blind_sum = secp.blind_sum(new[] { blind_pos}, new[] { blind_neg});

           secp.verify_commit_sum(
               new []{Commit(101, blind_pos)},
               new[] {Commit(75, blind_neg), Commit(26, blind_sum)}
               );
        }

       [Fact]
        public void test_to_two_pubkeys()
        {
            var secp = Secp256k1.WithCaps(ContextFlag.Commit);
            var blinding = SecretKey.New(secp, RandomNumberGenerator.Create());
            var commit = secp.commit(5, blinding);

            Assert.Equal(2,commit.to_two_pubkeys(secp).Length);
 
        }

        [Fact]
        // to_pubkey() is not currently working as secp does currently
        // provide an api to extract a public key from a commitment
        public void test_to_pubkey()
        {
            var secp = Secp256k1.WithCaps(ContextFlag.Commit);
            var blinding = SecretKey.New(secp, RandomNumberGenerator.Create());
            var commit = secp.commit(5, blinding);

            Assert.Throws<Exception>(() =>
            {
                var pubkey = commit.to_pubkey(secp);
            });

        }


        [Fact]
        public void test_sign_with_pubkey_from_commitment()
        {
            var secp = Secp256k1.WithCaps(ContextFlag.Commit);
            var blinding = SecretKey.New(secp, RandomNumberGenerator.Create());
            var commit = secp.commit(0, blinding);

            var msgBytes = ByteUtil.get_random_bytes(RandomNumberGenerator.Create());

            var msg = Message.from_slice(msgBytes);

            var sig = secp.Sign(msg, blinding);

            var pubkeys = commit.to_two_pubkeys(secp);

            // check that we can successfully verify the signature with one of the public keys

            try
            {
                secp.Verify(msg, sig, pubkeys[0]);
            }
            catch (Exception)
            {
                secp.Verify(msg, sig, pubkeys[0]);
                throw new Exception("this is not good");
            }
        }

        [Fact]
        public void test_commit_sum()
        {
            var secp = Secp256k1.WithCaps(ContextFlag.Commit);

            Commitment Commit(ulong value, SecretKey blinding)
            {
                return secp.commit(value, blinding);
            }

            var blind_a = SecretKey.New( secp, RandomNumberGenerator.Create());
            var blind_b = SecretKey.New( secp, RandomNumberGenerator.Create());

            var commit_a = Commit(3, blind_a);
            var commit_b = Commit(2, blind_b);

            var blind_c = secp.blind_sum(new []{blind_a, blind_b},new SecretKey[]{});

            var commit_c = Commit(3 + 2, blind_c);

            var commit_d = secp.commit_sum(new[] { commit_a, commit_b}, new Commitment[] { });

            Assert.Equal(commit_c.Value, commit_d.Value);

            var blind_e = secp.blind_sum(new[]{blind_a}, new[] {blind_b});

            var commit_e = Commit(3 - 2, blind_e);

            var commit_f = secp.commit_sum(new[] { commit_a}, new[] { commit_b});

            Assert.Equal(commit_e.Value, commit_f.Value);

        }

        [Fact]
        public void test_range_proof()
        {
            var secp = Secp256k1.WithCaps(ContextFlag.Commit);
            var blinding = SecretKey.New(secp, RandomNumberGenerator.Create());
            var commit = secp.commit(7, blinding);
            var msg = ProofMessage.empty();
            var range_proof = secp.range_proof(0, 7, blinding, commit, msg.clone());
            var proof_range = secp.verify_range_proof(commit, range_proof);

            Assert.Equal<ulong>(0,proof_range.min);

            var  proof_info = secp.range_proof_info(range_proof);
            Assert.True(proof_info.success);
            Assert.Equal<ulong>(0, proof_info.min);

            //// check we get no information back for the value here
            Assert.Equal<ulong>(0, proof_info.value);

            proof_info = secp.rewind_range_proof(commit, range_proof, blinding);
            Assert.True(proof_info.success);
            Assert.Equal<ulong>(0, proof_info.min);
            Assert.Equal<ulong>(7, proof_info.value);

            //// check we cannot rewind a range proof without the original nonce
            var bad_nonce = SecretKey.New( secp, RandomNumberGenerator.Create());
            var bad_info = secp.rewind_range_proof(commit, range_proof, bad_nonce);
            Assert.False(bad_info.success);
            Assert.Equal<ulong>(0,bad_info.value);

            //// check we can construct and verify a range proof on value 0
             commit = secp.commit(0, blinding);
            range_proof = secp.range_proof(0, 0, blinding, commit, msg);
            secp.verify_range_proof(commit, range_proof);
             proof_info = secp.rewind_range_proof(commit, range_proof, blinding.clone());
            Assert.True(proof_info.success);
            Assert.Equal<ulong>(0,proof_info.min);
            Assert.Equal<ulong>(0,proof_info.value);
        }


    }
}