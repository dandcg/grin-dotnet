using System;
using Common;
using Grin.Chain;
using Grin.Core.Core.Block;
using Grin.Core.Core.Transaction;
using Secp256k1Proxy.Pedersen;

namespace Grin.Api
{
    /// The state of the current fork tip
    public class ApiTip : ICloneable
    {
        /// Height of the tip (max height of the fork)
        public ulong height { get; set; }

        // Last block pushed to the fork
        public string last_block_pushed { get; set; }

        // Block previous to last
        public string prev_block_to_last { get; set; }

        // Total difficulty accumulated on that fork
        public ulong total_difficulty { get; set; }


        public static ApiTip from_tip(ChainTip tip)
        {
            return new ApiTip
            {
                height = tip.height,
                last_block_pushed = HexUtil.to_hex(tip.last_block_h.Value),
                prev_block_to_last = HexUtil.to_hex(tip.prev_block_h.Value),
                total_difficulty = tip.total_difficulty.into_num()
            };
        }


        public object Clone()
        {
            return new ApiTip
            {
                height = height,
                last_block_pushed = last_block_pushed,
                prev_block_to_last = prev_block_to_last,
                total_difficulty = total_difficulty
            };
        }
    }


    /// Sumtrees
    public class ApiSumTrees : ICloneable
    {
        /// UTXO Root Hash
        public string utxo_root_hash { get; set; }

        // UTXO Root Sum
        public string utxo_root_sum { get; set; }

        // Rangeproof root hash
        public string range_proof_root_hash { get; set; }

        // Kernel set root hash
        public string kernel_root_hash { get; set; }

        public static ApiSumTrees from_head(Chain.Chain head)
        {
            throw new NotImplementedException();

            //var roots = head.get_sumtree_roots();

            // return new ApiSumTrees {
            //     utxo_root_hash= HexUtil.to_hex(roots.0.hash.to_vec()),
            //     utxo_root_sum= HexUtil.to_hex(roots.0.sum.commit.0.to_vec()),
            //     range_proof_root_hash= HexUtil.to_hex(roots.1.hash.to_vec()),
            //     kernel_root_hash= HexUtil.to_hex(roots.2.hash.to_vec()),
            // }
        }

        public object Clone()
        {
            return new ApiSumTrees
            {
                utxo_root_hash = utxo_root_hash,
                utxo_root_sum = utxo_root_sum,
                range_proof_root_hash = range_proof_root_hash,
                kernel_root_hash = kernel_root_hash
            };
        }
    }


    /// Wrapper around a list of sumtree nodes, so it can be
    /// presented properly via json
    public class ApiSumTreeNode : ICloneable
    {
        // The hash
        public string hash { get; set; }

        // Output (if included)
        public ApiOutputPrintable output { get; set; }


        public static ApiSumTreeNode[] get_last_n_utxo(Chain.Chain chain, ulong distance)
        {
            throw new NotImplementedException();

            //let mut return_vec = Vec::new();
            //let last_n = chain.get_last_n_utxo(distance);
            //for elem_output in last_n {
            //    let header = chain
            //        .get_block_header_by_output_commit(&elem_output.1.commit)
            //        .map_err(| _ | Error::NotFound);
            //    // Need to call further method to check if output is spent
            //    let mut output = OutputPrintable::from_output(&elem_output.1, &header.unwrap(), true);
            //    if let Ok(_) = chain.get_unspent(&elem_output.1.commit) {
            //        output.spent = false;
            //    }
            //    return_vec.push(SumTreeNode {
            //        hash: util::to_hex(elem_output.0.to_vec()),
            //        output: Some(output),
            //    });
            //}
            //return_vec
        }

        public static ApiSumTreeNode[] get_last_n_rangeproof(Chain.Chain head, ulong distance)

        {
            throw new NotImplementedException();

            //    let mut return_vec = Vec::new();
            //let last_n = head.get_last_n_rangeproof(distance);
            //for elem in last_n {
            //    return_vec.push(SumTreeNode {
            //        hash: util::to_hex(elem.hash.to_vec()),
            //        output: None,
            //    });
            //}
            //return_vec
        }

        public static ApiSumTreeNode[] get_last_n_kernel(Chain.Chain head, ulong distance)
        {
            throw new NotImplementedException();
            //    let mut return_vec = Vec::new();
            //let last_n = head.get_last_n_kernel(distance);
            //for elem in last_n {
            //    return_vec.push(SumTreeNode {
            //        hash: util::to_hex(elem.hash.to_vec()),
            //        output: None,
            //    });
            //}
            //return_vec
        }

        public object Clone()
        {
            throw new NotImplementedException();
        }
    }


    public enum ApiOutputType
    {
        Coinbase,
        Transaction
    }


    public struct ApiOutput : ICloneable
    {
        /// The type of output Coinbase|Transaction
        public ApiOutputType output_type { get; set; }

        /// The homomorphic commitment representing the output's amount
        public Commitment commit { get; set; }

        /// switch commit hash
        public SwitchCommitHash switch_commit_hash { get; set; }

