using System;
using Grin.ChainImpl;

namespace Grin.ApiImpl.ApiTypes
{
    /// Wrapper around a list of sumtree nodes, so it can be
    /// presented properly via json
    public class ApiSumTreeNode : ICloneable
    {
        // The hash
        public string Hash { get; set; }

        // Output (if included)
        public ApiOutputPrintable Output { get; set; }


        public static ApiSumTreeNode[] get_last_n_utxo(Chain chain, ulong distance)
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

        public static ApiSumTreeNode[] get_last_n_rangeproof(Chain head, ulong distance)

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

        public static ApiSumTreeNode[] get_last_n_kernel(Chain head, ulong distance)
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
}