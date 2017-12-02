using System;
using System.Runtime.InteropServices;
using Secp256k1Proxy.Helpers;

namespace Secp256k1Proxy.Ffi
{
    public class Proxy
    {

        private const string LibName = "Libs/x64/secp256k1.dll";

        //[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        //public delegate int secp256k1_nonce_function( byte[] nonce32, byte[] msg32, byte[] key32, byte[] algo16, uint attempt, out IntPtr data );
       

        [DllImport(LibName)]
        public static extern IntPtr secp256k1_nonce_function_rfc6979();

        [DllImport(LibName)]
        public static extern IntPtr secp256k1_nonce_function_default();

        // Contexts
        [DllImport(LibName)]
        public static extern IntPtr secp256k1_context_create(uint flags);

        [DllImport(LibName)]
        public static extern IntPtr secp256k1_context_clone(IntPtr cx);

        [DllImport(LibName)]
        public static extern void secp256k1_context_destroy(IntPtr cx);

        [DllImport(LibName)]
        public static extern int secp256k1_context_randomize(IntPtr cx, byte[] seed32);

        // TODO secp256k1_context_set_illegal_callback
        // TODO secp256k1_context_set_error_callback
        // (Actually, I don't really want these exposed; if either of these
        // are ever triggered it indicates a bug in rust-secp256k1, since
        // one goal is to use Rust's type system to eliminate all possible
        // bad inputs.)

        // Pubkeys

        [DllImport(LibName)]
        // pub fn secp256k1_ec_pubkey_parse(cx: *const Context, pk: *mut PublicKey, input: *const c_uchar, in_len: size_t) -> c_int;
        public static extern int secp256k1_ec_pubkey_parse(IntPtr ctx, byte[] pubkey, byte[] seckey, int length);
    
        [DllImport(LibName)]
        // pub fn secp256k1_ec_pubkey_serialize(cx: *const Context, output: *const c_uchar, out_len: *mut size_t, pk: *const PublicKey, compressed: c_uint) -> c_int;
        public static extern int secp256k1_ec_pubkey_serialize(IntPtr ctx, byte[] ret, [In, Out] ref long retLength, [In] byte[] pubkey, uint compressed);

        // [ Signatures ]

        [DllImport(LibName)]
        // pub fn secp256k1_ecdsa_signature_parse_compact(cx: *const Context, sig: *mut Signature, input64: *const c_uchar) -> c_int;
        public static extern int secp256k1_ecdsa_signature_parse_compact(IntPtr secpCtx, byte[] ret, byte[] data);
        
        [DllImport(LibName)]
        // pub fn secp256k1_ecdsa_recoverable_signature_parse_compact(cx: *const Context, sig: *mut RecoverableSignature, input64: *const c_uchar, recid: c_int) -> c_int;
        public static extern int secp256k1_ecdsa_recoverable_signature_parse_compact(IntPtr secpCtx, byte[] ret, byte[] data, int recidValue);

        [DllImport(LibName)]
        // pub fn ecdsa_signature_parse_der_lax(cx: *const Context, sig: *mut Signature, input: *const c_uchar, in_len: size_t) -> c_int;
        public static extern int ecdsa_signature_parse_der_lax(IntPtr secpCtx, byte[] ret, byte[] data, int dataLength);

        [DllImport(LibName)]
        // pub fn secp256k1_ecdsa_signature_serialize_der(cx: *const Context, output: *const c_uchar, out_len: *mut size_t, sig: *const Signature) -> c_int;
        public static extern int secp256k1_ecdsa_signature_serialize_der(IntPtr secpCtx, byte[] ret, ref long retLen, byte[] value);
        
        [DllImport(LibName)]
        // pub fn secp256k1_ecdsa_signature_serialize_compact(cx: *const Context, output64: *const c_uchar, sig: *const Signature) -> c_int;
        public static extern int secp256k1_ecdsa_signature_serialize_compact(IntPtr secpCtx, byte[] ret, byte[] value);

