namespace Grin.WalletImpl.WalletTypes
{
    public class WalletConfig
    {
        public static WalletConfig Default()
        {
            return new WalletConfig
            {
                EnableWallet = false,
                ApiHttpAddr = "0.0.0.0:13416",
                CheckNodeApiHttpAddr = "http://127.0.0.1:13413",
                DataFileDir = "."
            };
        }


        // Whether to run a wallet
        public bool EnableWallet { get; set; }

        // The api address that this api server (i.e. this wallet) will run
        public string ApiHttpAddr { get; set; }

        // The api address of a running server node, against which transaction inputs will be checked
        // during send
        public string CheckNodeApiHttpAddr { get; set; }

        // The directory in which wallet files are stored
        public string DataFileDir { get; set; }
    }
}