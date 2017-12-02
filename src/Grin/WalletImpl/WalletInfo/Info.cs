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


            WalletData.read_wallet(config.DataFileDir, walletData =>
            {
                ulong currentHeight;

                try
                {
                    var tip = Checker.get_tip_from_node(config);
                    currentHeight = tip.Height;
                }
                catch
                {
                    currentHeight = walletData.Outputs.Any() ? walletData.Outputs.Values.Max(m => m.Height) : 0;
                }


                ulong unspentTotal = 0;
                ulong unspentButLockedTotal = 0;
                ulong unconfirmedTotal = 0;
                ulong lockedTotal = 0;


                foreach (var op in walletData.Outputs.Values.Where(w => w.RootKeyId == keychain.Root_key_id()))


                {
                    if (op.Status == OutputStatus.Unspent)
                    {
                        unspentTotal += op.Value;
                        if (op.LockHeight > currentHeight)
                        {
                            unspentButLockedTotal += op.Value;
                        }
                    }
                    if (op.Status == OutputStatus.Unconfirmed && !op.IsCoinbase)
                    {
                        unconfirmedTotal += op.Value;
                    }
                    if (op.Status == OutputStatus.Locked)
                    {
                        lockedTotal += op.Value;
                    }
                }
                
                var title = $"Wallet Summary Info - Block Height: {currentHeight}";

                sb.AppendLine($"{title}");
                sb.AppendLine("----------------------------------------------");
                sb.AppendLine("");

                sb.AppendLine($"Total:                          {unspentTotal + unconfirmedTotal}");
                sb.AppendLine($"Awaiting Confirmation:          {unconfirmedTotal}");
                sb.AppendLine($"Confirmed but Still Locked:     {unspentButLockedTotal}");
                sb.AppendLine($"Currently Spendable             {unspentTotal - unspentButLockedTotal}");
                sb.AppendLine("");
                sb.AppendLine("-----------------------------------------------");
                sb.AppendLine("");
                sb.AppendLine($"Locked by previous transaction: {lockedTotal}");

                return walletData;
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