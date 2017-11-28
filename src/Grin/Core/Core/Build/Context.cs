using Grin.Keychain.Blind;

namespace Grin.Core.Core.Build
{
    /// Context information available to transaction combinators.
    public class Context
    {
        public Context(Keychain.KeychainImpl.Keychain keychain, Transaction.Transaction tx, BlindSum sum)
        {
            Keychain = keychain;
            Tx = tx;
            Sum = sum;
        }

        public Keychain.KeychainImpl.Keychain Keychain { get; }
        public Transaction.Transaction Tx { get; set; }
        public BlindSum Sum { get; set; }
    }
}