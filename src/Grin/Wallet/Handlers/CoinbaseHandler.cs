using Common;
using Grin.Core.Ser;
using Grin.Keychain.ExtKey;
using Grin.Wallet.Types;
using Microsoft.AspNetCore.Mvc;

namespace Grin.Wallet.Handlers
{
    public class CoinbaseHandler
    {
        public CoinbaseHandler(WalletConfig config, Keychain.Keychain.Keychain keychain)
        {
            this.config = config;
            this.keychain = keychain;
        }

        public WalletConfig config { get;  }
        public Keychain.Keychain.Keychain keychain { get; }


      
     public CbData build_coinbase(BlockFees bf)
        {

            var (outp, kern, block_fees) = Receiver.Receiver.receive_coinbase(
                                             config,
                                             keychain,
                                             bf);

            //                             ).map_err(|e| {
            //    api::Error::Internal(format!("Error building coinbase: {:?}", e))

            //})?;

            //var out_bin = ser::ser_vec(&out).map_err(| e | {

            //     api::Error::Internal(format!("Error serializing output: {:?}", e))

            // })?;
            var out_bin = Ser.ser_vec(outp);

            // let kern_bin = ser::ser_vec(&kern).map_err(| e | {

            //     api::Error::Internal(format!("Error serializing kernel: {:?}", e))

            // })?;

            var kern_bin = Ser.ser_vec(kern);

            //    let key_id_bin = match block_fees.key_id_set {

            //        Some(key_id_set) => {
            //            ser::ser_vec(&key_id_set).map_err(|e| {
            //                api::Error::Internal(
            //                    format!("Error serializing kernel: {:?}", e),

            //                    )

            //            })?
            //        }
            //        None => vec![],
            //    };


            var key_id_bin = block_fees.key_id != null ? block_fees.key_id.Bytes : new byte[Identifier.IdentifierSize];

            return new CbData(HexUtil.to_hex(out_bin), HexUtil.to_hex(kern_bin), HexUtil.to_hex(key_id_bin));
       
    }



        public IActionResult Handle(WalletReceiveRequest receiveRequest)
        {
            if (receiveRequest.Coinbase != null)
            {
                var coinbase = build_coinbase(receiveRequest.Coinbase);
                return new JsonResult(coinbase);

            }
            return new BadRequestResult();
        }


// TODO - error handling - what to return if we fail to get the wallet lock for some reason...
    //impl Handler for CoinbaseHandler {
    //fn handle(&self, req: &mut Request) -> IronResult<Response> {
    //    let struct_body = req.get::< bodyparser::Struct < BlockFees >> ();

    //    if let Ok(Some(block_fees)) = struct_body {
    //        let coinbase = self.build_coinbase(&block_fees)
    //                           .map_err(| e | IronError::new(e, status::BadRequest)) ?;
    //        if let Ok(json) = serde_json::to_string(&coinbase) {

    //            Ok(Response::with((status::Ok, json)))
    //        } else {

    //            Ok(Response::with((status::BadRequest, "")))
    //        }
    //    } else {

    //        Ok(Response::with((status::BadRequest, "")))
    //    }
    //}
    //}


    }
}