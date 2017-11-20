using System;
using Grin.Keychain;
using Secp256k1Proxy;
using Serilog;

namespace Grin.Core.Core
{
    public class BlockHeader
    {
        private BlockHeader(ushort version, ulong height, Hash previous, DateTime timestamp, Hash utxoRoot,
            Hash rangeProofRoot, Hash kernelRoot, ulong nonce, Proof pow, Difficulty difficulty,
            Difficulty totalDifficulty)
        {
            this.version = version;
            this.height = height;
            this.previous = previous;
            this.timestamp = timestamp;
            utxo_root = utxoRoot;
            range_proof_root = rangeProofRoot;
            kernel_root = kernelRoot;
            this.nonce = nonce;
            this.pow = pow;
            this.difficulty = difficulty;
            total_difficulty = totalDifficulty;
        }


        /// Version of the block
        public ushort version { get; }

        /// Height of this block since the genesis block (height 0)
        public ulong height { get; }

        /// Hash of the block previous to this in the chain.
        public Hash previous { get; }

        /// Timestamp at which the block was built.
        public DateTime timestamp { get; }

        /// Merklish root of all the commitments in the UTXO set
        public Hash utxo_root { get; }

        /// Merklish root of all range proofs in the UTXO set
        public Hash range_proof_root { get; }

        /// Merklish root of all transaction kernels in the UTXO set
        public Hash kernel_root { get; }

        /// Nonce increment used to mine this block.
        public ulong nonce { get; }

        /// Proof of work data.
        public Proof pow { get; }

        /// Difficulty used to mine the block.
        public Difficulty difficulty { get; }

        /// Total accumulated difficulty since genesis block
        public Difficulty total_difficulty { get; }


        public static BlockHeader Default()
        {
            var proofSize = Global.proofsize();
            // ReSharper disable ArgumentsStyleLiteral
            // ReSharper disable ArgumentsStyleOther
            // ReSharper disable ArgumentsStyleNamedExpression
            return new BlockHeader(
                version: 1,
                height: 0,
                previous: Hash.ZERO_HASH(),
                timestamp: DateTime.UtcNow,
                difficulty: Difficulty.From_num(Consensus.MINIMUM_DIFFICULTY),
                totalDifficulty: Difficulty.From_num(Consensus.MINIMUM_DIFFICULTY),
                utxoRoot: Hash.ZERO_HASH(),
                rangeProofRoot: Hash.ZERO_HASH(),
                kernelRoot: Hash.ZERO_HASH(),
                nonce: 0,
                pow: Proof.Zero(proofSize)
            );
            // ReSharper restore ArgumentsStyleNamedExpression
            // ReSharper restore ArgumentsStyleLiteral
            // ReSharper restore ArgumentsStyleOther
        }
    }

    public class Block
    {
        /// The header with metadata and commitments to the rest of the data
        public BlockHeader header { get; }
        /// List of transaction inputs
        public Input[] inputs { get; }
        /// List of transaction outputs
        public Output[] outputs { get; }
    /// List of transaction kernels and associated proofs
    public TxKernel[] kernels { get; }


        public static Block New(BlockHeader @default, Transaction[] txs, Keychain.Keychain keychain, Identifier keyId)
        {
            throw new NotImplementedException();
        }

        public static (Output, TxKernel) Reward_output(Keychain.Keychain keychain, Identifier keyId, ulong fees)
        {

     var secp = keychain.Secp;

            var commit = keychain.Commit(Consensus.reward(fees), keyId);
            var switch_commit = keychain.Switch_commit(keyId);
            var switch_commit_hash = SwitchCommitHash.From_switch_commit(switch_commit);

            Log.Verbose(
                "Block reward - Pedersen Commit is: {commit}, Switch Commit is: {switch_commit}",
                commit,
                switch_commit
                );

            Log.Verbose(
              "Block reward - Switch Commit Hash is: {  switch_commit_hash}",
                switch_commit_hash
                );

            var msg = ProofMessage.empty();
            var rproof = keychain.Range_proof(Consensus.reward(fees), keyId, commit, msg);

            var output = new  Output {
                features =  OutputFeatures.COINBASE_OUTPUT,
                    commit= commit,
                    switch_commit_hash= switch_commit_hash,
                    proof= rproof
                };

            var over_commit = secp.commit_value(Consensus.reward(fees));
            var out_commit = output.commit;
            var excess = secp.commit_sum(new []{out_commit}, new[] { over_commit}) ;

            var msg2 = Message.from_slice(new byte[Constants.MESSAGE_SIZE]);
            var sig = keychain.Sign(msg2, keyId);

            var proof = new TxKernel {
                features= KernelFeatures.COINBASE_KERNEL,
                    excess=excess,
                    excess_sig= sig.serialize_der(secp),
                    fee= 0,
                    lock_height= 0
                };
            return (output, proof);
        }
    }

}