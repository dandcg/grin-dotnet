using System;

namespace Secp256k1Proxy.Lib
{
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
                case Constants.Constants.MESSAGE_SIZE:

                    var ret = new byte[Constants.Constants.MESSAGE_SIZE];
                    Array.Copy(data, ret, Constants.Constants.MESSAGE_SIZE);
                    return new Message(data);


                default:

                    throw new Exception("InvalidMessage");
            }
        }
    }
}