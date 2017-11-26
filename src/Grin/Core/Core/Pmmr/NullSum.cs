using System;

namespace Grin.Core.Core
{
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