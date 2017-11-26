using System;
using System.Diagnostics;
using System.IO;
using System.Net.Mime;
using Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using Newtonsoft.Json;

namespace Grin.Wallet
{

    public class ServerController:Controller
    {
        private readonly CoinbaseHandler coinbasehandler;

        public ServerController(CoinbaseHandler coinbasehandler)
        {
            this.coinbasehandler = coinbasehandler;
        }


        [Route("/")]
        [HttpGet]
        public IActionResult Default()
        {
            return Content("-[ Grin Wallet On DotNetCore ]-");
        }

        [Route("/v1/receive/coinbase")]
        [HttpPost]
        public IActionResult ReceiveCoinbase()
        {

            var tt = Request.Body.ReadJson<WalletReceiveRequest>();
            var res = coinbasehandler.Handle(tt);
            return res;

        }



        //    public WalletServerModule()
        //    {



        //        Get("/", args => "-[ Grin Wallet On DotNetCore ]-");

        //        Get("/test", args =>
        //        {

        //            Console.WriteLine("Hello!");
        //            return "-[ Grin Wallet On DotNetCore ]-";
        //        });

        //        Post("/v1/receive", arg=>"");

        //        Post("/v1/finalize", arg => "");

        //        Post("/v1/receive/coinbase", arg =>
        //        {
        //            var body = RequestStream.FromStream(Request.Body);


        //            //var sr = new StreamReader(body);

        //            //using (JsonReader reader = new JsonTextReader(sr))
        //            //{
        //            //    var serializer = new JsonSerializer();
        //            //    var tt = serializer.Deserialize<WalletReceiveRequest>(reader);

        //            //    Console.WriteLine(JsonConvert.SerializeObject(tt));
        //            //}


        //             var foo = this.Bind<WalletReceiveRequest>(new BindingConfig(){ BodyOnly = true});
        //            Console.WriteLine(JsonConvert.SerializeObject(foo));
        //            return "";
        //        });




        //        /*
        //        apis.register_endpoint(
        //            "/receive".to_string(),
        //            WalletReceiver {
        //            config: wallet_config.clone(),
        //            keychain: keychain.clone(),
        //        },
        //        );

        //        let coinbase_handler = CoinbaseHandler {
        //            config: wallet_config.clone(),
        //            keychain: keychain.clone(),
        //        };
        //        // let tx_handler = TxHandler{};

        //        let router = router!(
        //            receive_coinbase: post "/receive/coinbase" => coinbase_handler,
        //            // receive_tx: post "/receive/tx" => tx_handler,
        //            );
        //        apis.register_handler("/v2", router);

        //        apis.start(wallet_config.api_http_addr).unwrap_or_else(| e | {
        //            error!(LOGGER, "Failed to start Grin wallet receiver: {}.", e);
        //        });
        //        */

        //    }
    }


}