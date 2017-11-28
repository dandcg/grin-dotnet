using System;
using Grin.ChainImpl;

namespace Grin.Api.ApiTypes
{
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

        public static ApiSumTrees from_head(Chain head)
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
}