        /// A proof that the commitment is in the right range
        public RangeProof proof { get; set; }

        /// The height of the block creating this output
        public ulong height { get; set; }

        /// The lock height (earliest block this output can be spent)
        public ulong lock_height { get; set; }


        public static ApiOutput from_output(Output output, BlockHeader block_header, bool include_proof,
            bool include_switch)
        {
            throw new NotImplementedException();

            //          let(output_type, lock_height) = match output.features {
            //              x if x.contains(core::transaction::COINBASE_OUTPUT) => (
            //                  OutputType::Coinbase,
            //                  block_header.height + global::coinbase_maturity(),

            //              ),
            //	_ => (OutputType::Transaction, 0),
            //};

            //          Output {
            //              output_type: output_type,
            //	commit: output.commit,
            //	switch_commit_hash: match include_switch {
            //                  true => Some(output.switch_commit_hash),
            //		false => None,
            //	},
            //	proof: match include_proof {
            //                  true => Some(output.proof),
            //		false => None,
            //	},
            //	height: block_header.height,
            //	lock_height: lock_height,
        }

        public object Clone()
        {
            throw new NotImplementedException();
        }
    }

    // As above, except formatted a bit better for human viewing
    public class ApiOutputPrintable : ICloneable
    {
        /// The type of output Coinbase|Transaction
        public ApiOutputType output_type { get; set; }

        /// The homomorphic commitment representing the output's amount (as hex
        /// string)
        public string commit { get; set; }

        /// switch commit hash
        public string switch_commit_hash { get; set; }

        /// The height of the block creating this output
        public ulong height { get; set; }

        /// The lock height (earliest block this output can be spent)
        public ulong lock_height { get; set; }

        /// Whether the output has been spent
        public bool spent { get; set; }

        /// Rangeproof hash  (as hex string)
        public string proof_hash { get; set; }


        public static ApiOutputPrintable from_output(Output output, BlockHeader block_header, bool include_proof_hash)
        {
            throw new NotImplementedException();

            //let(output_type, lock_height) = match output.features {
            //    x if x.contains(core::transaction::COINBASE_OUTPUT) => (
            //        OutputType::Coinbase,
            //        block_header.height + global::coinbase_maturity(), 

            //        ),
            //    _ => (OutputType::Transaction, 0),
            //}
            //;
            //OutputPrintable {
            //    output_type:
            //    output_type,
            //    commit:
            //    util::to_hex(output.commit.0.to_vec()),
            //    switch_commit_hash:
            //    util::to_hex(output.switch_commit_hash.hash.to_vec()),
            //    height:
            //    block_header.height,
            //    lock_height:
            //    lock_height,
            //    spent:
            //    true,
            //    proof_hash:
            //    match include_proof_hash {
            //        true => Some(util::to_hex(output.proof.hash().to_vec())),
            //        false => None,
            //    }
            //}
        }

        public object Clone()
        {
            throw new NotImplementedException();
        }
    }

    // As above, except just the info needed for wallet reconstruction

    public class ApiOutputSwitch : ICloneable
    {
        /// the commit
        public string commit { get; set; }

        /// switch commit hash
        public byte[] switch_commit_hash { get; set; } //[u8; core::SWITCH_COMMIT_HASH_SIZE],

        /// The height of the block creating this output
        public ulong height { get; set; }


        public static ApiOutputSwitch from_output(Output output, BlockHeader block_header)
        {
            throw new NotImplementedException();

            //          OutputSwitch {
            //              commit: util::to_hex(output.commit.0.to_vec()),
            //	switch_commit_hash: output.switch_commit_hash.hash,
            //	height: block_header.height,
            //}
        }

        public object Clone()
        {
            throw new NotImplementedException();
        }
    }
    // Just the information required for wallet reconstruction

    public class ApiBlockHeaderInfo : ICloneable
    {
        /// Hash
        public string hash { get; set; }

        /// Previous block hash
        public string previous { get; set; }

        /// Height
        public ulong height { get; set; }


        public static ApiBlockHeaderInfo from_header(BlockHeader block_header)
        {
            throw new NotImplementedException();

            //          BlockHeaderInfo {
            //              hash: util::to_hex(block_header.hash().to_vec()),
            //	previous: util::to_hex(block_header.previous.to_vec()),
            //	height: block_header.height,
            //}
        }

        public object Clone()
        {
            throw new NotImplementedException();
        }
    }

// For wallet reconstruction, include the header info along with the
// transactions in the block

    public class ApiBlockOutputs : ICloneable
    {
        /// The block header
        public ApiBlockHeaderInfo header { get; set; }

        /// A printable version of the outputs
        public ApiOutputSwitch[] outputs { get; set; }

        public object Clone()
        {
            throw new NotImplementedException();
        }
    }


    public class ApiPoolInfo
    {
        /// Size of the pool
        public uint pool_size { get; set; }

        /// Size of orphans
        public uint orphans_size { get; set; }

        /// Total size of pool + orphans
        public uint total_size { get; set; }
    }
}