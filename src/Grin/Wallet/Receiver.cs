using System;
using Grin.Core;
using Grin.Core.Core;
using Grin.Keychain;
using Serilog;

namespace Grin.Wallet
{
    public class TxWrapper
    {
        public string tx_hex { get; }
    }


    /// Component used to receive coins, implements all the receiving end of the
    /// wallet REST API as well as some of the command-line operations.
//#[derive(Clone)]
    public class WalletReceiver
    {
        public Keychain.Keychain keychain { get; set; }
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
                            key_id_set: String::from(""),
                        })
                    }
                    _ => Err(api::Error::Argument(format!("Incorrect request data: {}", op))),
                }
            }
            _ => Err(api::Error::Argument(format!("Unknown operation: {}", op))),
        }
    }



    }*/

    public static class Receiver
    {
        /// Receive an already well formed JSON transaction issuance and finalize the
        /// transaction, adding our receiving output, to broadcast to the rest of the
        /// network.
        public static void receive_json_tx(
            WalletConfig config,
            Keychain.Keychain keychain,
            string partial_tx_str
        )
        {
            //var (amount, blinding, partial_tx) = partial_tx_from_json(keychain, partial_tx_str)?;
            //let final_tx = receive_transaction(config, keychain, amount, blinding, partial_tx) ?;
            //let tx_hex = util::to_hex(ser::ser_vec(&final_tx).unwrap());

            //let url = format!("{}/v1/pool/push", config.check_node_api_http_addr.as_str());
            //let _: () = api::client::post(url.as_str(), &TxWrapper { tx_hex: tx_hex
            //})
            //.map_err(|e| Error::Node(e))?;
        }


        /// Build a coinbase output and the corresponding kernel
        public static (Output, TxKernel, BlockFees) receive_coinbase(
            WalletConfig config,
            Keychain.Keychain keychain,
            BlockFees block_fees
        )
        {
            var root_key_id = keychain.Root_key_id();
            var key_id = block_fees.key_id_clone();


            var (keyId2, derivation) = retrieve_existing_key(config, key_id);
            if (keyId2 == null)
            {
                (keyId2, derivation) = next_available_key(config, keychain);
            }

            // Now acquire the wallet lock and write the new output.
            WalletData.with_wallet(config.data_file_dir, wallet_data =>
            {
                // track the new output and return the stuff needed for reward
                var opd = new OutputData(
                    root_key_id.Clone(),
                    keyId2.Clone(),
                    derivation,
                    Consensus.reward(block_fees.fees),
                    OutputStatus.Unconfirmed,
                    0,
                    0,
                    true);

                wallet_data.add_output(opd);

                return opd;
            });


            Log.Debug("Received coinbase and built candidate output - {root_key_id}, {key_id_set}, {derivation}",
                root_key_id.Clone(),
                key_id.Clone(),
                derivation
            );

            Log.Debug("block_fees - {block_fees}", block_fees);

            var block_fees2 = block_fees.Clone();
            block_fees2.key_id_set(key_id.Clone());

            Log.Debug("block_fees updated - {block_fees}", block_fees);

            var (outd, kern) = Block.Reward_output(keychain, key_id, block_fees.fees);
            return (outd, kern, block_fees);
        }


        // Read wallet data without acquiring the write lock.
        public static (Identifier, uint) retrieve_existing_key(
            WalletConfig config,
            Identifier key_id
        )

        {
            return WalletData.read_wallet(config.data_file_dir, wallet_data =>
            {
                var existing = wallet_data.get_output(key_id);

                if (existing != null)

                {
                    var key_id_2 = existing.key_id.Clone();
                    var derivation = existing.n_child;

                    return (key_id_2, derivation);
                }

                throw new Exception("This should never happen!");
            });


            //    if let Some(existing) = wallet_data.get_output(&key_id_set) 

            //            {

            //        let key_id_set = existing.key_id_set.clone();
            //        let derivation = existing.n_child;
            //        (key_id_set, derivation)
            //    } else {
            //        panic!("should never happen");
            //    }
            //})?;
        }

        public static (Identifier, uint) next_available_key(
            WalletConfig config,
            Keychain.Keychain keychain
        )
        {
            var res = WalletData.read_wallet(config.data_file_dir,
                wallet_data =>
                {
                    var root_key_id = keychain.Root_key_id();
                    var derivation = wallet_data.next_child(root_key_id.Clone());
                    var key_id = keychain.Derive_key_id(derivation);
                    return (key_id, derivation);
                });

            return res;
        }
    }
}