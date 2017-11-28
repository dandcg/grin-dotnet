using System;
using System.Runtime.Serialization;

namespace Grin.Wallet.Types
{
    public sealed class WalletErrorException : ApplicationException
    {

        public WalletErrorException(WalletError error) : base(error.ToString())
        {

        }


        public WalletErrorException(WalletError error, string message ) : base(error.ToString() + ": " + message)
        {
            
        }

        public WalletErrorException(WalletError error, Exception innerException) : base(error.ToString(), innerException)
        {
        

        }


        public WalletErrorException(WalletError error, string message  ,Exception innerException) : base(error.ToString() + ": " + message, innerException)
        {


        }

    }
}

