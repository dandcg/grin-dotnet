using Nancy;

namespace Grin.Wallet
{
    public class Server
    {



    }

    public class WalletServerModule : NancyModule
    {
        public WalletServerModule()
        {
            Get("/", args => "-[ Grin Wallet On DotNetCore ]-");

            Post("/v1/wallet/receive", arg=>"");

            Post("/v1/wallet/finalize", arg => "");

            Post("/v1/wallet/receive_coinbase", arg => "");




            /*
            apis.register_endpoint(
                "/receive".to_string(),
                WalletReceiver {
                config: wallet_config.clone(),
                keychain: keychain.clone(),
            },
            );

            let coinbase_handler = CoinbaseHandler {
                config: wallet_config.clone(),
                keychain: keychain.clone(),
            };
            // let tx_handler = TxHandler{};

            let router = router!(
                receive_coinbase: post "/receive/coinbase" => coinbase_handler,
                // receive_tx: post "/receive/tx" => tx_handler,
                );
            apis.register_handler("/v2", router);

            apis.start(wallet_config.api_http_addr).unwrap_or_else(| e | {
                error!(LOGGER, "Failed to start Grin wallet receiver: {}.", e);
            });
            */

        }
    }


}