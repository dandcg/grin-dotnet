using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Grin.CoreImpl.Core.Hash;
using Grin.CoreImpl.Core.Mod;
using Grin.CoreImpl.Core.Target;
using Grin.CoreImpl.Core.Transaction;
using Grin.CoreImpl.Ser;
using Grin.KeychainImpl;
using Grin.KeychainImpl.ExtKey;
using Grin.UtilImpl;
using Secp256k1Proxy.Constants;
using Secp256k1Proxy.Lib;
using Secp256k1Proxy.Pedersen;
using Serilog;

namespace Grin.CoreImpl.Core.Block
{
    public class Block : IReadable, IWriteable, ICommitted
    {
        private Block()
        {
        }

        /// The header with metadata and commitments to the rest of the data
        public BlockHeader Header { get; private set; }

        /// List of transaction inputs
        public Input[] Inputs { get; private set; }

        /// List of transaction outputs
        public Output[] Outputs { get; private set; }

        /// List of transaction kernels and associated proofs
        public TxKernel[] Kernels { get; private set; }

        /// Default properties for a block, everything zeroed out and empty vectors.
        public static Block Default()
        {
            return new Block
            {
                Header = BlockHeader.Default(),
                Inputs = null,
                Outputs = null,
                Kernels = null
            };
        }


        /// Builds a new block from the header of the previous block, a vector of
        /// transactions and the private key that will receive the reward. Checks
        /// that all transactions are valid and calculates the Merkle tree.
        /// 
        /// Only used in tests (to be confirmed, may be wrong here).
        public static Block New(BlockHeader prev, Transaction.Transaction[] txs, Keychain keychain, Identifier keyId)
        {
            var txfees = txs.Select(s => s.Fee).ToArray();

            ulong fees = 0;

            if (txfees.Any())
            {
                fees = txfees.Aggregate((t, t1) => t + t1);
            }


            var (rewardOut, rewardProof) = Reward_output(keychain, keyId, fees);
            var block = with_reward(prev, txs, rewardOut, rewardProof);
            return block;
        }

        /// Builds a new block ready to mine from the header of the previous block,
        /// a vector of transactions and the reward information. Checks
        /// that all transactions are valid and calculates the Merkle tree.
        public static Block with_reward(BlockHeader prev, Transaction.Transaction[] txs, Output rewardOut,
            TxKernel rewardKern)
        {
            // note: the following reads easily but may not be the most efficient due to
            // repeated iterations, revisit if a problem
            var secp = Secp256K1.WithCaps(ContextFlag.Commit);

            // validate each transaction and gather their kernels

            var kernels = txs.Select(tx => tx.verify_sig(secp)).ToList();
            kernels.Add(rewardKern);

            // build vectors with all inputs and all outputs, ordering them by hash
            // needs to be a fold so we don't end up with a vector of vectors and we
            // want to fully own the refs (not just a pointer like flat_map).

            var inputs = new List<Input>();
            var outputs = new List<Output>();
            foreach (var tx in txs)
            {
                inputs.AddRange(tx.Inputs.Select(i => i.Clone()));

                outputs.AddRange(tx.Outputs.Select(o => o.Clone()));
            }

            outputs.Add(rewardOut);

            // calculate the overall Merkle tree and fees

            var bh = BlockHeader.Default();

            bh.Height = prev.Height + 1;
            bh.Previous = prev.Hash();
            bh.Timestamp = DateTime.UtcNow;

            bh.TotalDifficulty =
                Difficulty.From_num(prev.Pow.Clone().To_difficulty().Num + prev.TotalDifficulty.Clone().Num);

            var b = new Block
            {
                Header = bh,
                Inputs = inputs.ToArray(),
                Outputs = outputs.ToArray(),
                Kernels = kernels.ToArray()
            };

            return b.Compact();
        }


        // Blockhash, computed using only the header
        public Hash.Hash Hash()
        {
            return Header.Hash();
        }

        /// Sum of all fees (inputs less outputs) in the block
        public ulong total_fees()
        {
            return Kernels.Select(k => k.Fee).Aggregate((t, t1) => t + t1);
        }

