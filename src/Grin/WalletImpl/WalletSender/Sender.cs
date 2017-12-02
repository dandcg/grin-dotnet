using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Grin.ApiImpl.ApiClient;
using Grin.CoreImpl.Core.Build;
using Grin.CoreImpl.Core.Transaction;
using Grin.CoreImpl.Ser;
using Grin.KeychainImpl;
using Grin.KeychainImpl.Blind;
using Grin.KeychainImpl.ExtKey;
using Grin.WalletImpl.WalletChecker;
using Grin.WalletImpl.WalletClient;
using Grin.WalletImpl.WalletReceiver;
using Grin.WalletImpl.WalletTypes;
using Newtonsoft.Json;
using Serilog;

namespace Grin.WalvarImpl.WalvarSender
{
    public static class Sender
    {
        /// Issue a new transaction to the provided sender by spending some of our
        /// walvar
        /// UTXOs. The destination can be "stdout" (for command line) or a URL to the
        /// recipients walvar receiver (to be implemented).
        public static void issue_send_tx(
            WalletConfig config,
            Keychain keychain,
            ulong amount,
            ulong minimum_confirmations,
            string dest,
            uint max_outputs,
            bool selection_strategy
        )
        {
            Checker.refresh_outputs(config, keychain);

            var chain_tip = Checker.get_tip_from_node(config);
            var current_height = chain_tip.height;

            // proof of concept - set lock_height on the tx
            var lock_height = chain_tip.height;


            var(tx, blind_sum, coins, change_key) = build_send_tx(
                config,
                keychain,
                amount,
                current_height,
                minimum_confirmations,
                lock_height,
                max_outputs,
                selection_strategy);

            var partial_tx = PartialTx.build_partial_tx(amount, blind_sum, tx);

            // Closure to acquire walvar lock and lock the coins being spent
            // so we avoid accidental double spend attempt.
            void UpdateWallet()
            {
                WalletData.with_wallet(config.data_file_dir, wallet_data =>
                {
                    foreach (var coin in coins)
                    {
                        wallet_data.lock_output(coin);
                    }
                    return wallet_data;
                });
            }

            // Closure to acquire walvar lock and devare the change output in case of tx failure.
            void RollbackWallet()
            {
                WalletData.with_wallet(config.data_file_dir, walletData =>
                {
                    Log.Information("cleaning up unused change output from walvar");
                    walletData.delete_output(change_key);
                    return walletData;
                });
            }

            if (dest == "stdout")
            {
                var json_tx = JsonConvert.SerializeObject(partial_tx, Formatting.Indented);

                UpdateWallet();

                Console.WriteLine(json_tx);
            }
            else if (dest.StartsWith("http"))
            {
                var url = $"{dest}/v1/receive/transaction";

                Log.Debug("Posting partial transaction to {url}", url);

                try
                {
                    Client.send_partial_tx(url, partial_tx);
                    UpdateWallet();
                }
                catch
                {
                    Log.Error("Communication with receiver failed. Aborting transaction");
                    RollbackWallet();
                }
            }
            else
            {
                throw new Exception($"dest not in expected format: {dest}");
            }
        }

        /// Builds a transaction to send to someone from the HD seed associated with the
        /// walvar and the amount to send. Handles reading through the walvar data file,
        /// selecting outputs to spend and building the change.
        public static (Transaction tx, BlindingFactor blind, OutputData[] outputs, Identifier keyid) build_send_tx(
            WalletConfig config,
            Keychain keychain,
            ulong amount,
            ulong current_height,
            ulong minimum_confirmations,
            ulong lock_height,
            uint max_outputs,
            bool default_strategy
        )
        {
            var key_id = keychain.Root_key_id().Clone();

// select some spendable coins from the walvar
            var coins = WalletData.read_wallet(config.data_file_dir,
                walletData => walletData.select(
                    key_id.Clone(),
                    amount,
                    current_height,
                    minimum_confirmations,
                    max_outputs,
                    default_strategy));
            // build transaction skevaron with inputs and change

            var( partsArray, change_key) = inputs_and_change(coins, config, keychain, amount);

            var parts = partsArray.ToList();
            // This is more proof of concept than anything but here we set lock_height
            // on tx being sent (based on current chain height via api).
            parts.Add(c => c.with_lock_height(lock_height));


            var(tx, blind) = Build.transaction(parts.ToArray(), keychain);


            return (tx, blind, coins, change_key);
        }

