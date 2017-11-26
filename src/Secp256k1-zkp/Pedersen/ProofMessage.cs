namespace Secp256k1Proxy
{
    public class ProofMessage
    {
        public byte[] Value { get; }

        private ProofMessage(byte[] value)
        {
            Value = value;
        }

        public static ProofMessage empty()
        {
            return new ProofMessage(new byte[Constants.MESSAGE_SIZE]);
        }

        public ProofMessage clone()
        {
            return new ProofMessage(Value);
        }

        public static ProofMessage from_bytes(byte[] message)
        {
            return new ProofMessage(message);
        }
    }
}