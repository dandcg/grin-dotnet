using Grin.KeychainImpl.Blind;

namespace Grin.CoreImpl.Core.Build
{
    /// Function type returned by the transaction combinators. Transforms a
    /// (Transaction, BlindSum) pair into another, provided some context.
    public class Append
    {
        public Transaction.Transaction Transaction { get; }
        public BlindSum Blind { get; }

        public Append(Transaction.Transaction transaction, BlindSum blind)
        {
            Transaction = transaction;
            Blind = blind;
        }
    }
}