using System;

namespace Grin.Wallet
{
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