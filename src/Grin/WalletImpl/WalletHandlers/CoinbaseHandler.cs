using Common;
using Grin.CoreImpl.Ser;
using Grin.KeychainImpl;
using Grin.KeychainImpl.ExtKey;
using Grin.WalletImpl.WalletTypes;
using Microsoft.AspNetCore.Mvc;

namespace Grin.WalletImpl.WalletHandlers
{
    public class CoinbaseHandler
    {
        public CoinbaseHandler(WalletConfig config, Keychain keychain)
        {
            Config = config;
            Keychain = keychain;
        }

        public WalletConfig Config { get;  }
        public Keychain Keychain { get; }


      
     public CbData build_coinbase(BlockFees bf)
        {

            var (outp, kern, blockFees) = WalletReceiver.WalletReceiver.receive_coinbase(
                                             Config,
                                             Keychain,
                                             bf);

            //                             ).map_err(|e| {
            //    api::Error::Internal(format!("Error building coinbase: {:?}", e))

            //})?;

            //var out_bin = ser::ser_vec(&out).map_err(| e | {

            //     api::Error::Internal(format!("Error serializing output: {:?}", e))

            // })?;
            var outBin = Ser.Ser_vec(outp);

            // let kern_bin = ser::ser_vec(&kern).map_err(| e | {

            //     api::Error::Internal(format!("Error serializing kernel: {:?}", e))

            // })?;

            var kernBin = Ser.Ser_vec(kern);

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


            var keyIdBin = blockFees.KeyId != null ? blockFees.KeyId.Value : new byte[Identifier.IdentifierSize];

            return new CbData(HexUtil.to_hex(outBin), HexUtil.to_hex(kernBin), HexUtil.to_hex(keyIdBin));
       
    }



        public IActionResult Handle(BlockFees cb)
        {

            var coinbase = build_coinbase(cb);
            return new JsonResult(coinbase);

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