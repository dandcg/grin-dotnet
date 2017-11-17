using Grin.Core.Core;
using Secp256k1Proxy;
using Xunit;

namespace Grin.Tests.Unit.CoreTests.Core
{
    public class TransactionTests
    {
        [Fact]
        public void test_kernel_ser_deser()
        {
            var keychain = Keychain.Keychain.From_random_seed();
            var key_id = keychain.Derive_key_id(1);
            var commit = keychain.Commit(5, key_id);

            // just some bytes for testing ser/deser
            var sig = new byte[] {1, 0, 0, 0, 0, 0, 0, 1};

            var kernel = new TxKernel
            {
                features = KernelFeatures.DEFAULT_KERNEL,
                lock_height = 0,
                excess = commit,
                excess_sig = sig,
                fee = 10
            };

            //var mut vec = vec![];
            //ser::serialize(&mut vec, &kernel).expect("serialized failed");
            //var kernel2: TxKernel = ser::deserialize(&mut & vec[..]).unwrap();
            //assert_eq!(kernel2.features, DEFAULT_KERNEL);
            //assert_eq!(kernel2.lock_height, 0);
            //assert_eq!(kernel2.excess, commit);
            //assert_eq!(kernel2.excess_sig, sig.clone());
            //assert_eq!(kernel2.fee, 10);

            //// now check a kernel with lock_height serializes/deserializes correctly
            //var kernel = TxKernel {
            //    features: DEFAULT_KERNEL,
            //    lock_height: 100,
            //    excess: commit,
            //    excess_sig: sig.clone(),
            //    fee: 10,
            //};

            //var mut vec = vec![];
            //ser::serialize(&mut vec, &kernel).expect("serialized failed");
            //var kernel2: TxKernel = ser::deserialize(&mut & vec[..]).unwrap();
            //assert_eq!(kernel2.features, DEFAULT_KERNEL);
            //assert_eq!(kernel2.lock_height, 100);
            //assert_eq!(kernel2.excess, commit);
            //assert_eq!(kernel2.excess_sig, sig.clone());
            //assert_eq!(kernel2.fee, 10);
        }

        [Fact]
        public void test_output_ser_deser()
        {
            //var keychain = Keychain::from_random_seed().unwrap();
            //var key_id_set = keychain.derive_key_id(1).unwrap();
            //var commit = keychain.commit(5, &key_id_set).unwrap();
            //var switch_commit = keychain.switch_commit(&key_id_set).unwrap();
            //var switch_commit_hash = SwitchCommitHash::from_switch_commit(switch_commit);
            //var msg = secp::pedersen::ProofMessage::empty();
            //var proof = keychain.range_proof(5, &key_id_set, commit, msg).unwrap();

            //var out = Output {
            //    features: DEFAULT_OUTPUT,
            //    commit: commit,
            //    switch_commit_hash: switch_commit_hash,
            //    proof: proof,
            //};

            //var mut vec = vec![];
            //ser::serialize(&mut vec, &out).expect("serialized failed");
            //var dout: Output = ser::deserialize(&mut & vec[..]).unwrap();

            //assert_eq!(dout.features, DEFAULT_OUTPUT);
            //assert_eq!(dout.commit, out.commit);
            //assert_eq!(dout.proof, out.proof);
        }

        [Fact]
        public void test_output_value_recovery()
        {
            var keychain = Keychain.Keychain.From_random_seed();
            var key_id = keychain.Derive_key_id(1);

            var commit = keychain.Commit(1003, key_id);
            var switch_commit = keychain.Switch_commit(key_id);
            var switch_commit_hash = SwitchCommitHash.From_switch_commit(switch_commit);
            var msg = ProofMessage.empty();
            var proof = keychain.Range_proof(1003, key_id, commit, msg);

            var output = new Output
            {
                features = OutputFeatures.DEFAULT_OUTPUT,
                commit = commit,
                switch_commit_hash = switch_commit_hash,
                proof = proof
            };

            // check we can successfully recover the value with the original blinding factor
            var recovered_value = output.Recover_value(keychain, key_id);
            Assert.Equal<ulong?>(1003, recovered_value);

            // check we cannot recover the value without the original blinding factor
            var key_id2 = keychain.Derive_key_id(2);
            var not_recoverable = output.Recover_value(keychain, key_id2);
            Assert.Null(not_recoverable);
        }
    }
}