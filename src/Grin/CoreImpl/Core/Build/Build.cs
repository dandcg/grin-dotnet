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
using Grin.CoreImpl.Core.Transaction;
using Grin.KeychainImpl;
using Grin.KeychainImpl.Blind;
using Grin.KeychainImpl.ExtKey;
using Secp256k1Proxy.Lib;
using Secp256k1Proxy.Pedersen;
using Serilog;

namespace Grin.CoreImpl.Core.Build
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
        public static (Transaction.Transaction transaction, BlindingFactor blindingFactor) Transaction(Func<Context,Append>[] elems,Keychain keychain)

        {
            var tx = Core.Transaction.Transaction.Empty();
            var sum = BlindSum.New();
            var ctx = new Context(keychain,tx,sum);

            foreach (var elem in elems)
            {
                var append = elem(ctx);
                tx = ctx.Tx = append.Transaction.Clone();
                sum = ctx.Sum = append.Blind;
            }

            var blindSum = ctx.Keychain.Blind_sum(sum);
            var msg = Message.from_slice(TransactionHelper.kernel_sig_msg(tx.Fee, tx.LockHeight));
            var sig = ctx.Keychain.Sign_with_blinding(msg, blindSum);
            tx.ExcessSig = sig.serialize_der(ctx.Keychain.Secp);

            return (tx, blindSum);
        }

        /// Sets an initial transaction to add to when building a new transaction.
        public static Append initial_tx(this Context build, Transaction.Transaction tx)
        {
            return new Append(tx.Clone(), build.Sum);
        }

        /// Sets a known excess value on the transaction being built. Usually used in
        /// combination with the initial_tx function when a new transaction is built
        /// by adding to a pre-existing one.
        public static Append with_excess(this Context build, BlindingFactor excess)
        {
            return new Append(build.Tx, build.Sum.add_blinding_factor(excess.Clone()));
        }

        /// Adds an output with the provided value and key identifier from the
        /// keychain.
        public static Append Output(this Context build, ulong value, Identifier keyId)
        {
            var commit = build.Keychain.Commit(value, keyId);
            var switchCommit = build.Keychain.Switch_commit(keyId);
            var switchCommitHash = SwitchCommitHash.From_switch_commit(switchCommit);
            Log.Verbose(
                "Builder - Pedersen Commit is: {commit}, Switch Commit is: {switch_commit}",
                commit,
                switchCommit
            );
            Log.Verbose(
                "Builder - Switch Commit Hash is: {switch_commit_hash}",
                switchCommitHash
            );
            var msg = ProofMessage.Empty();
            var rproof = build.Keychain.Range_proof(value, keyId, commit, msg);

            return new Append(
                build.Tx.with_output(new Output
                {
                    Features = OutputFeatures.DefaultOutput,
                    Commit = commit,
                    SwitchCommitHash = switchCommitHash,
                    Proof = rproof
                })
                ,
                build.Sum.add_key_id(keyId.Clone()));
        }

        /// Adds an input with the provided value and blinding key to the transaction
        /// being built.
        public static Append Input(this Context build, ulong value, Identifier keyId)
        {
          var commit = build.Keychain.Commit(value, keyId);
            return new Append(build.Tx.with_input(new Input(commit)), build.Sum.sub_key_id(keyId.Clone()));
        }

        /// Sets the fee on the transaction being built.
        public static Append with_fee(this Context build, ulong fee)
        {
            return new Append(build.Tx.with_fee(fee),build.Sum);
        }

        /// Sets the lock_height on the transaction being built.
        public static Append with_lock_height(this Context build, ulong lockHeight)
        {
            return new Append(build.Tx.with_lock_height(lockHeight), build.Sum);
        
        }


    }
}