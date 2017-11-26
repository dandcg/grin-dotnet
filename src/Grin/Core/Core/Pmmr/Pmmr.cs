using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grin.Core.Core
{
    /// Trait for an element of the tree that has a well-defined sum and hash that
    /// the tree can sum over
    public interface ISummable
    {

        /// Obtain the sum of the element
        Sum sum();
        /// Length of the Sum type when serialized. Can be used as a hint by
        /// underlying storages.
        uint sum_len();

    }

    public class Sum
    {
        
    }


    public class NullSum : ISummable, IWriteable, IReadable
    {
        public void write(IWriter writer)
        {
            throw new NotImplementedException();
        }

        public void read(IReader reader)
        {
            throw new NotImplementedException();
        }

        public Sum sum()
        {
            throw new NotImplementedException();
        }

        public uint sum_len()
        {
            throw new NotImplementedException();
        }
    }

}