        /// Matches any output with a potential spending input, eliminating them
        /// from the block. Provides a simple way to compact the block. The
        /// elimination is stable with respect to inputs and outputs order.
        /// 
        /// NOTE: exclude coinbase from compaction process
        /// if a block contains a new coinbase output and
        /// is a transaction spending a previous coinbase
        /// we do not want to compact these away
        public Block Compact()
        {
            var inSet = new HashSet<string>();

            foreach (var i in Inputs)
            {
                inSet.Add(i.Commitment.Hex);
            }


            var outSet = new HashSet<string>();

            foreach (var o in Outputs.Where(w => !w.Features.HasFlag(OutputFeatures.CoinbaseOutput)))
            {
                outSet.Add(o.Commit.Hex);
            }


            var commitmentsToCompact = inSet.Intersect(outSet);

            var newInputs = Inputs.Where(w => !commitmentsToCompact.Contains(w.Commitment.Hex))
                .Select(s => s.Clone());

            var newOutputs = Outputs.Where(w => !commitmentsToCompact.Contains(w.Commit.Hex)).Select(s => s.Clone());


            var newKernels = Kernels.Select(s => s.Clone());


            var b = new Block
            {
                Header = Header.Clone(),
                Inputs = newInputs.ToArray(),
                Outputs = newOutputs.ToArray(),
                Kernels = newKernels.ToArray()
            };

            return b;
        }

        /// Merges the 2 blocks, essentially appending the inputs, outputs and
        /// kernels.
        /// Also performs a compaction on the result.
        public Block Merge(Block other)

        {
            var allInputs = Inputs.Select(s => s.Clone()).ToList();
            allInputs.AddRange(other.Inputs.Select(s => s.Clone()));

            var allOutputs = Outputs.Select(s => s.Clone()).ToList();
            allOutputs.AddRange(other.Outputs.Select(s => s.Clone()));

            var allKernels = Kernels.Select(s => s.Clone()).ToList();
            allKernels.AddRange(other.Kernels.Select(s => s.Clone()));

            var b = new Block
            {
                // compact will fix the merkle tree
                Header = Header.Clone(),

                Inputs = allInputs.ToArray(),
                Outputs = allOutputs.ToArray(),
                Kernels = allKernels.ToArray()
            }.Compact();

            return b;
        }

        /// Validates all the elements in a block that can be checked without
        /// additional data. Includes commitment sums and kernels, Merkle
        /// trees, reward, etc.
        /// 
        /// TODO - performs various verification steps - discuss renaming this to "verify"
        public void Validate(Secp256K1 secp)
        {
            if (Consensus.Exceeds_weight((uint) Inputs.Length, (uint) Outputs.Length, (uint) Kernels.Length))
            {
                throw new BlockErrorException(BlockError.WeightExceeded);
            }
            verify_coinbase();
            verify_kernels(secp, false);
        }

        /// Verifies the sum of input/output commitments match the sum in kernels
        /// and that all kernel signatures are valid.
        /// TODO - when would we skip_sig? Is this needed or used anywhere?
        public void verify_kernels(Secp256K1 secp, bool skipSig)

        {
            foreach (var k in Kernels)
            {
                if ((k.Fee & 1) != 0)
                {
                    //throw new BlockErrorException(BlockError.OddKernelFee);
                }

                if (k.LockHeight > Header.Height)

                {
                    throw new BlockErrorException(BlockError.KernelLockHeight).Data("lock_height", k.LockHeight);
                }


                // sum all inputs and outs commitments
                var ioSum = this.sum_commitments(secp);

                // sum all kernels commitments
                var proofCommits = Kernels.Select(s => s.Excess).ToArray();
                var proofSum = secp.commit_sum(proofCommits, new Commitment[] { });

                // both should be the same
                if (proofSum.Hex != ioSum.Hex)
                {
                    throw new BlockErrorException(BlockError.KernelSumMismatch);
                }

                // verify all signatures with the commitment as pk
                if (!skipSig)
                {
                    foreach (var proof in Kernels)
                    {
                        proof.Verify(secp);
                    }
                }
            }
        }

