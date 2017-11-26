namespace Secp256k1Proxy
{
    public class ProofInfo
    {
        /// Whether the proof is valid or not
        public bool success { get; set; }

        /// Value that was used by the commitment
        public ulong value { get; set; }

        /// Message embedded in the proof
        public ProofMessage message { get; set; }

        /// Length of the embedded message (message is "padded" with garbage to fixed number of bytes)
        public int mlen { get; set; }

        /// Min value that was proven
        public ulong min { get; set; }

        /// Max value that was proven
        public ulong max { get; set; }

        /// Exponent used by the proof
        public int exp { get; set; }

        /// Mantissa used by the proof
        public int mantissa { get; set; }
    }
}