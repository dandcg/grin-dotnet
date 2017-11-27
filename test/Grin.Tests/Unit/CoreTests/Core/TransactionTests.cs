using System;
using System.IO;
using Common;
using Grin.Core;
using Grin.Core.Core;
using Secp256k1Proxy;
using Xunit;

namespace Grin.Tests.Unit.CoreTests.Core
{
    public class TransactionTests : IClassFixture<LoggingFixture>
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

            var stream = new MemoryStream();
            Ser.serialize(stream, kernel);

            Console.WriteLine("-------");
            Console.WriteLine(stream.ToArray().AsString());
            Console.WriteLine("-------");

            stream.Position = 0;

            var kernel2 = Ser.deserialize(stream, new TxKernel());
            Assert.Equal(KernelFeatures.DEFAULT_KERNEL, kernel2.features);
            Assert.Equal<ulong>(0, kernel2.lock_height);
            Assert.Equal(commit.Value, kernel2.excess.Value);
            Assert.Equal(sig, kernel2.excess_sig);
            Assert.Equal<ulong>(10, kernel2.fee);

            //// now check a kernel with lock_height serializes/deserializes correctly
            kernel = new TxKernel
            {
                features = KernelFeatures.DEFAULT_KERNEL,
                lock_height = 100,
                excess = commit,
                excess_sig = sig,
                fee = 10
            };
            
            stream = new MemoryStream();
            Ser.serialize(stream, kernel);

            Console.WriteLine("-------");
            Console.WriteLine(stream.ToArray().AsString());
            Console.WriteLine("-------");

            stream.Position = 0;

            kernel2 = Ser.deserialize(stream, new TxKernel());
            Assert.Equal(KernelFeatures.DEFAULT_KERNEL, kernel2.features);
            Assert.Equal<ulong>(100, kernel2.lock_height);
            Assert.Equal(commit.Value, kernel2.excess.Value);
            Assert.Equal(sig, kernel2.excess_sig);
            Assert.Equal<ulong>(10, kernel2.fee);
        }

        [Fact]
        public void test_output_ser_deser()
        {
            var keychain = Keychain.Keychain.From_random_seed();
            var key_id_set = keychain.Derive_key_id(1);
            var commit = keychain.Commit(5, key_id_set);
            var switch_commit = keychain.Switch_commit(key_id_set);
            var switch_commit_hash = SwitchCommitHash.From_switch_commit(switch_commit);
            var msg = ProofMessage.empty();
            var proof = keychain.Range_proof(5, key_id_set, commit, msg);

            var outp = new Output { 
                features= OutputFeatures.DEFAULT_OUTPUT,
                commit= commit,
                switch_commit_hash= switch_commit_hash,
                proof= proof
            };

            var stream = new MemoryStream();
            Ser.serialize(stream, outp);

            Console.WriteLine("-------");
            Console.WriteLine(stream.ToArray().AsString());
            Console.WriteLine("-------");

            stream.Position = 0;

            var dout = Ser.deserialize(stream, new Output());

            Assert.Equal(OutputFeatures.DEFAULT_OUTPUT, dout.features);
            Assert.Equal(outp.commit.Value , dout.commit.Value);
            Assert.Equal(outp.proof.Proof, dout.proof.Proof);
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