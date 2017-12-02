using Grin.KeychainImpl;
using Secp256k1Proxy.Constants;
using Secp256k1Proxy.Lib;
using Secp256k1Proxy.Pedersen;
using Xunit;

namespace Grin.Tests.Unit.KeychainTests
{
    public class KeychainTests : IClassFixture<LoggingFixture>
    {
        [Fact]
        public void Test_key_derivation()
        {
            var secp = Secp256K1.WithCaps(ContextFlag.Commit);

            var keychain = Keychain.From_random_seed();

            // use the keychain to derive a "key_id_set" based on the underlying seed
            var keyId = keychain.Derive_key_id(1);

            var msgBytes = new byte[32];
            var msg = Message.from_slice(msgBytes);

            // now New a zero commitment using the key on the keychain associated with
            // the key_id_set
            var commit = keychain.Commit(0, keyId);

            // now check we can use our key to verify a signature from this zero commitment
            var sig = keychain.Sign(msg, keyId);
            secp.verify_from_commit(msg, sig, commit);
        }


        [Fact]
        public void Test_rewind_range_proof()
        {
            var keychain = Keychain.From_random_seed();
            var keyId = keychain.Derive_key_id(1);
            var commit = keychain.Commit(5, keyId);
            var msg = ProofMessage.Empty();

            var proof = keychain.Range_proof(5, keyId, commit, msg);
            var proofInfo = keychain.Rewind_range_proof(keyId, commit, proof);

            Assert.True(proofInfo.Success);
            Assert.Equal<ulong>(5, proofInfo.Value);


            var pm1 = proofInfo.Message;
            var pm2 = ProofMessage.from_bytes(new byte[Constants.ProofMsgSize]);

            // now check the recovered message is "empty" (but not truncated) i.e. all zeroes

            Assert.Equal(pm1.Value, pm2.Value);

            var keyId2 = keychain.Derive_key_id(2);

            // cannot rewind with a different nonce
            proofInfo = keychain.Rewind_range_proof(keyId2, commit, proof);
            Assert.False(proofInfo.Success);
            Assert.Equal<ulong>(0, proofInfo.Value);

            // cannot rewind with a commitment to the same value using a different key
            var commit2 = keychain.Commit(5, keyId2);
            proofInfo = keychain.Rewind_range_proof(keyId, commit2, proof);
            Assert.False(proofInfo.Success);
            Assert.Equal<ulong>(0, proofInfo.Value);

            // cannot rewind with a commitment to a different value
            var commit3 = keychain.Commit(4, keyId);
            proofInfo = keychain.Rewind_range_proof(keyId, commit3, proof);
            Assert.False(proofInfo.Success);
            Assert.Equal<ulong>(0, proofInfo.Value);
        }
    }
}