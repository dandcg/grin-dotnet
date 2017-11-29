using System;

namespace Grin.CoreImpl.Core.Block
{
    public sealed class BlockErrorException : ApplicationException
    {
        public BlockError Error { get; }

        public BlockErrorException(BlockError error) : base(error.ToString())
        {
            Error = error;
        }


        public BlockErrorException(BlockError error, string message ) : base(error.ToString() + ": " + message)
        {
            Error = error;
        }

        public BlockErrorException(BlockError error, Exception innerException) : base(error.ToString(), innerException)
        {
            Error = error;
        }


        public BlockErrorException(BlockError error, string message  ,Exception innerException) : base(error.ToString() + ": " + message, innerException)
        {
            Error = error;
        }

    }
}

