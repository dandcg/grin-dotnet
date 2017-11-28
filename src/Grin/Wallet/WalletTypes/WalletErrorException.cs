using System;
using System.Runtime.Serialization;

namespace Grin.Wallet.Types
{
    public sealed class WalletErrorException : ApplicationException
    {
 
        public WalletErrorException(WalletError error, params (object key,object value)[] dataItems ) : base(error.ToString())
        {
            foreach (var d in dataItems)
            {
                Data.Add(d.key,d.value);
            }
        }

        public WalletErrorException(WalletError error, Exception innerException, params (object key, object value)[] dataItems) : base(error.ToString(), innerException)
        {
            foreach (var d in dataItems)
            {
                Data.Add(d.key, d.value);
            }

        }

      
    }
}