        // Validate the coinbase outputs generated by miners. Entails 2 main checks:
        //
        // * That the sum of all coinbase-marked outputs equal the supply.
        // * That the sum of blinding factors for all coinbase-marked outputs match
        //   the coinbase-marked kernels.
        public void verify_coinbase()
        {
            var cbOuts = Outputs.Where(w => w.Features.HasFlag(OutputFeatures.CoinbaseOutput)).Select(s => s.Commit)
                .ToArray();

            var cbKerns = Kernels.Where(w => w.Features.HasFlag(KernelFeatures.CoinbaseKernel)).Select(s => s.Excess)
                .ToArray();

            var secp = SecpStatic.Instance;

            Commitment outAdjustSum;
            Commitment kernsSum;
            try
            {
                var overCommit = secp.commit_value(Consensus.Reward(total_fees()));
                outAdjustSum = secp.commit_sum(cbOuts, new[] {overCommit});
                kernsSum = secp.commit_sum(cbKerns, new Commitment[] { });
            }
            catch (Exception ex)
            {
                throw new BlockErrorException(BlockError.Secp, ex);
            }

            if (kernsSum.Hex != outAdjustSum.Hex)
            {
                throw new BlockErrorException(BlockError.CoinbaseSumMismatch);
            }
        }


        public static (Output, TxKernel) Reward_output(Keychain keychain, Identifier keyId, ulong fees)
        {
            var secp = keychain.Secp;

            var commit = keychain.Commit(Consensus.Reward(fees), keyId);
            var switchCommit = keychain.Switch_commit(keyId);
            var switchCommitHash = SwitchCommitHash.From_switch_commit(switchCommit);

            Log.Verbose(
                "Block reward - Pedersen Commit is: {commit}, Switch Commit is: {switch_commit}",
                commit,
                switchCommit
            );

            Log.Verbose(
                "Block reward - Switch Commit Hash is: {  switch_commit_hash}",
                switchCommitHash
            );

            var msg = ProofMessage.Empty();
            var rproof = keychain.Range_proof(Consensus.Reward(fees), keyId, commit, msg);

            var output = new Output
            {
                Features = OutputFeatures.CoinbaseOutput,
                Commit = commit,
                SwitchCommitHash = switchCommitHash,
                Proof = rproof
            };

            var overCommit = secp.commit_value(Consensus.Reward(fees));
            var outCommit = output.Commit;
            var excess = secp.commit_sum(new[] {outCommit}, new[] {overCommit});

            var msg2 = Message.from_slice(new byte[Constants.MessageSize]);
            var sig = keychain.Sign(msg2, keyId);

            var proof = new TxKernel
            {
                Features = KernelFeatures.CoinbaseKernel,
                Excess = excess,
                ExcessSig = sig.serialize_der(secp),
                Fee = 0,
                LockHeight = 0
            };
            return (output, proof);
        }


        /// Implementation of Readable for a block, defines how to read a full block
        /// from a binary stream.
        public void Read(IReader reader)
        {
            Header = BlockHeader.Readnew(reader);

            var inputLen = reader.read_u64();
            var outputLen = reader.read_u64();
            var kernelLen = reader.read_u64();

            Inputs = Ser.Ser.Read_and_verify_sorted<Input>(reader, inputLen);
            Outputs = Ser.Ser.Read_and_verify_sorted<Output>(reader, outputLen);
            Kernels = Ser.Ser.Read_and_verify_sorted<TxKernel>(reader, kernelLen);

        }


        /// Implementation of Writeable for a block, defines how to write the block to a
        /// binary writer. Differentiates between writing the block for the purpose of
        /// full serialization and the one of just extracting a hash.
        public void Write(IWriter writer)
        {
            Header.Write(writer);

            if (writer.serialization_mode() != SerializationMode.Hash)
            {
                //Console.WriteLine("{0},{1},{2}", inputs.Length, outputs.Length, kernels.Length);

                writer.write_u64((ulong) Inputs.Length);
                writer.write_u64((ulong) Outputs.Length);
                writer.write_u64((ulong) Kernels.Length);


                // Consensus rule that everything is sorted in lexicographical order on the wire.


                var inputsSorted = Inputs.OrderBy(o => o.Hash().Hex);
                foreach (var input in inputsSorted)
                {
                    input.Write(writer);
                }

                var outputsSorted = Outputs.OrderBy(o => o.Hash().Hex);
                foreach (var output in outputsSorted)
                {
                    output.Write(writer);
                }

                var kernelsSorted = Kernels.OrderBy(o => o.Hash().Hex);
                foreach (var kernel in kernelsSorted)
                {
                    kernel.Write(writer);
                }
            }
        }

        public Input[] inputs_commited()
        {
            return Inputs;
        }

        public Output[] outputs_committed()
        {
            return Outputs;
        }

        public long Overage()
        {
            return (long) total_fees() / 2 - (long) Consensus.RewardAmount;
        }
    }
}