using Common;
using Grin.KeychainImpl;
using Grin.WalletImpl.WalletHandlers;
using Grin.WalletImpl.WalletSender;
using Grin.WalletImpl.WalletTypes;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Grin.WalletImpl.WalletServer
{
    public class ServerController : Controller
    {
        private readonly CoinbaseHandler coinbasehandler;
        private readonly WalletReceiver.WalletReceiver walletReceiver;
        private readonly WalletConfig config;
        private readonly Keychain keychain;

        public ServerController(CoinbaseHandler coinbasehandler, WalletReceiver.WalletReceiver walletReceiver, WalletConfig config, Keychain keychain)
        {
            this.coinbasehandler = coinbasehandler;
            this.walletReceiver = walletReceiver;
            this.config = config;
            this.keychain = keychain;
        }


        [Route("/")]
        [HttpGet]
        public IActionResult Default()
        {
            return Content("-[ Grin Wallet On DotNetCore ]-\n\r");
        }

        [Route("/info")]
        [HttpGet]
        public IActionResult Info()
        {
            var text = WalletInfo.Info.build_info(config, keychain);
            return Content(text);
        }

        [Route("/send")]
        [HttpGet]
        public IActionResult Send()
        {
            uint max_outputs = 500;
            ulong amount = 3000000000;
            ulong minimum_confirmations = 2;
            var dest = "http://localhost:13415";//13415";

            Sender.issue_send_tx(
                config,
                keychain,
                amount,
                minimum_confirmations,
                dest,
                max_outputs,
                false);

            return Ok();
        }


        [Route("/v1/receive/transaction")]
        [HttpPost]
        public IActionResult ReceiveTransaction()
        {
            var partialTxStr = Request.Body.ReadString();
            var res = walletReceiver.Handle(partialTxStr);
            return res;
        }


        [Route("/v1/receive/coinbase")]
        [HttpPost]
        public IActionResult ReceiveCoinbase()
        {
            var str = Request.Body.ReadString();
            var tt = JsonConvert.DeserializeObject<BlockFees>(str);
            var res = coinbasehandler.Handle(tt);
            return res;
        }
        
  }
}