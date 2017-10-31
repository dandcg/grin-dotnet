using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Grin.Secp256k1
{

    public static class ECDSA
    {

        [DllImport("Libs/secp256k1.dll", EntryPoint = "secp256k1_start", CallingConvention = CallingConvention.Cdecl)]
        private static extern void StartCrypto();

        [DllImport("Libs/secp256k1.dll", EntryPoint = "secp256k1_stop", CallingConvention = CallingConvention.Cdecl)]
        private static extern void StopCrypto();

        [DllImport("Libs/secp256k1.dll", EntryPoint = "secp256k1_ecdsa_verify", CallingConvention = CallingConvention.Cdecl)]
        public static extern int VerifySignature(byte[] msg, int msglen, byte[] sig, int siglen, byte[] pubkey, int pubkeylen);

        [DllImport("Libs/secp256k1.dll", EntryPoint = "secp256k1_ecdsa_sign", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SignMessage(byte[] msg, int msglen, byte[] sig, ref int siglen, byte[] seckey, ref int nonce);

        [DllImport("Libs/secp256k1.dll", EntryPoint = "secp256k1_ecdsa_sign_compact", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SignCompact(byte[] msg, int msglen, byte[] sig64, byte[] seckey, ref int nonce, ref int recid);

        [DllImport("Libs/secp256k1.dll", EntryPoint = "secp256k1_ecdsa_recover_compact", CallingConvention = CallingConvention.Cdecl)]
        public static extern int RecoverCompact(byte[] msg, int msglen, byte[] sig64, byte[] pubkey, ref int pubkeylen, int compressed, int recid);





        [DllImport("Libs/secp256k1.dll", EntryPoint = "secp256k1_ec_seckey_verify", CallingConvention = CallingConvention.Cdecl)]
        public static extern int VerifySecretKey(byte[] seckey);

        [DllImport("Libs/secp256k1.dll", EntryPoint = "secp256k1_ec_pubkey_verify", CallingConvention = CallingConvention.Cdecl)]
        public static extern int VerifyPublicKey(byte[] pubkey, int pubkeylen);

        [DllImport("Libs/secp256k1.dll", EntryPoint = "secp256k1_ec_pubkey_create", CallingConvention = CallingConvention.Cdecl)]
        public static extern int PublicKeyFromSecretKey(byte[] pubkey, ref int pubkeylen, byte[] seckey, int compressed);


        [DllImport("Libs/secp256k1.dll", EntryPoint = "secp256k1_ec_pubkey_parse", CallingConvention = CallingConvention.Cdecl)]
        public static extern int PublicKeyParse(byte[] pubkey,  byte[] seckey, int length);




        //[DllImport("secp256k1.dll", EntryPoint = "secp256k1_ec_pubkey_decompress", CallingConvention = CallingConvention.Cdecl)]
        //public static extern int DecompressPublicKey(byte[] pubkey, ref int pubkeylen);

        //[DllImport("secp256k1.dll", EntryPoint = "secp256k1_ecdsa_pubkey_compress", CallingConvention = CallingConvention.Cdecl)]
        //public static extern int CompressPublicKey(byte[] pubkey, ref int pubkeylen);

        [DllImport("Libs/secp256k1.dll", EntryPoint = "secp256k1_ecdsa_privkey_export", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ExportPrivateKey(byte[] seckey, byte[] privkey, ref int privkeylen, int compressed);

        [DllImport("Libs/secp256k1.dll", EntryPoint = "secp256k1_ecdsa_privkey_import", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ImportPrivateKey(byte[] seckey, byte[] privkey, int privkeylen);

        [DllImport("Libs/secp256k1.dll", EntryPoint = "secp256k1_ec_privkey_tweak_add", CallingConvention = CallingConvention.Cdecl)]
        public static extern int PrivateKeyTweakAdd(byte[] seckey, byte[] tweak);

        [DllImport("Libs/secp256k1.dll", EntryPoint = "secp256k1_ec_pubkey_tweak_add", CallingConvention = CallingConvention.Cdecl)]
        public static extern int PublicKeyTweakAdd(byte[] pubkey, int pubkeylen, byte[] tweak);

        [DllImport("Libs/secp256k1.dll", EntryPoint = "secp256k1_ec_privkey_tweak_mul", CallingConvention = CallingConvention.Cdecl)]
        public static extern int PrivateKeyTweakMul(byte[] seckey, byte[] tweak);

        [DllImport("Libs/secp256k1.dll", EntryPoint = "secp256k1_ec_pubkey_tweak_mul", CallingConvention = CallingConvention.Cdecl)]
        public static extern int PublicKeyTweakMul(byte[] pubkey, int pubkeylen, byte[] tweak);

  
    }
}
