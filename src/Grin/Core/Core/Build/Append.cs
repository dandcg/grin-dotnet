using Grin.Keychain;

namespace Grin.Core.Core
{
    /// Function type returned by the transaction combinators. Transforms a
    /// (Transaction, BlindSum) pair into another, provided some context.
    public class Append
    {
        public Transaction Transaction { get; }
        public BlindSum Blind { get; }

        public Append(Transaction transaction, BlindSum blind)
        {
            Transaction = transaction;
            Blind = blind;
        }
    }
}