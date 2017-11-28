using System;
using System.Linq;
using System.Text;
using Grin.KeychainImpl;
using Grin.WalletImpl.WalletChecker;
using Grin.WalletImpl.WalletTypes;

namespace Grin.WalletImpl.WalletInfo
{



    public static class Info
    {
        public static void show_info(WalletConfig config, Keychain keychain)
        {
            var txt = build_info(config, keychain);
            Console.WriteLine(txt);
        }

        public static string build_info(WalletConfig config, Keychain keychain)
        {
            var sb = new StringBuilder();

            var result = Checker.refresh_outputs(config, keychain);


            WalletData.read_wallet(config.data_file_dir, wallet_data =>
            {
                ulong current_height;

                try
                {
                    var tip = Checker.get_tip_from_node(config);
                    current_height = tip.height;
                }
                catch
                {
                    if (wallet_data.outputs.Any())
                    {
                        current_height = wallet_data.outputs.Values.Max(m => m.height);
                    }
                    else
                    {
                        current_height = 0;
                    }
                }


                ulong unspent_total = 0;
                ulong unspent_but_locked_total = 0;
                ulong unconfirmed_total = 0;
                ulong locked_total = 0;


                foreach (var op in wallet_data.outputs.Values.Where(w => w.root_key_id == keychain.Root_key_id()))


                {
                    if (op.status == OutputStatus.Unspent)
                    {
                        unspent_total += op.value;
                        if (op.lock_height > current_height)
                        {
                            unspent_but_locked_total += op.value;
                        }
                    }
                    if (op.status == OutputStatus.Unconfirmed && !op.is_coinbase)
                    {
                        unconfirmed_total += op.value;
                    }
                    if (op.status == OutputStatus.Locked)
                    {
                        locked_total += op.value;
                    }
                }
                ;


                var title = $"Wallet Summary Info - Block Height: {current_height}";

                sb.AppendLine($"{title}");
                sb.AppendLine("----------------------------------------------");
                sb.AppendLine("");

                sb.AppendLine($"Total:                          {unspent_total + unconfirmed_total}");
                sb.AppendLine($"Awaiting Confirmation:          {unconfirmed_total}");
                sb.AppendLine($"Confirmed but Still Locked:     {unspent_but_locked_total}");
                sb.AppendLine($"Currently Spendable             {unspent_total - unspent_but_locked_total}");
                sb.AppendLine("");
                sb.AppendLine("-----------------------------------------------");
                sb.AppendLine("");
                sb.AppendLine($"Locked by previous transaction: {locked_total}");

                return wallet_data;
            });

            if (!result)
            {
                sb.AppendLine("");
                sb.AppendLine(
                    "WARNING - Showing local data only - Wallet was unable to contact a node to update and verify the info shown here.");
            }


            return sb.ToString();
        }
    }
}