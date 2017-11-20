using System;
using Microsoft.AspNetCore.Mvc;

namespace Grin.Wallet
{
    public class CoinbaseHandler
    {
        public CoinbaseHandler(WalletConfig config, Keychain.Keychain keychain)
        {
            this.config = config;
            this.keychain = keychain;
        }

        public WalletConfig config { get;  }
        public Keychain.Keychain keychain { get; }


      
     public CbData build_coinbase(BlockFees block_fees)
        {

            var (outp, kern, bf) = Receiver.receive_coinbase(
                                             config,
                                             keychain,
                                             block_fees);

            //                             ).map_err(|e| {
            //    api::Error::Internal(format!("Error building coinbase: {:?}", e))

            //})?;

           //var out_bin = ser::ser_vec(&out).map_err(| e | {

           //     api::Error::Internal(format!("Error serializing output: {:?}", e))

           // })?;

           // let kern_bin = ser::ser_vec(&kern).map_err(| e | {

           //     api::Error::Internal(format!("Error serializing kernel: {:?}", e))

           // })?;

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


        //    Ok(CbData {
        //        output: util::to_hex(out_bin),
        //        kernel: util::to_hex(kern_bin),
        //        key_id_set: util::to_hex(key_id_bin),
        //    })
        //}

            throw new NotImplementedException();

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