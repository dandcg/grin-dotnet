using System;
using System.Runtime.Serialization;

namespace Grin.Wallet.Types
{
    public class WalletDataException : ApplicationException
    {
        public WalletDataException()
        {
        }

        public WalletDataException(string message) : base(message)
        {
        }

        public WalletDataException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected WalletDataException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}

