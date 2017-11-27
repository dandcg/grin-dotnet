using Grin.Wallet.Types;

namespace Grin.Wallet.Receiver
{
    /// Component used to receive coins, implements all the receiving end of the
    /// wallet REST API as well as some of the command-line operations.
//#[derive(Clone)]
    public class WalletReceiver
    {
        public Keychain.Keychain.Keychain keychain { get; set; }
        public WalletConfig config { get; set; }
    }
}