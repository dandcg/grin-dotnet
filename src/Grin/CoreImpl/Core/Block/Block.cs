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
        public BlockHeader header { get; private set; }

        /// List of transaction inputs
        public Input[] inputs { get; private set; }

        /// List of transaction outputs
        public Output[] outputs { get; private set; }

        /// List of transaction kernels and associated proofs
        public TxKernel[] kernels { get; private set; }

        /// Default properties for a block, everything zeroed out and empty vectors.
        public static Block Default()
        {
            return new Block
            {
                header = BlockHeader.Default(),
                inputs = null,
                outputs = null,
                kernels = null
            };
        }


        /// Builds a new block from the header of the previous block, a vector of
        /// transactions and the private key that will receive the reward. Checks
        /// that all transactions are valid and calculates the Merkle tree.
        /// 
        /// Only used in tests (to be confirmed, may be wrong here).
        public static Block New(BlockHeader prev, Transaction.Transaction[] txs, Keychain keychain, Identifier keyId)
        {
            var txfees = txs.Select(s => s.fee).ToArray();

            ulong fees = 0;

            if (txfees.Any())
            {
                fees = txfees.Aggregate((t, t1) => t + t1);
            }


            var (reward_out, reward_proof) = Reward_output(keychain, keyId, fees);
            var block = with_reward(prev, txs, reward_out, reward_proof);
            return block;
        }

        /// Builds a new block ready to mine from the header of the previous block,
        /// a vector of transactions and the reward information. Checks
        /// that all transactions are valid and calculates the Merkle tree.
        public static Block with_reward(BlockHeader prev, Transaction.Transaction[] txs, Output reward_out,
            TxKernel reward_kern)
        {
            // note: the following reads easily but may not be the most efficient due to
            // repeated iterations, revisit if a problem
            var secp = Secp256k1.WithCaps(ContextFlag.Commit);

            // validate each transaction and gather their kernels

            var kernels = txs.Select(tx => tx.verify_sig(secp)).ToList();
            kernels.Add(reward_kern);

            // build vectors with all inputs and all outputs, ordering them by hash
            // needs to be a fold so we don't end up with a vector of vectors and we
            // want to fully own the refs (not just a pointer like flat_map).

            var inputs = new List<Input>();
            var outputs = new List<Output>();
            foreach (var tx in txs)
            {
                foreach (var i in tx.inputs)
                {
                    inputs.Add(i.Clone());
                }

                foreach (var o in tx.outputs)
                {
                    outputs.Add(o.Clone());
                }
            }

            outputs.Add(reward_out);

            // calculate the overall Merkle tree and fees

            var bh = BlockHeader.Default();

            bh.height = prev.height + 1;
            bh.previous = prev.hash();
            bh.timestamp = DateTime.UtcNow;

            bh.total_difficulty =
                Difficulty.From_num(prev.pow.Clone().to_difficulty().num + prev.total_difficulty.Clone().num);

            var b = new Block
            {
                header = bh,
                inputs = inputs.ToArray(),
                outputs = outputs.ToArray(),
                kernels = kernels.ToArray()
            };

            return b.compact();
        }


        // Blockhash, computed using only the header
        public Hash.Hash hash()
        {
            return header.hash();
        }

        /// Sum of all fees (inputs less outputs) in the block
        public ulong total_fees()
        {
            return kernels.Select(k => k.fee).Aggregate((t, t1) => t + t1);
        }

        /// Matches any output with a potential spending input, eliminating them
        /// from the block. Provides a simple way to compact the block. The
        /// elimination is stable with respect to inputs and outputs order.
        /// 
        /// NOTE: exclude coinbase from compaction process
        /// if a block contains a new coinbase output and
        /// is a transaction spending a previous coinbase
        /// we do not want to compact these away
        public Block compact()
        {
            var in_set = new HashSet<string>();

            foreach (var i in inputs)
            {
                in_set.Add(i.Commitment.Hex);
            }


            var out_set = new HashSet<string>();

            foreach (var o in outputs.Where(w => !w.Features.HasFlag(OutputFeatures.COINBASE_OUTPUT)))
            {
                out_set.Add(o.Commit.Hex);
            }


            var commitments_to_compact = in_set.Intersect(out_set);

            var new_inputs = inputs.Where(w => !commitments_to_compact.Contains(w.Commitment.Hex))
                .Select(s => s.Clone());

            var new_outputs = outputs.Where(w => !commitments_to_compact.Contains(w.Commit.Hex)).Select(s => s.Clone());


            var new_kernels = kernels.Select(s => s.Clone());


            var b = new Block
            {
                header = header.Clone(),
                inputs = new_inputs.ToArray(),
                outputs = new_outputs.ToArray(),
                kernels = new_kernels.ToArray()
            };

            return b;
        }

        /// Merges the 2 blocks, essentially appending the inputs, outputs and
        /// kernels.
        /// Also performs a compaction on the result.
        public Block merge(Block other)

        {
            var all_inputs = inputs.Select(s => s.Clone()).ToList();
            all_inputs.AddRange(other.inputs.Select(s => s.Clone()));

            var all_outputs = outputs.Select(s => s.Clone()).ToList();
            all_outputs.AddRange(other.outputs.Select(s => s.Clone()));

            var all_kernels = kernels.Select(s => s.Clone()).ToList();
            all_kernels.AddRange(other.kernels.Select(s => s.Clone()));

            var b = new Block
            {
                // compact will fix the merkle tree
                header = header.Clone(),

                inputs = all_inputs.ToArray(),
                outputs = all_outputs.ToArray(),
                kernels = all_kernels.ToArray()
            }.compact();

            return b;
        }

        /// Validates all the elements in a block that can be checked without
        /// additional data. Includes commitment sums and kernels, Merkle
        /// trees, reward, etc.
        /// 
        /// TODO - performs various verification steps - discuss renaming this to "verify"
        public void validate(Secp256k1 secp)
        {
            if (Consensus.Exceeds_weight((uint) inputs.Length, (uint) outputs.Length, (uint) kernels.Length))
            {
                throw new BlockErrorException(BlockError.WeightExceeded);
            }
            verify_coinbase();
            verify_kernels(secp, false);
        }

        /// Verifies the sum of input/output commitments match the sum in kernels
        /// and that all kernel signatures are valid.
        /// TODO - when would we skip_sig? Is this needed or used anywhere?
        public void verify_kernels(Secp256k1 secp, bool skip_sig)

        {
            foreach (var k in kernels)
            {
                if ((k.fee & 1) != 0)
                {
                    //throw new BlockErrorException(BlockError.OddKernelFee);
                }

                if (k.lock_height > header.height)

                {
                    throw new BlockErrorException(BlockError.KernelLockHeight).Data("lock_height", k.lock_height);
                }


                // sum all inputs and outs commitments
                var io_sum = this.sum_commitments(secp);

// sum all kernels commitments
                var proof_commits = kernels.Select(s => s.excess).ToArray();
                var proof_sum = secp.commit_sum(proof_commits, new Commitment[] { });

                // both should be the same
                if (proof_sum.Hex != io_sum.Hex)
                {
                    throw new BlockErrorException(BlockError.KernelSumMismatch);
                }

                // verify all signatures with the commitment as pk
                if (!skip_sig)
                {
                    foreach (var proof in kernels)
                    {
                        proof.verify(secp);
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
            var cb_outs = outputs.Where(w => w.Features.HasFlag(OutputFeatures.COINBASE_OUTPUT)).Select(s => s.Commit)
                .ToArray();

            var cb_kerns = kernels.Where(w => w.features.HasFlag(KernelFeatures.COINBASE_KERNEL)).Select(s => s.excess)
                .ToArray();

            var secp = SecpStatic.Instance;

            Commitment out_adjust_sum;
            Commitment kerns_sum;
            try
            {
                var over_commit = secp.commit_value(Consensus.Reward(total_fees()));
                out_adjust_sum = secp.commit_sum(cb_outs, new[] {over_commit});
                kerns_sum = secp.commit_sum(cb_kerns, new Commitment[] { });
            }
            catch (Exception ex)
            {
                throw new BlockErrorException(BlockError.Secp, ex);
            }

            if (kerns_sum.Hex != out_adjust_sum.Hex)
            {
                throw new BlockErrorException(BlockError.CoinbaseSumMismatch);
            }
        }


        public static (Output, TxKernel) Reward_output(Keychain keychain, Identifier keyId, ulong fees)
        {
            var secp = keychain.Secp;

            var commit = keychain.Commit(Consensus.Reward(fees), keyId);
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
            var rproof = keychain.Range_proof(Consensus.Reward(fees), keyId, commit, msg);

            var output = new Output
            {
                Features = OutputFeatures.COINBASE_OUTPUT,
                Commit = commit,
                SwitchCommitHash = switch_commit_hash,
                Proof = rproof
            };

            var over_commit = secp.commit_value(Consensus.Reward(fees));
            var out_commit = output.Commit;
            var excess = secp.commit_sum(new[] {out_commit}, new[] {over_commit});

            var msg2 = Message.from_slice(new byte[Constants.MESSAGE_SIZE]);
            var sig = keychain.Sign(msg2, keyId);

            var proof = new TxKernel
            {
                features = KernelFeatures.COINBASE_KERNEL,
                excess = excess,
                excess_sig = sig.serialize_der(secp),
                fee = 0,
                lock_height = 0
            };
            return (output, proof);
        }


        /// Implementation of Readable for a block, defines how to read a full block
        /// from a binary stream.
        public void read(IReader reader)
        {
            header = BlockHeader.readnew(reader);

            var input_len = reader.read_u64();
            var output_len = reader.read_u64();
            var kernel_len = reader.read_u64();

            inputs = Ser.Ser.Read_and_verify_sorted<Input>(reader, input_len);
            outputs = Ser.Ser.Read_and_verify_sorted<Output>(reader, output_len);
            kernels = Ser.Ser.Read_and_verify_sorted<TxKernel>(reader, kernel_len);

        }


        /// Implementation of Writeable for a block, defines how to write the block to a
        /// binary writer. Differentiates between writing the block for the purpose of
        /// full serialization and the one of just extracting a hash.
        public void write(IWriter writer)
        {
            header.write(writer);

            if (writer.serialization_mode() != SerializationMode.Hash)
            {
                //Console.WriteLine("{0},{1},{2}", inputs.Length, outputs.Length, kernels.Length);

                writer.write_u64((ulong) inputs.Length);
                writer.write_u64((ulong) outputs.Length);
                writer.write_u64((ulong) kernels.Length);


                // Consensus rule that everything is sorted in lexicographical order on the wire.


                var inputsSorted = inputs.OrderBy(o => o.hash().Hex);
                foreach (var input in inputsSorted)
                {
                    input.write(writer);
                }

                var outputsSorted = outputs.OrderBy(o => o.hash().Hex);
                foreach (var output in outputsSorted)
                {
                    output.write(writer);
                }

                var kernelsSorted = kernels.OrderBy(o => o.hash().Hex);
                foreach (var kernel in kernelsSorted)
                {
                    kernel.write(writer);
                }
            }
        }

        public Input[] inputs_commited()
        {
            return inputs;
        }

        public Output[] outputs_committed()
        {
            return outputs;
        }

        public long overage()
        {
            return (long) total_fees() / 2 - (long) Consensus.REWARD;
        }
    }
}