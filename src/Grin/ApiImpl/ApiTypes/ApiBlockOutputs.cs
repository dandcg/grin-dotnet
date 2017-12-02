using System;

namespace Grin.ApiImpl.ApiTypes
{
    public class ApiBlockOutputs : ICloneable
    {
        /// The block header
        public ApiBlockHeaderInfo Header { get; set; }

        /// A printable version of the outputs
        public ApiOutputSwitch[] Outputs { get; set; }

        public object Clone()
        {
            throw new NotImplementedException();
        }
    }
}