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
using Grin.Keychain;
using Secp256k1Proxy;
using Serilog;

namespace Grin.Core.Core
{
    public static class Build
    {
        /// Builds a new transaction by combining all the combinators provided in a
        /// Vector. Transactions can either be built "from scratch" with a list of
        /// inputs or outputs or from a pre-existing transaction that gets added to.
        /// 
        /// Example:
        /// let (tx1, sum) = build::transaction(vec![input_rand(4), output_rand(1),
        /// with_fee(1)], keychain).unwrap();
        /// let (tx2, _) = build::transaction(vec![initial_tx(tx1), with_excess(sum),
        /// output_rand(2)], keychain).unwrap();
        public static (Transaction transaction, BlindingFactor blindingFactor) transaction(Func<Context,Append>[] elems,Keychain.Keychain keychain)

        {
            var tx = Transaction.Empty();
            var sum = BlindSum.New();
            var ctx = new Context(keychain,tx,sum);

            foreach (var elem in elems)
            {
                var append = elem(ctx);
                tx = ctx.Tx = append.Transaction.clone();
                sum = ctx.Sum = append.Blind;
            }

            var blind_sum = ctx.Keychain.Blind_sum(sum);
            var msg = Message.from_slice(TransactionHelper.kernel_sig_msg(tx.fee, tx.lock_height));
            var sig = ctx.Keychain.Sign_with_blinding(msg, blind_sum);
            tx.excess_sig = sig.serialize_der(ctx.Keychain.Secp);

            return (tx, blind_sum);
        }

        /// Sets an initial transaction to add to when building a new transaction.
        public static Append initial_tx(this Context build, Transaction tx)
        {
            return new Append(tx.clone(), build.Sum);
        }

        /// Sets a known excess value on the transaction being built. Usually used in
        /// combination with the initial_tx function when a new transaction is built
        /// by adding to a pre-existing one.
        public static Append with_excess(this Context build, BlindingFactor excess)
        {
            return new Append(build.Tx, build.Sum.add_blinding_factor(excess.clone()));
        }

        /// Adds an output with the provided value and key identifier from the
        /// keychain.
        public static Append output(this Context build, ulong value, Identifier key_id)
        {
            var commit = build.Keychain.Commit(value, key_id);
            var switch_commit = build.Keychain.Switch_commit(key_id);
            var switch_commit_hash = SwitchCommitHash.From_switch_commit(switch_commit);
            Log.Verbose(
                "Builder - Pedersen Commit is: {commit}, Switch Commit is: {switch_commit}",
                commit,
                switch_commit
            );
            Log.Verbose(
                "Builder - Switch Commit Hash is: {switch_commit_hash}",
                switch_commit_hash
            );
            var msg = ProofMessage.empty();
            var rproof = build.Keychain.Range_proof(value, key_id, commit, msg);

            return new Append(
                build.Tx.with_output(new Output
                {
                    features = OutputFeatures.DEFAULT_OUTPUT,
                    commit = commit,
                    switch_commit_hash = switch_commit_hash,
                    proof = rproof
                })
                ,
                build.Sum.add_key_id(key_id.Clone()));
        }

        /// Adds an input with the provided value and blinding key to the transaction
        /// being built.
        public static Append input(this Context build, ulong value, Identifier key_id)
        {
          var commit = build.Keychain.Commit(value, key_id);
            return new Append(build.Tx.with_input(new Input(commit)), build.Sum.sub_key_id(key_id.Clone()));
        }

        /// Sets the fee on the transaction being built.
        public static Append with_fee(this Context build, ulong fee)
        {
            return new Append(build.Tx.with_fee(fee),build.Sum);
        }

        /// Sets the lock_height on the transaction being built.
        public static Append with_lock_height(this Context build, UInt64 lock_height)
        {
            return new Append(build.Tx.with_lock_height(lock_height), build.Sum);
        
        }


    }
}