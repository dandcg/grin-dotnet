using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Grin.Wallet
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

