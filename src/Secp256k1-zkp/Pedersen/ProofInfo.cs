namespace Secp256k1Proxy.Pedersen
{
    public class ProofInfo
    {
        /// Whether the proof is valid or not
        public bool Success { get; set; }

        /// Value that was used by the commitment
        public ulong Value { get; set; }

        /// Message embedded in the proof
        public ProofMessage Message { get; set; }

        /// Length of the embedded message (message is "padded" with garbage to fixed number of bytes)
        public int Mlen { get; set; }

        /// Min value that was proven
        public ulong Min { get; set; }

        /// Max value that was proven
        public ulong Max { get; set; }

        /// Exponent used by the proof
        public int Exp { get; set; }

        /// Mantissa used by the proof
        public int Mantissa { get; set; }
    }
}