using System;
using System.Runtime.InteropServices;
using Common;
using Secp256k1Proxy.Ffi;
using Secp256k1Proxy.Key;
using Secp256k1Proxy.Lib;

namespace Secp256k1Proxy.Pedersen
{
    public static class Secp256k1PedersonExtensions
    {
        /// *** This is a temporary work-around. ***
        /// We do not know which of the two possible public keys from the commit to use,
        /// so here we try both of them and succeed if either works.
        /// This is sub-optimal in terms of performance.
        /// I believe apoelstra has a strategy for fixing this in the secp256k1-zkp lib.
        public static void verify_from_commit(this Secp256k1 self, Message msg, Signiture sig, Commitment commit)
        {
            if (self.Caps != ContextFlag.Commit)
                throw new Exception("IncapableContext");

            // If we knew which one we cared about here we would just use it,
            // but for now return both so we can try them both.
            var pubkeys = commit.to_two_pubkeys(self);

            // Attempt to verify with the first public key,
            // if verify fails try the other one.
            // The first will fail on average 50% of the time.

            try
            {
                self.Verify(msg, sig, pubkeys[0]);
            }
            catch (Exception)
            {
                self.Verify(msg, sig, pubkeys[1]);
            }
        }

        /// Creates a switch commitment from a blinding factor.
        public static Commitment switch_commit(this Secp256k1 self, SecretKey blind)
        {
            if (self.Caps != ContextFlag.Commit)
                throw new Exception("IncapableContext");

            var commit = new byte[33];

            Proxy.secp256k1_switch_commit(self.Ctx, commit, blind.Value, Constants.Constants.GENERATOR_J);


            return new Commitment(commit);
        }


        /// Creates a pedersen commitment from a value and a blinding factor
        public static Commitment commit(this Secp256k1 self, ulong value, SecretKey blind)
        {
            if (self.Caps != ContextFlag.Commit) throw new Exception("IncapableContext");
            var commit = new byte [33];


            Proxy.secp256k1_pedersen_commit(
                self.Ctx,
                commit,
                blind.Value,
                value,
                Constants.Constants.GENERATOR_H
            );

            return new Commitment(commit);
        }


        /// Convenience method to Create a pedersen commitment only from a value,
        /// with a zero blinding factor
        public static Commitment commit_value(this Secp256k1 self, ulong value)
        {
            if (self.Caps != ContextFlag.Commit)
                throw new Exception("IncapableContext");

            var commit = new byte[33];
            var zblind = new byte[32];

            Proxy.secp256k1_pedersen_commit(
                self.Ctx,
                commit,
                zblind,
                value,
                Constants.Constants.GENERATOR_H);


            return new Commitment(commit);
        }

        /// Taking vectors of positive and negative commitments as well as an
        /// expected excess, verifies that it all sums to zero.
        public static bool verify_commit_sum(this Secp256k1 self, Commitment[] positive, Commitment[] negative)
        {
            var pos = new byte[positive.Length][];

            for (var i = 0; i < positive.Length; i++)
                pos[i] = positive[i].Value;

            var neg = new byte[negative.Length][];

            for (var i = 0; i < negative.Length; i++)
                neg[i] = negative[i].Value;


            return Proxy.secp256k1_pedersen_verify_tally(
                       self.Ctx,
                       pos,
                       pos.Length,
                       neg,
                       neg.Length
                   ) == 1;
        }

        /// Computes the sum of multiple positive and negative pedersen commitments.
        public static Commitment commit_sum(this Secp256k1 self, Commitment[] positive, Commitment[] negative)
        {
            var pos = new byte[positive.Length][];

            for (var i = 0; i < positive.Length; i++)
                pos[i] = positive[i].Value;

            var neg = new byte[negative.Length][];

            for (var i = 0; i < negative.Length; i++)
                neg[i] = negative[i].Value;

            var ret = Commitment.blank();
            var err = Proxy.secp256k1_pedersen_commit_sum(
                self.Ctx,
                ret.Value,
                pos,
                pos.Length,
                neg,
                neg.Length);

            if (err == 1) return ret;
            throw new Exception("IncorrectCommitSum");
        }


        /// Computes the sum of multiple positive and negative blinding factors.
        public static SecretKey blind_sum(this Secp256k1 self, SecretKey[] positive, SecretKey[] negative)
        {
            //var neg = new byte[negative.Length][];


            var all = new byte[positive.Length + negative.Length][];

            for (var i = 0; i < positive.Length; i++)
                all[i] = positive[i].Value;
            for (var i = 0; i < negative.Length; i++)
                all[positive.Length + i] = negative[i].Value;


            var ret = new byte[32];

            var err = Proxy.secp256k1_pedersen_blind_sum(
                self.Ctx,
                ret,
                all,
                all.Length,
                positive.Length);


            if (err == 1)

                return SecretKey.from_slice(self, ret);
            ;
            // secp256k1 should never return an invalid private
            throw new Exception("This should never happen!");
        }