        [DllImport(LibName)]
        // pub fn secp256k1_ecdsa_signature_parse_der(cx: *const Context, sig: *mut Signature, input: *const c_uchar, in_len: size_t)  -> c_int;
        public static extern int secp256k1_ecdsa_signature_parse_der(IntPtr ctx, byte[] ret, byte[] data, int dataLength);
        
        [DllImport(LibName)]
        // pub fn secp256k1_ecdsa_recoverable_signature_serialize_compact(cx: *const Context, output64: *const c_uchar, recid: *mut c_int, sig: *const RecoverableSignature) -> c_int;
        public static extern int secp256k1_ecdsa_recoverable_signature_serialize_compact(IntPtr secpCtx, byte[] ret, ref int recid, byte[] value);
        
        [DllImport(LibName)]
        // pub fn secp256k1_ecdsa_recoverable_signature_convert(cx: *const Context, sig: *mut Signature, input: *const RecoverableSignature) -> c_int;
        public static extern int secp256k1_ecdsa_recoverable_signature_convert(IntPtr secpCtx, byte[] ret, byte[] value);

        [DllImport(LibName)]
        // pub fn secp256k1_ecdsa_signature_normalize(cx: *const Context, out_sig: *mut Signature, in_sig: *const Signature) -> c_int;
        public static extern void secp256k1_ecdsa_signature_normalize(IntPtr secpCtx, byte[] value, byte[] bytes);

        // [ ECDSA ]

        [DllImport(LibName)]
        // pub fn secp256k1_ecdsa_verify(cx: *const Context, sig: *const Signature, msg32: *const c_uchar, pk: *const PublicKey) -> c_int;
        public static extern int secp256k1_ecdsa_verify(IntPtr ctx, byte[] sigValue, byte[] msgValue, byte[] pkValue);

        [DllImport(LibName)]
        // pub fn secp256k1_ecdsa_sign(cx: *const Context, sig: *mut Signature, msg32: *const c_uchar, sk: *const c_uchar, noncefn: NonceFn, noncedata: *const c_void) -> c_int;
        public static extern int secp256k1_ecdsa_sign(IntPtr ctx, byte[] ret, byte[] msgValue, byte[] skValue, IntPtr nonce, IntPtr outprm);

        [DllImport(LibName)]
        // pub fn secp256k1_ecdsa_sign_recoverable(cx: *const Context, sig: *mut RecoverableSignature, msg32: *const c_uchar, sk: *const c_uchar, noncefn: NonceFn, noncedata: *const c_void) -> c_int;
        public static extern int secp256k1_ecdsa_sign_recoverable(IntPtr ctx, byte[] ret, byte[] msgValue, byte[] skValue, IntPtr nonce, IntPtr zero);
        
        [DllImport(LibName)]
        // pub fn secp256k1_ecdsa_recover(cx: *const Context, pk: *mut PublicKey, sig: *const RecoverableSignature, msg32: *const c_uchar) -> c_int;
        public static extern int secp256k1_ecdsa_recover(IntPtr ctx, byte[] pkBytes, byte[] sigValue, byte[] msgValue);
 
        // [ EC ]

        [DllImport(LibName)]
        // pub fn secp256k1_ec_seckey_verify(cx: *const Context, sk: *const c_uchar) -> c_int;
        public static extern int secp256k1_ec_seckey_verify(IntPtr ctx, byte[] seckey);

        [DllImport(LibName)]
        // pub fn secp256k1_ec_pubkey_create(cx: *const Context, pk: *mut PublicKey, sk: *const c_uchar) -> c_int;
        public static extern int secp256k1_ec_pubkey_create(IntPtr ctx, byte[] pkBytes, byte[] sk);

        ////TODO secp256k1_ec_privkey_export
        ////TODO secp256k1_ec_privkey_import

        [DllImport(LibName)]
        // pub fn secp256k1_ec_privkey_tweak_add(cx: *const Context, sk: *mut c_uchar, tweak: *const c_uchar) -> c_int;
        public static extern int secp256k1_ec_privkey_tweak_add(IntPtr secpCtx, byte[] value, byte[] otherValue);


