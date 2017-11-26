using System;

namespace Secp256k1Proxy
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

        public RecoveryId clone()
        {
            var rcid=new RecoveryId(Value);
            return rcid;
        }
    }


    // ReSharper disable once InconsistentNaming
}
