using System;
using System.IO;
using System.Net.Http;
using System.Text;
using Common;
using Grin.ApiImpl.ApiClient;
using Grin.WalletImpl.WalletTypes;
using Newtonsoft.Json;
using Polly;
using Serilog;

namespace Grin.WalletImpl.WalletClient
{
    public static class Client
    {
        /// Call the wallet API to create a coinbase output for the given block_fees.
        /// Will retry based on default "retry forever with backoff" behavior.
        public static CbData create_coinbase(string url, BlockFees blockFees)
        {
            return retry_backoff_forever(
                () =>
                {
                    try
                    {
                        var res = single_create_coinbase(url, blockFees);
                        return res;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                });
        }

        /// Runs the specified function wrapped in some basic retry logic.
        public static T retry_backoff_forever<T>(Func<T> action)

        {
            return Policy
                .Handle<IOException>()
                .WaitAndRetry(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)))
                .Execute(action);
        }

        public static void send_partial_tx(string url, PartialTx partialTx)
        {
            single_send_partial_tx(url, partialTx);
        }

        public static void single_send_partial_tx(string url, PartialTx partialTx)
        {

            var json = JsonConvert.SerializeObject(partialTx);
            var stringContent = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            var req = ApiClient.PostContentAsync(url, stringContent).Result;

            if (req.IsSuccessStatusCode)
            {
                Log.Information("Transaction sent successfully");
            }
            else
            {
                Log.Error("Error sending transaction - status: {status}", req.StatusCode);
                throw new WalletErrorException(WalletError.Hyper);
            }
        }

        /// Makes a single request to the wallet API to create a new coinbase output.
        public static CbData single_create_coinbase(string url, BlockFees blockFees)
        {
            var json = JsonConvert.SerializeObject(blockFees);
            var stringContent = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            var req = ApiClient.PostContentAsync(url, stringContent).Result;

            if (req.IsSuccessStatusCode)
            {
                var stream = req.Content.ReadAsStreamAsync().Result;
                var cbData = stream.ReadJson<CbData>();
                return cbData;
            }

            throw new Exception(req.StatusCode.ToString());
        }
    }
}