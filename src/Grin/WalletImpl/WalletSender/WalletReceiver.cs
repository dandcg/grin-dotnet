using System;
using Common;
using Grin.ApiImpl.ApiClient;
using Grin.CoreImpl;
using Grin.CoreImpl.Core.Block;
using Grin.CoreImpl.Core.Build;
using Grin.CoreImpl.Core.Transaction;
using Grin.CoreImpl.Ser;
using Grin.KeychainImpl;
using Grin.KeychainImpl.Blind;
using Grin.KeychainImpl.ExtKey;
using Grin.WalletImpl.WalletTypes;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Serilog;

namespace Grin.WalletImpl.WalletReceiver
{
    /// Component used to receive coins, implements all the receiving end of the
    /// wallet REST API as well as some of the command-line operations.
    public class WalletReceiver
    {
        public WalletReceiver(WalletConfig config, Keychain keychain)
        {
            this.Config = config;
            this.Keychain = keychain;
        }

        public WalletConfig Config { get; }
        public Keychain Keychain { get; }


        public IActionResult Handle(string partialTxStr)
        {
            var partialTx = JsonConvert.DeserializeObject<PartialTx>(partialTxStr);

            if (partialTx != null)
            {
                receive_json_tx(Config, Keychain, partialTx);
                return new OkResult();
            }
            return new BadRequestResult();
        }


        /// Receive an already well formed JSON transaction issuance and finalize the
        /// transaction, adding our receiving output, to broadcast to the rest of the
        /// network.
        public static void receive_json_tx(
            WalletConfig config,
            Keychain keychain,
            PartialTx partialTx
        )
        {
            var (amount, blinding, tx) = PartialTx.read_partial_tx(keychain, partialTx);

            var finalTx = receive_transaction(config, keychain, amount, blinding, tx);
            
            var txHex = HexUtil.to_hex(Ser.Ser_vec(finalTx));
            
            //todo:asyncification
            
            var uri = $"{config.check_node_api_http_addr}/v1/pool/push";

            var res = ApiClient.PostAsync(uri, new JsonContent(new TxWrapper {tx_hex = txHex})).Result;
            // var res = ApiClient.PostAsync(uri, new JsonContent(new TxWrapper(){tx_hex=tx_hex})).Result;
            
            //let url = format!("{}/v1/pool/push", config.check_node_api_http_addr.as_str());
            //let _: () = api::client::post(url.as_str(), &TxWrapper { tx_hex: tx_hex
            //})
            //.map_err(|e| Error::Node(e))?;
        }


        // Read wallet data without acquiring the write lock.
        public static (Identifier, uint) retrieve_existing_key(
            WalletConfig config,
            Identifier keyId
        )

        {
            return WalletData.read_wallet(config.data_file_dir, walletData =>
            {
                var existing = walletData.get_output(keyId);

                if (existing != null)

                {
                    var keyId2 = existing.key_id.Clone();
                    var derivation = existing.n_child;

                    return (keyId2, derivation);
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
            Keychain keychain
        )
        {
            var res = WalletData.read_wallet(config.data_file_dir,
                walletData =>
                {
                    var rootKeyId = keychain.Root_key_id();
                    var derivation = walletData.next_child(rootKeyId.Clone());
                    var keyId = keychain.Derive_key_id(derivation);
                    return (keyId, derivation);
                });

            return res;
        }

        /// Build a coinbase output and the corresponding kernel
        public static (Output, TxKernel, BlockFees) receive_coinbase(
            WalletConfig config,
            Keychain keychain,
            BlockFees blockFees
        )
        {
            var rootKeyId = keychain.Root_key_id();
            var keyId = blockFees.KeyId;


            uint derivation;
            if (keyId != null)
            {
                (keyId, derivation) = retrieve_existing_key(config, keyId);
            }
            else
            {
                (keyId, derivation) = next_available_key(config, keychain);
            }

            // Now acquire the wallet lock and write the new output.
            var fees = blockFees;
            WalletData.with_wallet(config.data_file_dir, walletData =>
            {
                // track the new output and return the stuff needed for reward
                var opd = new OutputData(
                    rootKeyId.Clone(),
                    keyId.Clone(),
                    derivation,
                    Consensus.Reward(fees.Fees),
                    OutputStatus.Unconfirmed,
                    0,
                    0,
                    true);

                walletData.add_output(opd);
                return opd;
            });


            Log.Debug("Received coinbase and built candidate output - {root_key_id}, {key_id_set}, {derivation}",
                rootKeyId,
                keyId,
                derivation
            );

            Log.Debug("block_fees - {block_fees}", blockFees);

            blockFees = blockFees.Clone();
            blockFees.Key_id_set(keyId.Clone());

            Log.Debug("block_fees updated - {block_fees}", blockFees);

            var (outd, kern) = Block.Reward_output(keychain, keyId, blockFees.Fees);
            return (outd, kern, blockFees);
        }

        /// Builds a full transaction from the partial one sent to us for transfer
        public static Transaction receive_transaction(
            WalletConfig config,
            Keychain keychain,
            ulong amount,
            BlindingFactor blinding,
            Transaction partial
        )
        {
            var rootKeyId = keychain.Root_key_id();


            var (keyId, derivation) = next_available_key(config, keychain);

            // double check the fee amount included in the partial tx
            // we don't necessarily want to just trust the sender
            // we could just overwrite the fee here (but we won't) due to the ecdsa sig
            var fee = Types.tx_fee((uint) partial.inputs.Length, (uint) partial.outputs.Length + 1, null);

            if (fee != partial.fee)
            {
                throw new WalletErrorException(WalletError.FeeDispute).Data("sender_fee", partial.fee)
                    .Data("recipient_fee", fee);
            }

            var outAmount = amount - fee;


            var (txFinal, _) = Build.transaction(new Func<Context, Append>[]
                {
                    c => c.initial_tx(partial),
                    c => c.with_excess(blinding),
                    c => c.output(outAmount, keyId.Clone())
                }
                , keychain);

            // make sure the resulting transaction is valid (could have been lied to on excess).
            txFinal.validate(keychain.Secp);

            // operate within a lock on wallet data

            var opd = new OutputData(
                rootKeyId.Clone(),
                keyId.Clone(),
                derivation,
                outAmount,
                OutputStatus.Unconfirmed,
                0,
                0,
                false);

            WalletData.with_wallet(config.data_file_dir,
                walletData =>
                {
                    walletData.add_output(opd);

                    return opd;
                });

            Log.Debug(
                "Received txn and built output - {root_key_id}, {key_id}, {derivation}",
                rootKeyId,
                keyId,
                derivation
            );

            return txFinal;
        }
    }
}