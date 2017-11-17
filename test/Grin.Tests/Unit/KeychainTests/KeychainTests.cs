using Secp256k1Proxy;
using Xunit;

namespace Grin.Tests.Unit.KeychainTests
{
    public class KeychainTests
    {
        [Fact]
        public void test_key_derivation()
        {
            var secp = Secp256k1.WithCaps(ContextFlag.Commit);

            var keychain = Keychain.Keychain.From_random_seed();

            // use the keychain to derive a "key_id_set" based on the underlying seed
            var key_id = keychain.Derive_key_id(1);

            var msg_bytes = new byte[32];
            var msg = Message.from_slice(msg_bytes);

            // now New a zero commitment using the key on the keychain associated with
            // the key_id_set
            var commit = keychain.Commit(0, key_id);

            // now check we can use our key to verify a signature from this zero commitment
            var sig = keychain.Sign(msg, key_id);
            secp.verify_from_commit(msg, sig, commit);
        }


        [Fact]
        public void test_rewind_range_proof()
        {
            var keychain = Keychain.Keychain.From_random_seed();
            var key_id = keychain.Derive_key_id(1);
            var commit = keychain.Commit(5, key_id);
            var msg = ProofMessage.empty();

            var proof = keychain.Range_proof(5, key_id, commit, msg);
            var proof_info = keychain.Rewind_range_proof(key_id, commit, proof);

            Assert.True(proof_info.success);
            Assert.Equal<ulong>(5, proof_info.value);


            var pm1 = proof_info.message;
            var pm2 = ProofMessage.from_bytes(new byte[Constants.PROOF_MSG_SIZE]);

            // now check the recovered message is "empty" (but not truncated) i.e. all zeroes

            Assert.Equal(pm1.Value, pm2.Value);

            var key_id2 = keychain.Derive_key_id(2);

            // cannot rewind with a different nonce
            proof_info = keychain.Rewind_range_proof(key_id2, commit, proof);
            Assert.False(proof_info.success);
            Assert.Equal<ulong>(0, proof_info.value);

            // cannot rewind with a commitment to the same value using a different key
            var commit2 = keychain.Commit(5, key_id2);
            proof_info = keychain.Rewind_range_proof(key_id, commit2, proof);
            Assert.False(proof_info.success);
            Assert.Equal<ulong>(0, proof_info.value);

            // cannot rewind with a commitment to a different value
            var commit3 = keychain.Commit(4, key_id);
            proof_info = keychain.Rewind_range_proof(key_id, commit3, proof);
            Assert.False(proof_info.success);
            Assert.Equal<ulong>(0, proof_info.value);
        }
    }
}