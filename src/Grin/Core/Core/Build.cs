//! Utility functions to build Grin transactions. Handles the blinding of
//! inputs and outputs, maintaining the sum of blinding factors, producing
//! the excess signature, etc.
//!
//! Each building function is a combinator that produces a function taking
//! a transaction a sum of blinding factors, to return another transaction
//! and sum. Combinators can then be chained and executed using the
//! _transaction_ function.
//!
//! Example:
//! build::transaction(vec![input_rand(75), output_rand(42), output_rand(32),
//!   with_fee(1)])

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grin.Keychain;
using Secp256k1Proxy;

namespace Grin.Core.Core
{
    /// Context information available to transaction combinators.
    public class Context
    {

        public Context(Keychain.Keychain keychain)
        {
            Keychain = keychain;
        }
      public Keychain.Keychain Keychain { get; }
    }

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




    public class Build
    {

        /// Builds a new transaction by combining all the combinators provided in a
        /// Vector. Transactions can either be built "from scratch" with a list of
        /// inputs or outputs or from a pre-existing transaction that gets added to.
        ///
        /// Example:
        /// let (tx1, sum) = build::transaction(vec![input_rand(4), output_rand(1),
        ///   with_fee(1)], keychain).unwrap();
        /// let (tx2, _) = build::transaction(vec![initial_tx(tx1), with_excess(sum),
        ///   output_rand(2)], keychain).unwrap();
        ///
        public static (Transaction, BlindingFactor) transaction(
         Append[] elems,
         Keychain.Keychain keychain
        )

        {
          var ctx =new  Context (keychain);

            //let(mut tx, sum) = elems.iter().fold(
            //    (Transaction::empty(), BlindSum::new()),
            //    |acc, elem| elem(&mut ctx, acc),
            //    );

            Transaction tx= Transaction.Empty();
            BlindSum sum = BlindSum.New() ;
            var blind_sum = ctx.Keychain.Blind_sum(sum);
            var msg = Message.from_slice(TransactionHelper.kernel_sig_msg(tx.fee, tx.lock_height)) ;
            var sig = ctx.Keychain.Sign_with_blinding(msg, blind_sum) ;
            tx.excess_sig = sig.serialize_der(ctx.Keychain.Secp);

            return (tx, blind_sum);
        }


    }
}
