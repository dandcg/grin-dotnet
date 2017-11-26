using Grin.Keychain;

namespace Grin.Core.Core
{
    /// Context information available to transaction combinators.
    public class Context
    {
        public Context(Keychain.Keychain keychain, Transaction tx, BlindSum sum)
        {
            Keychain = keychain;
            Tx = tx;
            Sum = sum;
        }

        public Keychain.Keychain Keychain { get; }
        public Transaction Tx { get; set; }
        public BlindSum Sum { get; set; }
    }
}