namespace Grin.Wallet
{
    public class Receiver
    {
    }


    /// Component used to receive coins, implements all the receiving end of the
    /// wallet REST API as well as some of the command-line operations.
//#[derive(Clone)]
    public class WalletReceiver
    {
        public  Keychain.Keychain keychain { get; set; }
        public WalletConfig config { get; set; }
        
    }

    /*


    impl ApiEndpoint for WalletReceiver {
    type ID = String;
    type T = String;
    type OP_IN = WalletReceiveRequest;
    type OP_OUT = CbData;

    fn operations(&self) -> Vec<Operation> {
        vec![Operation::Custom("receive_json_tx".to_string())]
    }

    fn operation(&self, op: String, input: WalletReceiveRequest) -> ApiResult<CbData> {
        match op.as_str()
        {
            "receive_json_tx" => {
                match input {
                    WalletReceiveRequest::PartialTransaction(partial_tx_str) => {
                        debug!(
                            LOGGER,
                            "Operation {} with transaction {}",
                            op,
                            &partial_tx_str,

                            );
                        receive_json_tx(&self.config, &self.keychain, &partial_tx_str)
                            .map_err(| e | {
                            api::Error::Internal(
                                format!("Error processing partial transaction: {:?}", e),

                                )

                        })
                        .unwrap();

                        // TODO: Return emptiness for now, should be a proper enum return type
                        Ok(CbData {
                            output: String::from(""),
                            kernel: String::from(""),
                            key_id: String::from(""),
                        })
                    }
                    _ => Err(api::Error::Argument(format!("Incorrect request data: {}", op))),
                }
            }
            _ => Err(api::Error::Argument(format!("Unknown operation: {}", op))),
        }
    }



    }*/


}