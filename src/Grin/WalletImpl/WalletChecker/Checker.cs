using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Net.Http;
using Grin.Api.ApiTypes;
using Grin.ApiImpl.ApiClient;
using Grin.KeychainImpl;
using Grin.KeychainImpl.ExtKey;
using Grin.WalletImpl.WalletClient;
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
        public static void refresh_output(OutputData od, ApiOutput api_out)
        {
            od.height = api_out.height;
            od.lock_height = api_out.lock_height;


            switch (od.status)
            {
                case OutputStatus.Unconfirmed:
                    od.status = OutputStatus.Unspent;
                    break;
            }
        }


        // Transitions a local wallet output (based on it not being in the node utxo
        // set) -
        // Unspent -> Spent
        // Locked -> Spent
        public static void mark_spent_output(OutputData od)
        {
            switch (od.status)
            {
                case OutputStatus.Unspent:
                case OutputStatus.Locked:
                    od.status = OutputStatus.Spent;
                    break;
            }
        }

        /// Builds a single api query to retrieve the latest output data from the node.
        /// So we can refresh the local wallet outputs.
        public static bool refresh_outputs(WalletConfig config, Keychain keychain)

        {
            Log.Debug("Refreshing wallet outputs");


            var wallet_outputs = new Dictionary<string, Identifier>();
            var commits = new List<Commitment>();

            // build a local map of wallet outputs by commits
            // and a list of outputs we want to query the node for
            WalletData.read_wallet(config.data_file_dir,
                wallet_data =>
                {
                    foreach (var op in wallet_data
                        .outputs
                        .Values
                        .Where(w => w.root_key_id == keychain.Root_key_id() && w.status != OutputStatus.Spent))
                    {
                        var commit = keychain.commit_with_key_index(op.value, op.n_child);
                        commits.Add(commit);
                        wallet_outputs.Add(commit.Hex, op.key_id.Clone());
                    }

                    return wallet_data;
                });

            // build the necessary query params -
            // ?id=xxx&id=yyy&id=zzz
            var query_params = commits.Select(s => $"id={s.Hex}");

            var query_string = string.Join("&", query_params);

            var url = $"{config.check_node_api_http_addr}/v1/chain/utxos/byids?{query_string}";


            // build a map of api outputs by commit so we can look them up efficiently
            var api_outputs = new Dictionary<string, ApiOutput>();

            //HttpClient here
            // todo:asyncification


 
            HttpResponseMessage response=null;
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
                api_outputs.Add(op.commit.Hex, op);
            }


// now for each commit, find the output in the wallet and
// the corresponding api output (if it exists)
// and refresh it in-place in the wallet.
// Note: minimizing the time we spend holding the wallet lock.
            WalletData.with_wallet(config.data_file_dir, wallet_data =>
            {
                foreach (var commit in commits)
                {
                    var id = wallet_outputs[commit.Hex];

                    if (wallet_data.outputs.TryGetValue(id.Hex, out var op))
                    {
                        if (api_outputs.TryGetValue(commit.Hex, out var api_output))
                        {
                            refresh_output(op, api_output);
                        }
                        else
                        {
                            mark_spent_output(op);
                        }
                    }
                }
                return wallet_data;
            });

            return true;
        }

        public static ApiTip get_tip_from_node(WalletConfig config)
        {


            var uri = $"{config.check_node_api_http_addr}/v1/chain";
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