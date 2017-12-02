namespace Grin.WalletImpl.WalletTypes
{
    /// Response to build a coinbase output.
    public class CbData
    {
        public CbData(string output, string kernel, string keyId)
        {
            Output = output;
            Kernel = kernel;
            KeyId = keyId;
        }

        public string Output { get; }
        public string Kernel { get; }
        public string KeyId { get; }
    }
}