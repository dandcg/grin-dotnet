namespace Grin.Wallet
{
    public class Types
    {



    }



//#[derive(Debug, Clone, Serialize, Deserialize)]
    public class WalletConfig
    {

        public WalletConfig()
        {
            enable_wallet = false;
            api_http_addr = "0.0.0.0:13416";
            check_node_api_http_addr = "http://127.0.0.1:13413";
            data_file_dir = ".";
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


    /// Status of an output that's being tracked by the wallet. Can either be
    /// unconfirmed, spent, unspent, or locked (when it's been used to generate
    /// a transaction but we don't have confirmation that the transaction was
    /// broadcasted or mined).
     
//#[derive(Serialize, Deserialize, Debug, Clone, PartialEq, Eq)]
    public enum OutputStatus
    {
        Unconfirmed,
        Unspent,
        Locked,
        Spent,
    }




}