using System;
using Secp256k1Proxy.Ffi;

namespace Secp256k1Proxy.Lib
{
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


        public byte[] serialize_der(Secp256k1 secp)
        {
            var ret = new byte[72];
            Int64 ret_len = ret.Length;

            var err = Proxy.secp256k1_ecdsa_signature_serialize_der(secp.Ctx, ret,ref ret_len,Value);

            if (err == 1)
            {

                Array.Resize(ref ret,(int) ret_len);

                return ret;

            }
            throw new Exception("Should never happen!");

        }

        public byte[] serialize_compact(Secp256k1 secp)
        {
            var ret = new byte[64];
      
            var err = Proxy.secp256k1_ecdsa_signature_serialize_compact(secp.Ctx, ret,Value);
            if (err == 1)
            {
                return ret;
            }
            throw new Exception("Should never happen!");
        }
    }
}