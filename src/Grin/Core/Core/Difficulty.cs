using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Grin.Core.Model
{
    public class Difficulty
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

        public void Clone()
        {
            throw new NotImplementedException();
        }
    }
}
