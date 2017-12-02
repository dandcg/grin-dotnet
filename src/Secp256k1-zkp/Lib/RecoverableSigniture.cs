using System;
using System.Linq;
using Common;
using Secp256k1Proxy.Ffi;

namespace Secp256k1Proxy.Lib
{
    public class RecoverableSigniture : ICloneable<RecoverableSigniture>
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

        public static RecoverableSigniture From_compact(Secp256K1 secp, byte[] data, RecoveryId recid)
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
            throw new Exception("InvalidSignature");
        }


        public Signiture To_standard(Secp256K1 secp)
        {
            var ret = new byte[65];

            var err = Proxy.secp256k1_ecdsa_recoverable_signature_convert(secp.Ctx, ret, Value);
            if (err == 1)
            {
                return Signiture.From(ret);
            }

            throw new Exception("This should never happen!");
        }

        public (RecoveryId, byte[]) Serialize_compact(Secp256K1 secp)
        {
            var ret = new byte[64];
            var recid = 0;

            var err = Proxy.secp256k1_ecdsa_recoverable_signature_serialize_compact(secp.Ctx, ret, ref recid, Value);

            if (err == 1)
            {
                return (RecoveryId.from_i32(recid), ret);
            }

            throw new Exception("This should never happen!");
        }

        public RecoverableSigniture Clone()
        {
            return new RecoverableSigniture(Value.ToArray());
        }
    }
}