        public static RangeProof range_proof(this Secp256k1 self, ulong min, ulong value, SecretKey blind,
            Commitment commit, ProofMessage message)
        {
            var retried = false;
            var proof = new byte[Constants.Constants.MAX_PROOF_SIZE];
            var plen = Constants.Constants.MAX_PROOF_SIZE;
            // IntPtr proofPtr= Marshal.AllocHGlobal(proof.Length);

            var proofPtr = Marshal.AllocCoTaskMem(plen);
            Marshal.Copy(proof, 0, proofPtr, proof.Length);
            // Marshal.Copy(proof, 0, proofPtr, proof.Length);


            // use a "known key" as the nonce, specifically the blinding factor
            // of the commitment for which we are generating the range proof
            // so we can later recover the value and the message by unwinding the range proof
            // with the same nonce
            var nonce = blind.clone();

            var extra_commit = ByteUtil.get_bytes(0, 33);

            // TODO - confirm this reworked retry logic works as expected
            // pretty sure the original approach retried on success (so twice in total)
            // and just kept looping forever on error


            do
            {
                var success = Proxy.secp256k1_rangeproof_sign(
                                  self.Ctx,
                                  proofPtr,
                                  ref plen,
                                  min,
                                  commit.Value,
                                  blind.Value,
                                  nonce.Value,
                                  0,
                                  64,
                                  value,
                                  message.Value,
                                  message.Value.Length,
                                  extra_commit,
                                  0,
                                  Constants.Constants.GENERATOR_H) == 1;

                if (success || retried)
                    break;
                retried = true;
            } while (true);

            var proof2 = new byte[plen];


            Marshal.Copy(proofPtr, proof2, 0, plen);
            Marshal.FreeCoTaskMem(proofPtr);


            return new RangeProof(proof2, plen);
        }

        public static ProofRange verify_range_proof(this Secp256k1 self, Commitment commit, RangeProof proof)
        {
            ulong min = 0;
            ulong max = 0;

            var extra_commit = ByteUtil.get_bytes(0, 33);

            var success =
                Proxy.secp256k1_rangeproof_verify(
                    self.Ctx,
                    ref min,
                    ref max,
                    commit.Value,
                    proof.Proof,
                    proof.Plen,
                    extra_commit,
                    0,
                    Constants.Constants.GENERATOR_H
                ) == 1;

            if (success)
                return new ProofRange
                {
                    min = min,
                    max = max
                };
            throw new Exception("InvalidRangeProof");
        }

        public static ProofInfo range_proof_info(this Secp256k1 self, RangeProof proof)
        {
            var exp = 0;
            var mantissa = 0;
            ulong min = 0;
            ulong max = 0;

            var extra_commit = new byte [33];

            var success = Proxy.secp256k1_rangeproof_info(
                              self.Ctx,
                              ref exp,
                              ref mantissa,
                              ref min,
                              ref max,
                              proof.Proof,
                              proof.Plen,
                              extra_commit,
                              0,
                              Constants.Constants.GENERATOR_H
                          ) == 1;

            return new
                ProofInfo
                {
                    success = success,
                    value = 0,
                    message = ProofMessage.empty(),
                    mlen = 0,
                    min = min,
                    max = max,
                    exp = exp,
                    mantissa = mantissa
                };
        }

        public static ProofInfo rewind_range_proof(this Secp256k1 self, Commitment commit, RangeProof proof,
            SecretKey nonce)
        {
            ulong value = 0;
            var blind = new byte[32];

            var message = new byte[Constants.Constants.PROOF_MSG_SIZE];
            var mlen = (UInt64)Constants.Constants.PROOF_MSG_SIZE;
            
            ulong min = 0;
            ulong max = 0;

            var extra_commit = new byte[33];


            var success = Proxy.secp256k1_rangeproof_rewind(
                              self.Ctx,
                              blind,
                              ref value,
                              message,
                              ref mlen,
                              nonce.Value,
                              ref min,
                              ref max,
                              commit.Value,
                              proof.Proof,
                              proof.Plen,
                              extra_commit,
                              0,
                              Constants.Constants.GENERATOR_H
                          ) == 1;


            return new ProofInfo
            {
                success = success,
                value = value,
                message = ProofMessage.from_bytes(message),
                mlen = (int)mlen,
                min = min,
                max = max,
                exp = 0,
                mantissa = 0
            };
        }
    }
}