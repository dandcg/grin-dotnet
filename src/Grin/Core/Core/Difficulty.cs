using System;

namespace Grin.Core.Core
{
    public class Difficulty:IWriteable,IReadable
    {
        public UInt64 num { get; set; }

        public static Difficulty From_num(ulong minimumDifficulty)
        {
            throw new NotImplementedException();
        }

        public static Difficulty Zero()
        {
            throw new NotImplementedException();
        }

        public Difficulty Clone()
        {
            throw new NotImplementedException();
        }

        public void write(IWriter writer)
        {
            throw new NotImplementedException();
        }

        public void read(IReader reader)
        {
            throw new NotImplementedException();
        }

        public static Difficulty readnew(IReader reader)
        {
            throw new NotImplementedException();
        }



        public static Difficulty From_hash(Hash hash)
        {
            throw new NotImplementedException();
        }
    }
}
