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

    public class FeeDisputeException : ApplicationException
    {
        private ulong sender_fee;
        private ulong recipient_fee;

        public FeeDisputeException(ulong sender_fee, ulong recipient_fee)
        {
            this.sender_fee = sender_fee;
            this.recipient_fee = recipient_fee;
        }
    }
}

