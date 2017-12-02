﻿using System.IO;

namespace Grin.CoreImpl.Ser
{
    public class BinReader : ReaderBase
    {
        private readonly Stream source;

        public BinReader(Stream source)
        {
            this.source = source;
        }


        public override byte[] read_fixed_bytes(uint length)
        {
            var bs = new byte[length];
            source.Read(bs, 0, (int) length);
            //Trace.Write($"{bs.Length},");
            return bs;
        }
    }
}