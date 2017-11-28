namespace Grin.WalletImpl.WalletTypes
{
    public class WalletConfig
    {
        public static WalletConfig Default()
        {
            return new WalletConfig
            {
                enable_wallet = false,
                api_http_addr = "0.0.0.0:13416",
                check_node_api_http_addr = "http://127.0.0.1:13413",
                data_file_dir = "."
            };
        }


        // Whether to run a wallet
        public bool enable_wallet { get; set; }

        // The api address that this api server (i.e. this wallet) will run
        public string api_http_addr { get; set; }

        // The api address of a running server node, against which transaction inputs will be checked
        // during send
        public string check_node_api_http_addr { get; set; }

        // The directory in which wallet files are stored
        public string data_file_dir { get; set; }
    }
}