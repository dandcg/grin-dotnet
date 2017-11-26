namespace Grin.Wallet
{
    /// Response to build a coinbase output.
    public class CbData
    {
        public CbData(string output, string kernel, string keyId)
        {
            this.output = output;
            this.kernel = kernel;
            key_id = keyId;
        }

        public string output { get; }
        public string kernel { get; }
        public string key_id { get; }
    }
}