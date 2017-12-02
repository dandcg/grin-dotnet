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

namespace Grin.WalletImpl.WalletSender
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
            ulong minimumConfirmations,
            string dest,
            uint maxOutputs,
            bool selectionStrategy
        )
        {
            Checker.refresh_outputs(config, keychain);

            var chainTip = Checker.get_tip_from_node(config);
            var currentHeight = chainTip.Height;

            // proof of concept - set lock_height on the tx
            var lockHeight = chainTip.Height;


            var(tx, blindSum, coins, changeKey) = build_send_tx(
                config,
                keychain,
                amount,
                currentHeight,
                minimumConfirmations,
                lockHeight,
                maxOutputs,
                selectionStrategy);

            var partialTx = PartialTx.build_partial_tx(amount, blindSum, tx);

            // Closure to acquire walvar lock and lock the coins being spent
            // so we avoid accidental double spend attempt.
            void UpdateWallet()
            {
                WalletData.with_wallet(config.DataFileDir, walletData =>
                {
                    foreach (var coin in coins)
                    {
                        walletData.lock_output(coin);
                    }
                    return walletData;
                });
            }

            // Closure to acquire walvar lock and devare the change output in case of tx failure.
            void RollbackWallet()
            {
                WalletData.with_wallet(config.DataFileDir, walletData =>
                {
                    Log.Information("cleaning up unused change output from walvar");
                    walletData.delete_output(changeKey);
                    return walletData;
                });
            }

            if (dest == "stdout")
            {
                var jsonTx = JsonConvert.SerializeObject(partialTx, Formatting.Indented);

                UpdateWallet();

                Console.WriteLine(jsonTx);
            }
            else if (dest.StartsWith("http"))
            {
                var url = $"{dest}/v1/receive/transaction";

                Log.Debug("Posting partial transaction to {url}", url);

                try
                {
                    Client.send_partial_tx(url, partialTx);
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
            ulong currentHeight,
            ulong minimumConfirmations,
            ulong lockHeight,
            uint maxOutputs,
            bool defaultStrategy
        )
        {
            var keyId = keychain.Root_key_id().Clone();

// select some spendable coins from the walvar
            var coins = WalletData.read_wallet(config.DataFileDir,
                walletData => walletData.Select(
                    keyId.Clone(),
                    amount,
                    currentHeight,
                    minimumConfirmations,
                    maxOutputs,
                    defaultStrategy));
            // build transaction skevaron with inputs and change

            var( partsArray, changeKey) = inputs_and_change(coins, config, keychain, amount);

            var parts = partsArray.ToList();
            // This is more proof of concept than anything but here we set lock_height
            // on tx being sent (based on current chain height via api).
            parts.Add(c => c.with_lock_height(lockHeight));


            var(tx, blind) = Build.Transaction(parts.ToArray(), keychain);


            return (tx, blind, coins, changeKey);
        }

        public static void issue_burn_tx(
            WalletConfig config,
            Keychain keychain,
            ulong amount,
            ulong minimumConfirmations,
            uint maxOutputs
        )
        {
            keychain = Keychain.Burn_enabled(keychain, Identifier.Zero());

            var chainTip = Checker.get_tip_from_node(config);
            var currentHeight = chainTip.Height;

            var _ = Checker.refresh_outputs(config, keychain);

            var keyId = keychain.Root_key_id();

// select some spendable coins from the walvar
            var coins = WalletData.read_wallet(
                config.DataFileDir, walletData => walletData.Select(
                    keyId.Clone(),
                    amount,
                    currentHeight,
                    minimumConfirmations,
                    maxOutputs,
                    false));


            Log.Debug("selected some coins - {}", coins.Length);

            var (partsArray, _) = inputs_and_change(coins, config, keychain, amount);

            var parts = partsArray.ToList();

            // add burn output and fees
            var fee = Types.tx_fee((uint) coins.Length, 2, null);
            parts.Add(c => c.Output(amount - fee, Identifier.Zero()));

            // finalize the burn transaction and send
            var(txBurn, _) = Build.Transaction(parts.ToArray(), keychain);
            txBurn.Validate(keychain.Secp);

            var txHex = HexUtil.to_hex(Ser.Ser_vec(txBurn));

            var url = $"{config.CheckNodeApiHttpAddr}/v1/pool/push";

            var res = ApiClient.PostAsync(url, new TxWrapper {TxHex = txHex}).Result;

            Log.Debug("{StatusCode}",res.StatusCode);

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
            var total = coins.Select(s => s.Value).Aggregate((a, b) => a + b);
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
                var keyId = keychain.Derive_key_id(coin.NChild);
                parts.Add(c => c.Input(coin.Value, keyId));
            }

            // track the output representing our change
            var changeKey = WalletData.with_wallet(config.DataFileDir,
                walletData =>
                {
                    var rootKeyId = keychain.Root_key_id();
                    var changeDerivation = walletData.next_child(rootKeyId.Clone());
                    var changekey = keychain.Derive_key_id(changeDerivation);

                    walletData.add_output(new OutputData(
                        rootKeyId.Clone(),
                        changekey.Clone(),
                        changeDerivation,
                        change,
                        OutputStatus.Unconfirmed,
                        0,
                        0,
                        false));

                    return changekey;
                });


            parts.Add(c => c.Output(change, changeKey.Clone()));


            return (parts.ToArray(), changeKey);
        }
    }
}