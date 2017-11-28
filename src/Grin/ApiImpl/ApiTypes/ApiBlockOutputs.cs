using System;

namespace Grin.Api.ApiTypes
{
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
}