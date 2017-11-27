using System;
using Secp256k1Proxy.Ffi;

namespace Secp256k1Proxy.Lib
{
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

        public static RecoverableSigniture from_compact(Secp256k1 secp, byte[] data, RecoveryId recid)
        {




            if (data.Length != 64)
            {
                throw new Exception("InvalidSignature");
            }


            var ret = new byte[65];

            if (Proxy.secp256k1_ecdsa_recoverable_signature_parse_compact(secp.Ctx, ret, data, recid.Value) == 1)
            {
                return new RecoverableSigniture(ret);
            }
            else
            {
                throw new Exception("InvalidSignature");
            }

        }


        public Signiture to_standard(Secp256k1 secp)
        {
            var ret = new byte[65];

            var err = Proxy.secp256k1_ecdsa_recoverable_signature_convert(secp.Ctx, ret, this.Value);
            if (err == 1)
            {
                return Signiture.From(ret);
            }

            throw new Exception("This should never happen!");
        }

        public (RecoveryId, byte[]) serialize_compact(Secp256k1 secp)
        {
            var ret = new  byte[64];
            Int32 recid = 0;
    
            var err = Proxy.secp256k1_ecdsa_recoverable_signature_serialize_compact(secp.Ctx, ret,ref recid,Value);
        
            if (err == 1)
            {
                return (RecoveryId.from_i32(recid), ret);
            }

            throw new Exception("This should never happen!");
        }
    }
}