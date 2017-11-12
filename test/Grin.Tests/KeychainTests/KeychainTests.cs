using Secp256k1Proxy;
using Xunit;

namespace Grin.Tests.KeychainTests
{
    public class KeychainTests
    {
        [Fact]
        public void test_key_derivation()
        {
            var secp = Secp256k1.WithCaps(ContextFlag.Commit);

            var keychain = Keychain.Keychain.from_random_seed();

            // use the keychain to derive a "key_id" based on the underlying seed
            var key_id = keychain.derive_key_id(1);

            var msg_bytes = new byte[32];
            var msg = Message.from_slice(msg_bytes);

            // now New a zero commitment using the key on the keychain associated with
            // the key_id
            var commit = keychain.commit(0, key_id);

            // now check we can use our key to verify a signature from this zero commitment
            var sig = keychain.sign(msg, key_id);
            secp.verify_from_commit(msg, sig, commit);
        }


        [Fact]
        public void test_rewind_range_proof()
        {
            //      let keychain = Keychain::from_random_seed().unwrap();
            //      let key_id = keychain.derive_key_id(1).unwrap();
            //      let commit = keychain.commit(5, &key_id).unwrap();
            //      let msg = ProofMessage::empty();

            //      let proof = keychain.range_proof(5, &key_id, commit, msg).unwrap();
            //      let proof_info = keychain.rewind_range_proof(&key_id, commit, proof).unwrap();

            //      assert_eq!(proof_info.success, true);
            //      assert_eq!(proof_info.value, 5);

            //      // now check the recovered message is "empty" (but not truncated) i.e. all
            //      // zeroes
            //      assert_eq!(
            //          proof_info.message,
            //          secp::pedersen::ProofMessage::from_bytes(&[0; secp::constants::PROOF_MSG_SIZE])
            //);

            //      let key_id2 = keychain.derive_key_id(2).unwrap();

            //      // cannot rewind with a different nonce
            //      let proof_info = keychain
            //          .rewind_range_proof(&key_id2, commit, proof)
            //          .unwrap();
            //      assert_eq!(proof_info.success, false);
            //      assert_eq!(proof_info.value, 0);

            //      // cannot rewind with a commitment to the same value using a different key
            //      let commit2 = keychain.commit(5, &key_id2).unwrap();
            //      let proof_info = keychain
            //          .rewind_range_proof(&key_id, commit2, proof)
            //          .unwrap();
            //      assert_eq!(proof_info.success, false);
            //      assert_eq!(proof_info.value, 0);

            //      // cannot rewind with a commitment to a different value
            //      let commit3 = keychain.commit(4, &key_id).unwrap();
            //      let proof_info = keychain
            //          .rewind_range_proof(&key_id, commit3, proof)
            //          .unwrap();
            //      assert_eq!(proof_info.success, false);
            //      assert_eq!(proof_info.value, 0);
        }
    }
}