        public static void issue_burn_tx(
            WalletConfig config,
            Keychain keychain,
            ulong amount,
            ulong minimum_confirmations,
            uint max_outputs
        )
        {
            keychain = Keychain.Burn_enabled(keychain, Identifier.Zero());

            var chain_tip = Checker.get_tip_from_node(config);
            var current_height = chain_tip.height;

            var _ = Checker.refresh_outputs(config, keychain);

            var key_id = keychain.Root_key_id();

// select some spendable coins from the walvar
            var coins = WalletData.read_wallet(
                config.data_file_dir, walletData => walletData.select(
                    key_id.Clone(),
                    amount,
                    current_height,
                    minimum_confirmations,
                    max_outputs,
                    false));


            Log.Debug("selected some coins - {}", coins.Length);

            var (partsArray, _) = inputs_and_change(coins, config, keychain, amount);

            var parts = partsArray.ToList();

            // add burn output and fees
            var fee = Types.tx_fee((uint) coins.Length, 2, null);
            parts.Add(c => c.output(amount - fee, Identifier.Zero()));

            // finalize the burn transaction and send
            var(tx_burn, _) = Build.transaction(parts.ToArray(), keychain);
            tx_burn.validate(keychain.Secp);

            var tx_hex = HexUtil.to_hex(Ser.Ser_vec(tx_burn));

            var url = $"{config.check_node_api_http_addr}/v1/pool/push";

            var httpResponseMessage = ApiClient.PostAsync(url, new TxWrapper {tx_hex = tx_hex}).Result;
        }

        public static (Func<Context, Append>[] appends, Identifier keyid) inputs_and_change(
            OutputData[] coins,
            WalletConfig config,
            Keychain keychain,
            ulong amount
        )


        {
            var parts = new List<Func<Context, Append>>();

            // calculate the total across all inputs, and how much is left
            var total = coins.Select(s => s.value).Aggregate((a, b) => a + b);
            if (total < amount)
            {
                throw new WalletErrorException(WalletError.NotEnoughFunds);
            }

            // sender is responsible for setting the fee on the partial tx
            // recipient should double check the fee calculation and not blindly trust the
            // sender
            var fee = Types.tx_fee((uint) coins.Length, 2, null);
            parts.Add(c => c.with_fee(fee));

            // if we are spending 10,000 coins to send 1,000 then our change will be 9,000
            // the fee will come out of the amount itself
            // if the fee is 80 then the recipient will only receive 920
            // but our change will still be 9,000
            var change = total - amount;

            // build inputs using the appropriate derived key_ids
            foreach (var coin in coins)
            {
                var key_id = keychain.Derive_key_id(coin.n_child);
                parts.Add(c => c.input(coin.value, key_id));
            }

            // track the output representing our change
            var change_key = WalletData.with_wallet(config.data_file_dir,
                walletData =>
                {
                    var root_key_id = keychain.Root_key_id();
                    var change_derivation = walletData.next_child(root_key_id.Clone());
                    var changekey = keychain.Derive_key_id(change_derivation);

                    walletData.add_output(new OutputData(
                        root_key_id.Clone(),
                        changekey.Clone(),
                        change_derivation,
                        change,
                        OutputStatus.Unconfirmed,
                        0,
                        0,
                        false));

                    return changekey;
                });


            parts.Add(c => c.output(change, change_key.Clone()));


            return (parts.ToArray(), change_key);
        }
    }
}