        [DllImport(LibName)]
        // pub fn secp256k1_ec_pubkey_tweak_add(cx: *const Context, pk: *mut PublicKey, tweak: *const c_uchar) -> c_int;
        public static extern int secp256k1_ec_pubkey_tweak_add(IntPtr secpCtx, byte[] value, byte[] otherValue);
 
        [DllImport(LibName)]
        // pub fn secp256k1_ec_privkey_tweak_mul(cx: *const Context, sk: *mut c_uchar, tweak: *const c_uchar) -> c_int;
        public static extern int secp256k1_ec_privkey_tweak_mul(IntPtr secpCtx, byte[] value, byte[] otherValue);

        [DllImport(LibName)]
        // pub fn secp256k1_ec_pubkey_tweak_mul(cx: *const Context, pk: *mut PublicKey, tweak: *const c_uchar) -> c_int;
        public static extern int secp256k1_ec_pubkey_tweak_mul(IntPtr secpCtx, byte[] value, byte[] otherValue);



        //    pub fn secp256k1_ec_pubkey_combine(cx: *const Context,
        //                                       out: *mut PublicKey,
        //                                       ins: *const *const PublicKey,
        //                                       n: c_int)
        //                                       -> c_int;

        [DllImport(LibName)]
        // pub fn secp256k1_ecdh(cx: *const Context, out: *mut SharedSecret, point: *const PublicKey, scalar: *const c_uchar) -> c_int;
        public static extern int secp256k1_ecdh(IntPtr secpCtx, byte[] sharedSecretBytes, byte[] pointValue, byte[] scalarValue);


        [DllImport(LibName)]
        //    // Generates a switch commitment: *commit = blind * J
        //    // The commitment is 33 bytes, the blinding factor is 32 bytes.
        //    pub fn secp256k1_switch_commit(ctx: *const Context,
        //                                   commit: *mut c_uchar,
        //                                   blind: *const c_uchar,
        //                                   gen: *const c_uchar)
        //                                   -> c_int;
        public static extern int secp256k1_switch_commit(IntPtr selfCtx, byte[] commit, byte[] blind, byte[] generatorJ);


        [DllImport(LibName)]
        //	// Generates a pedersen commitment: *commit = blind * G + value * G2.
        //	// The commitment is 33 bytes, the blinding factor is 32 bytes.
        //	pub fn secp256k1_pedersen_commit(
        //        ctx: *const Context,
        //        commit: *mut c_uchar,
        //        blind: *const c_uchar,
        //        value: uint64_t,
        //		gen: *const c_uchar
        //	) -> c_int;

        public static extern int secp256k1_pedersen_commit(IntPtr selfCtx, byte[] commit, byte[] blind, ulong value, byte[] generatorH);

