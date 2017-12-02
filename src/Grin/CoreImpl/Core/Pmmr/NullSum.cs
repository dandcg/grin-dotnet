using System;
using Grin.CoreImpl.Ser;

namespace Grin.CoreImpl.Core.Pmmr
{
    public class NullSum : ISummable, IWriteable, IReadable
    {
        public void Write(IWriter writer)
        {
            throw new NotImplementedException();
        }

        public void Read(IReader reader)
        {
            throw new NotImplementedException();
        }

        public Sum Sum()
        {
            throw new NotImplementedException();
        }

        public uint sum_len()
        {
            throw new NotImplementedException();
        }
    }
}