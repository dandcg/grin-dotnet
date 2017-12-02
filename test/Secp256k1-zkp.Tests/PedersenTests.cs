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
        public void Test_verify_commit_sum_zero_keys()
        {

            var secp = Secp256K1.WithCaps(ContextFlag.Commit);

            Commitment Commit(ulong value)
            {
                var blinding = SecretKey.ZeroKey;
                return secp.Commit(value, blinding);
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
        public void Test_verify_commit_sum_one_keys()
        {
            var secp = Secp256K1.WithCaps(ContextFlag.Commit);

            Commitment Commit(ulong value, SecretKey blinding)
            {
               return secp.Commit(value, blinding);
            }

            Assert.True(secp.verify_commit_sum(
                new[] { Commit(5, SecretKey.OneKey) },
                new[] { Commit(5, SecretKey.OneKey) }
                ));
            
            //// we expect this not to verify
            //// even though the values add up to 0
            //// the keys themselves do not add to 0

            Assert.False(secp.verify_commit_sum(
                new[] { Commit(3, SecretKey.OneKey), Commit(2, SecretKey.OneKey) }, 
                new[] { Commit(5, SecretKey.OneKey) }));


            //// to get these to verify we need to
            //// use the same "sum" of blinding factors on both sides

            var twoKey = secp.blind_sum(new []{SecretKey.OneKey, SecretKey.OneKey},new SecretKey[]{} );

            Assert.True(secp.verify_commit_sum(
                new[] { Commit(3, SecretKey.OneKey), Commit(2, SecretKey.OneKey) },
                new[] { Commit(5, twoKey) }));

        }

        [Fact]
        public void Test_verify_commit_sum_random_keys()
        {
            var secp = Secp256K1.WithCaps(ContextFlag.Commit);

            Commitment Commit(ulong value, SecretKey blinding)
            {
                return secp.Commit(value, blinding);
            }

            var blindPos = SecretKey.New(secp, RandomNumberGenerator.Create());
            var blindNeg = SecretKey.New(secp, RandomNumberGenerator.Create());

            // now construct blinding factor to net out appropriately
           var blindSum = secp.blind_sum(new[] { blindPos}, new[] { blindNeg});

           secp.verify_commit_sum(
               new []{Commit(101, blindPos)},
               new[] {Commit(75, blindNeg), Commit(26, blindSum)}
               );
        }

       [Fact]
        public void Test_to_two_pubkeys()
        {
            var secp = Secp256K1.WithCaps(ContextFlag.Commit);
            var blinding = SecretKey.New(secp, RandomNumberGenerator.Create());
            var commit = secp.Commit(5, blinding);

            Assert.Equal(2,commit.to_two_pubkeys(secp).Length);
 
        }

        [Fact]
        // to_pubkey() is not currently working as secp does currently
        // provide an api to extract a public key from a commitment
        public void test_to_pubkey()
        {
            var secp = Secp256K1.WithCaps(ContextFlag.Commit);
            var blinding = SecretKey.New(secp, RandomNumberGenerator.Create());
            var commit = secp.Commit(5, blinding);

            Assert.Throws<Exception>(() =>
            {
                commit.to_pubkey(secp);
            });

        }


        [Fact]
        public void Test_sign_with_pubkey_from_commitment()
        {
            var secp = Secp256K1.WithCaps(ContextFlag.Commit);
            var blinding = SecretKey.New(secp, RandomNumberGenerator.Create());
            var commit = secp.Commit(0, blinding);

            var msgBytes = ByteUtil.Get_random_bytes(RandomNumberGenerator.Create(),32);

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
                try
                {
                    secp.Verify(msg, sig, pubkeys[1]);
                }
                catch (Exception ex)
                {
                    throw new Exception("this is not good", ex);
                }
             
            }
        }

        [Fact]
        public void Test_commit_sum()
        {
            var secp = Secp256K1.WithCaps(ContextFlag.Commit);

            Commitment Commit(ulong value, SecretKey blinding)
            {
                return secp.Commit(value, blinding);
            }

            var blindA = SecretKey.New( secp, RandomNumberGenerator.Create());
            var blindB = SecretKey.New( secp, RandomNumberGenerator.Create());

            var commitA = Commit(3, blindA);
            var commitB = Commit(2, blindB);

            var blindC = secp.blind_sum(new []{blindA, blindB},new SecretKey[]{});

            var commitC = Commit(3 + 2, blindC);

            var commitD = secp.commit_sum(new[] { commitA, commitB}, new Commitment[] { });

            Assert.Equal(commitC.Value, commitD.Value);

            var blindE = secp.blind_sum(new[]{blindA}, new[] {blindB});

            var commitE = Commit(3 - 2, blindE);

            var commitF = secp.commit_sum(new[] { commitA}, new[] { commitB});

            Assert.Equal(commitE.Value, commitF.Value);

        }

        [Fact]
        public void Test_range_proof()
        {
            var secp = Secp256K1.WithCaps(ContextFlag.Commit);
            var blinding = SecretKey.New(secp, RandomNumberGenerator.Create());
            var commit = secp.Commit(7, blinding);
            var msg = ProofMessage.Empty();
            var rangeProof = secp.range_proof(0, 7, blinding, commit, msg.Clone());
            var proofRange = secp.verify_range_proof(commit, rangeProof);

            Assert.Equal<ulong>(0,proofRange.Min);

            var  proofInfo = secp.range_proof_info(rangeProof);
            Assert.True(proofInfo.Success);
            Assert.Equal<ulong>(0, proofInfo.Min);

            //// check we get no information back for the value here
            Assert.Equal<ulong>(0, proofInfo.Value);

            proofInfo = secp.rewind_range_proof(commit, rangeProof, blinding);
            Assert.True(proofInfo.Success);
            Assert.Equal<ulong>(0, proofInfo.Min);
            Assert.Equal<ulong>(7, proofInfo.Value);

            //// check we cannot rewind a range proof without the original nonce
            var badNonce = SecretKey.New( secp, RandomNumberGenerator.Create());
            var badInfo = secp.rewind_range_proof(commit, rangeProof, badNonce);
            Assert.False(badInfo.Success);
            Assert.Equal<ulong>(0,badInfo.Value);

            //// check we can construct and verify a range proof on value 0
             commit = secp.Commit(0, blinding);
            rangeProof = secp.range_proof(0, 0, blinding, commit, msg);
            secp.verify_range_proof(commit, rangeProof);
             proofInfo = secp.rewind_range_proof(commit, rangeProof, blinding.Clone());
            Assert.True(proofInfo.Success);
            Assert.Equal<ulong>(0,proofInfo.Min);
            Assert.Equal<ulong>(0,proofInfo.Value);
        }


    }
}