        [DllImport(LibName)]
        //	// Takes a list of n pointers to 32 byte blinding values, the first negs
        //	// of which are treated with positive sign and the rest negative, then
        //	// calculates an additional blinding value that adds to zero.
        //	pub fn secp256k1_pedersen_blind_sum(
        //        ctx: *const Context,
        //        blind_out: *const c_uchar,
        //        blinds: *const *const c_uchar,
        //        n: size_t,
        //		npositive: size_t
        //	) -> c_int;
        public static extern int secp256k1_pedersen_blind_sum(IntPtr selfCtx, byte[] ret, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(JaggedArrayMarshaler))] byte[][] all, int allLength,int positiveLength);


        [DllImport(LibName)]
        //	// Takes two list of 33-byte commitments and sums the first set, subtracts
        //	// the second and returns the resulting commitment.
        //	pub fn secp256k1_pedersen_commit_sum(
        //        ctx: *const Context,
        //        commit_out: *const c_uchar,
        //        commits: *const *const c_uchar,
        //        pcnt: size_t,
        //		ncommits: *const *const c_uchar,
        //        ncnt: size_t
        //	) -> c_int;
        public static extern int secp256k1_pedersen_commit_sum(IntPtr selfCtx, byte[] retValue, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(JaggedArrayMarshaler))] byte[][] pos, int posLength, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(JaggedArrayMarshaler))] byte[][] neg, int negLength);

        [DllImport(LibName)]
        //	// Takes two list of 33-byte commitments and sums the first set and
        //	// subtracts the second and verifies that they sum to 0.
        //	pub fn secp256k1_pedersen_verify_tally(ctx: *const Context,
        //        commits: *const *const c_uchar,
        //        pcnt: size_t,
        //		ncommits: *const *const c_uchar,
        //        ncnt: size_t
        //	) -> c_int;
        public static extern int secp256k1_pedersen_verify_tally(IntPtr selfCtx, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(JaggedArrayMarshaler))] byte[][] pos,  int  posLength, [MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(JaggedArrayMarshaler))] byte[][] neg,int negLength);


        [DllImport(LibName)]
        //	pub fn secp256k1_rangeproof_info(
        //        ctx: *const Context,
        //        exp: *mut c_int,
        //        mantissa: *mut c_int,
        //        min_value: *mut uint64_t,
        //        max_value: *mut uint64_t,
        //        proof: *const c_uchar,
        //        plen: size_t,
        //		extra_commit: *const c_uchar,
        //        extra_commit_len: size_t,
        //		gen: *const c_uchar
        //	) -> c_int;
        public static extern int secp256k1_rangeproof_info(IntPtr selfCtx, ref int exp, ref int mantissa, ref ulong min,
            ref ulong max, byte[] proofProof, int proofPlen, byte[] extraCommit, int i, byte[] generatorH);

        [DllImport(LibName)]
        //	pub fn secp256k1_rangeproof_rewind(
        //        ctx: *const Context,
        //        blind_out: *mut c_uchar,
        //        value_out: *mut uint64_t,
        //        message_out: *mut c_uchar,
        //        outlen: *mut size_t,
        //        nonce: *const c_uchar,
        //        min_value: *mut uint64_t,
        //        max_value: *mut uint64_t,
        //        commit: *const c_uchar,
        //        proof: *const c_uchar,
        //        plen: size_t,
        //		extra_commit: *const c_uchar,
        //        extra_commit_len: size_t,
        //		gen: *const c_uchar
        //	) -> c_int;
        public static extern int secp256k1_rangeproof_rewind(IntPtr selfCtx, byte[] blindOut, ref ulong value,
            byte[] message, ref ulong mlen, byte[] nonce, ref ulong min, ref ulong max, byte[] commit,
            byte[] proofProof, int proofPlen, byte[] extraCommit, int extraCommitLen, byte[] generatorH);
        

        [DllImport(LibName)]
        //	pub fn secp256k1_rangeproof_verify(
        //        ctx: *const Context,
        //        min_value: &mut uint64_t,
        //        max_value: &mut uint64_t,
        //        commit: *const c_uchar,
        //        proof: *const c_uchar,
        //        plen: size_t,
        //		extra_commit: *const c_uchar,
        //        extra_commit_len: size_t,
        //		gen: *const c_uchar
        //	) -> c_int;
        public static extern int secp256k1_rangeproof_verify(IntPtr ctx, ref ulong minValue, ref ulong maxValue,
            byte[] commit, byte[] proof, int plen, byte[] extraCommit, int extraCommitLen, byte[] gen);
      

        [DllImport(LibName)]
        //	pub fn secp256k1_rangeproof_sign(
        //        ctx: *const Context,
        //        proof: *mut c_uchar,
        //        plen: *mut size_t,
        //        min_value: uint64_t,
        //		commit: *const c_uchar,
        //        blind: *const c_uchar,
        //        nonce: *const c_uchar,
        //        exp: c_int,
        //		min_bits: c_int,
        //		value: uint64_t,
        //		message: *const c_uchar,
        //        msg_len: size_t,
        //		extra_commit: *const c_uchar,
        //        extra_commit_len: size_t,
        //		gen: *const c_uchar
        //	) -> c_int;
        public static extern int secp256k1_rangeproof_sign(IntPtr selfCtx,  IntPtr proof, ref int plen, ulong minValue,
            byte[] commit, byte[] blind, byte[] nonce, int exp, int minBits, ulong value, byte[] message,
            int msgLen, byte[] extraCommit, int extraCommitLen, byte[] gen);


 
    }
}
