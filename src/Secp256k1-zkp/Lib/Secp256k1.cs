using System;
using System.Security.Cryptography;
using Common;
using Secp256k1Proxy.Ffi;
using Secp256k1Proxy.Key;

namespace Secp256k1Proxy.Lib
{
    public class Secp256k1
    {
        public IntPtr Ctx { get; set; }

        public ContextFlag Caps { get; set; }


        private Secp256k1(IntPtr ctx , ContextFlag caps)
        {
            Ctx = ctx;
            Caps = caps;
        }

        /// Creates a new Secp256k1 context
        public static Secp256k1 New()
        {
            return WithCaps(ContextFlag.Full);
        }

        /// Creates a new Secp256k1 context with the specified capabilities
        public static Secp256k1 WithCaps(ContextFlag caps)
        {
            
            var flag = Secp256K1Options.SECP256K1_START_NONE;

            switch (caps)
            {
                case ContextFlag.None:
                    flag = Secp256K1Options.SECP256K1_START_NONE;

                    break;

                case ContextFlag.SignOnly:
                    flag = Secp256K1Options.SECP256K1_START_SIGN;
                    break;

                case ContextFlag.VerifyOnly:
                    flag = Secp256K1Options.SECP256K1_START_VERIFY;
                    break;

                case ContextFlag.Full:
                case ContextFlag.Commit:
                    flag = Secp256K1Options.SECP256K1_START_SIGN | Secp256K1Options.SECP256K1_START_VERIFY;
                    break;
            }

            var ctx = Proxy.secp256k1_context_create((uint) flag);
            return new Secp256k1(ctx,caps);
        }

        /// Creates a new Secp256k1 context with no capabilities (just de/serialization)
        public static Secp256k1 WithoutCaps()
        {
            return WithCaps(ContextFlag.None);
        }

        /// (Re)randomizes the Secp256k1 context for cheap sidechannel resistence;
        /// see comment in libsecp256k1 commit d2275795f by Gregory Maxwell
        public void Randomize(RandomNumberGenerator rng)
        {
            var seed = ByteUtil.get_random_bytes(rng);

            var err = Proxy.secp256k1_context_randomize(Ctx, seed);

            // This function cannot fail; it has an error return for future-proofing.
            // We do not expose this error since it is impossible to hit, and we have
            // precedent for not exposing impossible errors (for example in
            // `PublicKey::from_secret_key` where it is impossble to New an invalid
            // secret key through the API.)
            // However, if this DOES fail, the result is potentially weaker side-channel
            // resistance, which is deadly and undetectable, so we take out the entire
            // thread to be on the safe side.

            if (err != 1)
                throw new Exception("This should never happen!");
        }

        /// Generates a random keypair. Convenience function for `key::SecretKey::new`
        /// and `key::PublicKey::from_secret_key`; call those functions directly for
        /// batch key generation. Requires a signing-capable context.
        public (SecretKey secretKey, PublicKey publicKey) generate_keypair(RandomNumberGenerator rng)
        {
            var sk = SecretKey.New(this, rng);
            var pk = PublicKey.from_secret_key(this,sk);

            return (sk, pk);
        }

        /// Constructs a signature for `msg` using the secret key `sk` and RFC6979 nonce
        /// Requires a signing-capable context.
        public Signiture Sign(Message msg, SecretKey sk)
        {
            if (Caps == ContextFlag.VerifyOnly || Caps == ContextFlag.None)
                throw new Exception("IncapableContext");

            var ret = new byte[64];

            //// We can assume the return value because it's not possible to construct
            //// an invalid signature from a valid `Message` and `SecretKey`
            if (Proxy.secp256k1_ecdsa_sign(Ctx, ret, msg.Value, sk.Value,IntPtr.Zero/* Proxy.secp256k1_nonce_function_rfc6979()*/,IntPtr.Zero) == 1)
            {
                return Signiture.From(ret);
            }
            throw new Exception("This should never happen!");
        }

        /// Constructs a signature for `msg` using the secret key `sk` and RFC6979 nonce
        /// Requires a signing-capable context.
        public RecoverableSigniture sign_recoverable(Message msg, SecretKey sk)

        {
            if (Caps == ContextFlag.VerifyOnly || Caps == ContextFlag.None)
                throw new Exception("IncapableContext");

            var ret = new byte[65];

            // We can assume the return value because it's not possible to construct
            // an invalid signature from a valid `Message` and `SecretKey`
            if (Proxy.secp256k1_ecdsa_sign_recoverable(Ctx, ret, msg.Value, sk.Value, IntPtr.Zero/*Proxy.secp256k1_nonce_function_rfc6979()*/, IntPtr.Zero) == 1)

                return RecoverableSigniture.From(ret);
            throw new Exception("This should never happen!");
        }

        /// Determines the public key for which `sig` is a valid signature for
        /// `msg`. Requires a verify-capable context.
        public PublicKey Recover(Message msg, RecoverableSigniture sig)

        {
            if (Caps == ContextFlag.SignOnly || Caps == ContextFlag.None)
                throw new Exception("IncapableContext");

            var pkBytes = new byte[64];


            if (Proxy.secp256k1_ecdsa_recover(Ctx, pkBytes, sig.Value, msg.Value) != 1)
                throw new Exception("InvalidSignature");
            return PublicKey.From(pkBytes);
        }

        /// Checks that `sig` is a valid ECDSA signature for `msg` using the public
        /// key `pubkey`. Returns `Ok(true)` on success. Note that this function cannot
        /// be used for Bitcoin consensus checking since there may exist signatures
        /// which OpenSSL would verify but not libsecp256k1, or vice-versa. Requires a
        /// verify-capable context.
        public void Verify(Message msg, Signiture sig, PublicKey pk)
        {
            if (Caps == ContextFlag.SignOnly || Caps == ContextFlag.None)
                throw new Exception("IncapableContext");

            if (!pk.is_valid())
                throw new Exception("InvalidPublicKey");

            if (Proxy.secp256k1_ecdsa_verify(Ctx, sig.Value, msg.Value, pk.Value) == 0)
                throw new Exception("IncorrectSignature");
        }


        private bool disposed;


        public void Dispose()

        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }


        protected virtual void Dispose(bool disposeManagedResources)

        {
            if (!disposed)

            {
                if (disposeManagedResources)

                {
                    // dispose managed resources
                }

                // dispose unmanaged resources

                Proxy.secp256k1_context_destroy(Ctx);

                disposed = true;
            }
        }

    }
}