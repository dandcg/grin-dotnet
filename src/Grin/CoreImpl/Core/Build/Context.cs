using Grin.KeychainImpl;
using Grin.KeychainImpl.Blind;

namespace Grin.CoreImpl.Core.Build
{
    /// Context information available to transaction combinators.
    public class Context
    {
        public Context(Keychain keychain, Transaction.Transaction tx, BlindSum sum)
        {
            Keychain = keychain;
            Tx = tx;
            Sum = sum;
        }

        public Keychain Keychain { get; }
        public Transaction.Transaction Tx { get; set; }
        public BlindSum Sum { get; set; }
    }
}