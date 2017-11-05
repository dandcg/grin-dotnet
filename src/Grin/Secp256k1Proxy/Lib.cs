using System;
using System.Security.Cryptography;

namespace Grin.Secp256k1Proxy
{
    public class RecoveryId
    {
        public int Value { get; }

        private RecoveryId(int value)
        {
            Value = value;
        }

        public static RecoveryId from_i32(int id)
        {
            switch (id)
            {
                case 0:
                case 1:
                case 2:
                case 3:

                    return new RecoveryId(id);

                default:

                    throw new Exception("InvalidRecoveryId");
            }
        }
    }


    public class Signiture
    {
        public byte[] Value { get; }

        private Signiture(byte[] value)
        {
            Value = value;
        }


        /// Converts a DER-encoded byte slice to a signature
        public static Signiture from_der(Secp256k1 secp, byte[] data)

        {
            var ret = new byte[64];


            if (Proxy.secp256k1_ecdsa_signature_parse_der(secp.Ctx, ret, data, data.Length) == 1)
                return new Signiture(ret);
            throw new Exception("InvalidSignature");
        }

        public static Signiture From(byte[] ret)
        {
            return new Signiture(ret);
        }

        /// Converts a 64-byte compact-encoded byte slice to a signature
        public static Signiture from_compact(Secp256k1 secp, byte[] data)
        {

            var ret = new byte[64];


            if (data.Length != 64)
            {
                throw new Exception("InvalidSignature");
            }


            if (Proxy.secp256k1_ecdsa_signature_parse_compact(secp.Ctx, ret, data) == 1)
            {
                return new Signiture(ret);
            }
            else
            {
                throw new Exception("InvalidSignature");
            }

        }

        /// Converts a "lax DER"-encoded byte slice to a signature. This is basically
        /// only useful for validating signatures in the Bitcoin blockchain from before
        /// 2016. It should never be used in new applications. This library does not
        /// support serializing to this "format"
        public static Signiture from_der_lax(Secp256k1 secp, byte[] data)
        {

            var ret = new byte[64];
            if (Proxy.ecdsa_signature_parse_der_lax(secp.Ctx, ret, data, data.Length) == 1)
            {
                return new Signiture(ret);
            }
            else
            {
                throw new Exception("InvalidSignature");

            }
        }

        /// Normalizes a signature to a "low S" form. In ECDSA, signatures are
        /// of the form (r, s) where r and s are numbers lying in some finite
        /// field. The verification equation will pass for (r, s) iff it passes
        /// for (r, -s), so it is possible to ``modify'' signatures in transit
        /// by flipping the sign of s. This does not constitute a forgery since
        /// the signed message still cannot be changed, but for some applications,
        /// changing even the signature itself can be a problem. Such applications
        /// require a "strong signature". It is believed that ECDSA is a strong
        /// signature except for this ambiguity in the sign of s, so to accomodate
        /// these applications libsecp256k1 will only accept signatures for which
        /// s is in the lower half of the field range. This eliminates the
        /// ambiguity.
        ///
        /// However, for some systems, signatures with high s-values are considered
        /// valid. (For example, parsing the historic Bitcoin blockchain requires
        /// this.) For these applications we provide this normalization function,
        /// which ensures that the s value lies in the lower half of its range.
        public void normalize_s(Secp256k1 secp)
        {

            // Ignore return value, which indicates whether the sig
            // was already normalized. We don't care.
            Proxy.secp256k1_ecdsa_signature_normalize(secp.Ctx, Value, Value);

        }

    }


    public class RecoverableSigniture
    {
        public byte[] Value { get; }

        private RecoverableSigniture(byte[] value)
        {
            Value = value;
        }


        public static RecoverableSigniture From(byte[] ret)
        {
            return new RecoverableSigniture(ret);
        }
    }


    public class Message
    {
        public byte[] Value { get; }

        private Message(byte[] value)
        {
            Value = value;
        }

        public static Message from_slice(byte[] data)
        {
            switch (data.Length)
            {
                case Constants.MESSAGE_SIZE:

                    var ret = new byte[Constants.MESSAGE_SIZE];
                    Array.Copy(data, ret, Constants.MESSAGE_SIZE);
                    return new Message(data);


                default:

                    throw new Exception("InvalidMessage");
            }
        }
    }


    /// Flags used to determine the capabilities of a `Secp256k1` object;
    /// the more capabilities, the more expensive it is to New.
    public enum ContextFlag
    {
        /// Can neither sign nor verify signatures (cheapest to New, useful
        /// for cases not involving signatures, such as creating keys from slices)
        None,

        /// Can sign but not verify signatures
        SignOnly,

        /// Can verify but not New signatures
        VerifyOnly,

        /// Can verify and New signatures
        Full,

        /// Can do all of the above plus pedersen commitments
        Commit
    }


    // ReSharper disable once InconsistentNaming
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
            var seed = KeyUtils.random_32_bytes(rng);

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

            var ret = new byte[64];

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
            return PublicKey.from(pkBytes);
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

        public void sign(SecretKey sk)
        {
            throw new NotImplementedException();
        }
    }
}