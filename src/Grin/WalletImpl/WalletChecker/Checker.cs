using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Grin.ApiImpl.ApiClient;
using Grin.ApiImpl.ApiTypes;
using Grin.KeychainImpl;
using Grin.KeychainImpl.ExtKey;
using Grin.WalletImpl.WalletTypes;
using Newtonsoft.Json;
using Secp256k1Proxy.Pedersen;
using Serilog;

namespace Grin.WalletImpl.WalletChecker
{
    public static class Checker
    {
        // Transitions a local wallet output from Unconfirmed -> Unspent.
        // Also updates the height and lock_height based on latest from the api.
        public static void refresh_output(OutputData od, ApiOutput apiOut)
        {
            od.Height = apiOut.Height;
            od.LockHeight = apiOut.LockHeight;


            switch (od.Status)
            {
                case OutputStatus.Unconfirmed:
                    od.Status = OutputStatus.Unspent;
                    break;
            }
        }


        // Transitions a local wallet output (based on it not being in the node utxo
        // set) -
        // Unspent -> Spent
        // Locked -> Spent
        public static void mark_spent_output(OutputData od)
        {
            switch (od.Status)
            {
                case OutputStatus.Unspent:
                case OutputStatus.Locked:
                    od.Status = OutputStatus.Spent;
                    break;
            }
        }

        /// Builds a single api query to retrieve the latest output data from the node.
        /// So we can refresh the local wallet outputs.
        public static bool refresh_outputs(WalletConfig config, Keychain keychain)

        {
            Log.Debug("Refreshing wallet outputs");


            var walletOutputs = new Dictionary<string, Identifier>();
            var commits = new List<Commitment>();

            // build a local map of wallet outputs by commits
            // and a list of outputs we want to query the node for
            WalletData.read_wallet(config.DataFileDir,
                walletData =>
                {
                    foreach (var op in walletData
                        .Outputs
                        .Values
                        .Where(w => w.RootKeyId == keychain.Root_key_id() && w.Status != OutputStatus.Spent))
                    {
                        var commit = keychain.commit_with_key_index(op.Value, op.NChild);
                        commits.Add(commit);
                        walletOutputs.Add(commit.Hex, op.KeyId.Clone());
                    }

                    return walletData;
                });

            // build the necessary query params -
            // ?id=xxx&id=yyy&id=zzz
            var queryParams = commits.Select(s => $"id={s.Hex}");

            var queryString = string.Join("&", queryParams);

            var url = $"{config.CheckNodeApiHttpAddr}/v1/chain/utxos/byids?{queryString}";


            // build a map of api outputs by commit so we can look them up efficiently
            var apiOutputs = new Dictionary<string, ApiOutput>();

            //HttpClient here
            // todo:asyncification


 
            HttpResponseMessage response;
            try
            {
              response = ApiClient.GetAsync(url).Result;

            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to refresh from {Url}", url);
                return false;
            }


            // if we got anything other than 200 back from server, don't attempt to refresh the wallet
            //  data after
            if (!response.IsSuccessStatusCode)
            {
                Log.Warning("Failed to refresh from {Url} : {StatusCode} - {ReasonPhrase}", url, response.StatusCode, response.ReasonPhrase);
                return false;
            }

            var content = response.Content.ReadAsStringAsync().Result;

            var outputs = JsonConvert.DeserializeObject<ApiOutput[]>(content);

            foreach (var op in outputs)
            {
                apiOutputs.Add(op.Commit.Hex, op);
            }


// now for each commit, find the output in the wallet and
// the corresponding api output (if it exists)
// and refresh it in-place in the wallet.
// Note: minimizing the time we spend holding the wallet lock.
            WalletData.with_wallet(config.DataFileDir, walletData =>
            {
                foreach (var commit in commits)
                {
                    var id = walletOutputs[commit.Hex];

                    if (walletData.Outputs.TryGetValue(id.HexValue, out var op))
                    {
                        if (apiOutputs.TryGetValue(commit.Hex, out var apiOutput))
                        {
                            refresh_output(op, apiOutput);
                        }
                        else
                        {
                            mark_spent_output(op);
                        }
                    }
                }
                return walletData;
            });

            return true;
        }

        public static ApiTip get_tip_from_node(WalletConfig config)
        {


            var uri = $"{config.CheckNodeApiHttpAddr}/v1/chain";
            //todo:asyncification
   
            var response = ApiClient.GetAsync(uri).Result;
            if (response.IsSuccessStatusCode)
            {
                Log.Warning("Failed to refresh from {Url} : {StatusCode} - {ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
                throw new WalletErrorException(WalletError.Node);

            }
            var content = response.Content.ReadAsStringAsync().Result;
            var result = JsonConvert.DeserializeObject<ApiTip>(content);
            return result;
        }
    }
}