using System;
using Grin.Keychain;

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
            throw new NotImplementedException();
            //    let secp = keychain.secp();

            //    let commit = keychain.commit(reward(fees), key_id_set) ?;
            //    let switch_commit = keychain.switch_commit(key_id_set) ?;
            //    let switch_commit_hash = SwitchCommitHash::from_switch_commit(switch_commit);
            //    trace!(
            //        LOGGER,
            //        "Block reward - Pedersen Commit is: {:?}, Switch Commit is: {:?}",
            //        commit,
            //        switch_commit
            //        );
            //    trace!(
            //        LOGGER,
            //        "Block reward - Switch Commit Hash is: {:?}",
            //        switch_commit_hash
            //        );
            //    let msg = secp::pedersen::ProofMessage::empty();
            //    let rproof = keychain.range_proof(reward(fees), key_id_set, commit, msg) ?;

            //    let output = Output {
            //        features: COINBASE_OUTPUT,
            //        commit: commit,
            //        switch_commit_hash: switch_commit_hash,
            //        proof: rproof,
            //    };

            //    let over_commit = secp.commit_value(reward(fees)) ?;
            //    let out_commit = output.commitment();
            //    let excess = secp.commit_sum(vec![out_commit], vec![over_commit]) ?;

            //    let msg = secp::Message::from_slice(&[0; secp::constants::MESSAGE_SIZE])?;
            //    let sig = keychain.sign(&msg, &key_id_set) ?;

            //    let proof = TxKernel {
            //        features: COINBASE_KERNEL,
            //        excess: excess,
            //        excess_sig: sig.serialize_der(&secp),
            //        fee: 0,
            //        lock_height: 0,
            //    };
            //    Ok((output, proof))

        }
    }

}