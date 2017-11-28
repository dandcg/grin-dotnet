using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Common.Algo;
using Grin.KeychainImpl.ExtKey;
using Newtonsoft.Json;
using Polly;
using Serilog;

namespace Grin.WalletImpl.WalletTypes
{
    /// Wallet information tracking all our outputs. Based on HD derivation and
    /// avoids storing any key data, only storing output amounts and child index.
    /// This data structure is directly based on the JSON representation stored
    /// on disk, so selection algorithms are fairly primitive and non optimized.
    /// 
    /// TODO optimization so everything isn't O(n) or even O(n^2)
    /// TODO account for fees
    /// TODO write locks so files don't get overwritten
    public class WalletData
    {
        public WalletData()
        {
            outputs = new Dictionary<string, OutputData>();
        }

        public WalletData(Dictionary<string, OutputData> outputs)
        {
            this.outputs = outputs;
        }

        public Dictionary<string, OutputData> outputs { get; }

        /// Allows for reading wallet data (without needing to acquire the write lock).
        public static T read_wallet<T>(string data_file_dir, Func<WalletData, T> f)
        {
            // open the wallet readonly and do what needs to be done with it
            var data_file_path = Path.Combine(data_file_dir, Types.DAT_FILE);
            var wdat = read_or_create(data_file_path);
            return f(wdat);
        }


        /// Allows the reading and writing of the wallet data within a file lock.
        /// Just provide a closure taking a mutable WalletData. The lock should
        /// be held for as short a period as possible to avoid contention.
        /// Note that due to the impossibility to do an actual file lock easily
        /// across operating systems, this just creates a lock file with a "should
        /// not exist" option.
        public static T with_wallet<T>(string data_file_dir, Func<WalletData, T> f)

        {
            //if (activated)
            //{
            //    activated = true;
            //    return this;

            //}

            // create directory if it doesn't exist
            Directory.CreateDirectory(data_file_dir);

            var data_file_path = Path.Combine(data_file_dir, Types.DAT_FILE);
            var lock_file_path = Path.Combine(data_file_dir, Types.LOCK_FILE);

            Log.Information("Acquiring wallet lock ...");


            FileStream lf = null;

            void action()
            {
                Log.Debug("Attempting to acquire wallet lock");
                lf = File.Open(lock_file_path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
            }


            // use tokio_retry to cleanly define some retry logic
            //let mut core = reactor::Core::new().unwrap();
            //let retry_strategy = FibonacciBackoff::from_millis(10).take(10);
            //let retry_future = Retry::spawn(core.handle(), retry_strategy, (Action) Action);
            //let retry_result = core.run(retry_future);

            //match retry_result
            //{
            //    Ok(_) => { },
            //    Err(_) => {
            //        error!(
            //            LOGGER,
            //            "Failed to acquire wallet lock file (multiple retries)",

            //            );
            //        return Err(Error::WalletData(format!("Failed to acquire lock file")));
            //    }
            //}

            try
            {
                Policy
                    .Handle<IOException>()
                    .WaitAndRetry(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)))
                    .Execute(action);
            }
            catch (IOException)
            {
                Log.Error("Failed to acquire wallet lock file (multiple retries)");
                throw new WalletErrorException(WalletError.WalletData,"Failed to acquire lock file");
            }


            // We successfully acquired the lock - so do what needs to be done.
            var wdat = read_or_create(data_file_path);
            var res = f(wdat);
            wdat.write(data_file_path);

            // delete the lock file
            try
            {
                lf?.Dispose();
            }
            catch (IOException ex)
            {
                throw new WalletErrorException(WalletError.WalletData, "Could not remove wallet lock file. Maybe insufficient rights?", ex);
            }


            Log.Information("... released wallet lock");

            return res;
        }

        /// Read the wallet data or created a brand new one if it doesn't exist yet
        public static WalletData read_or_create(string data_file_path)
        {
            if (File.Exists(data_file_path))
            {
                return read(data_file_path);
            }
            // just create a new instance, it will get written afterward
            return new WalletData(new Dictionary<string, OutputData>());
        }

        /// Read the wallet data from disk.
        public static WalletData read(string data_file_path)
        {
            try
            {
                using (var data_file = File.OpenText(data_file_path))
                {
                    var serializer = new JsonSerializer();
                    var ret = (WalletData) serializer.Deserialize(data_file, typeof(WalletData));
                    return ret;
                }
            }
            catch (IOException ex)
            {
                throw new WalletErrorException(WalletError.WalletData, $"Could not open {data_file_path}: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new WalletErrorException(WalletError.WalletData, $"Error reading {data_file_path}: {ex.Message}", ex);
            }
        }

        /// Write the wallet data to disk.
        public void write(string data_file_path)
        {
            try
            {
                using (var data_file = File.CreateText(data_file_path))
                {
                    var serializer = new JsonSerializer();
                    serializer.Serialize(data_file, this);
                }
            }

            catch (IOException ex)
            {
                throw new WalletErrorException(WalletError.WalletData, $"Could not create {data_file_path}: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new WalletErrorException(WalletError.WalletData, $"Error serializing wallet data: {ex.Message}", ex);
            }
        }

        /// Append a new output data to the wallet data.
        /// TODO - we should check for overwriting here - only really valid for
        /// unconfirmed coinbase
        public void add_output(OutputData outd)
        {
            outputs.Add(outd.key_id.Hex, outd.clone());
        }

        // TODO - careful with this, only for Unconfirmed (maybe Locked)?
        public void delete_output(Identifier id)
        {
            outputs.Remove(id.Hex);
        }


        /// Lock an output data.
        /// TODO - we should track identifier on these outputs (not just n_child)
        public void lock_output(OutputData outd)
        {
            var out_to_lock = outputs[outd.key_id.Hex];
            if (out_to_lock != null)
            {
                if (out_to_lock.value == outd.value)
                {
                    out_to_lock.Lock();
                }
            }
        }

        public OutputData get_output(Identifier key_id)
        {
            return outputs[key_id.Hex];
        }

        /// Select spendable coins from the wallet.
        /// Default strategy is to spend the maximum number of outputs (up to max_outputs).
        /// Alternative strategy is to spend smallest outputs first but only as many as necessary.
        /// When we introduce additional strategies we should pass something other than a bool in.
        public OutputData[] select(
            Identifier root_key_id,
            ulong amount,
            ulong current_height,
            ulong minimum_confirmations,
            uint max_outputs,
            bool default_strategy
        )
        {
            // first find all eligible outputs based on number of confirmations
            // sort eligible outputs by increasing value

            var eligible = outputs.Values
                .Where(o => o.root_key_id == root_key_id && o.eligible_to_spend(current_height, minimum_confirmations))
                .OrderBy(o => o.key_id.Hex)
                .ToArray();


            // use a sliding window to identify potential sets of possible outputs to spend
            if (eligible.Length > max_outputs)
            {
                foreach (var window in eligible.Tuples((int) max_outputs))
                {
                    var eligible2 = window.ToArray();
                    var outputs2 = select_from(amount, default_strategy, eligible2);

                    if (outputs2.Any())
                    {
                        return outputs2;
                    }
                }
            }
            else
            {
                var outputs2 = select_from(amount, default_strategy, eligible.Select(s => s.clone()).ToArray());
                if (outputs2.Any())
                {
                    return outputs2;
                }
            }

            // we failed to find a suitable set of outputs to spend,
            // so return the largest amount we can so we can provide guidance on what is possible
            return eligible.Reverse().Take((int) max_outputs).ToArray();
        }


        // Select the full list of outputs if we are using the default strategy.
        // Otherwise select just enough outputs to cover the desired amount.
        public OutputData[] select_from(ulong amount, bool select_all, OutputData[] outputs)
        {
            var total = outputs.Select(s => s.value).Aggregate((a, b) => a + b);

            if (total >= amount)
            {
                if (select_all)
                {
                    return outputs;
                }
                ulong selected_amount = 0;
                var output2 = new List<OutputData>();
                foreach (var o in outputs)
                {
                    output2.Add(o);
                    var res = selected_amount < amount;
                    selected_amount += o.value;
                    if (!res)
                    {
                        break;
                    }
                }

                return output2.ToArray();
            }
            return new OutputData[] { };
        }


        /// Next child index when we want to create a new output.
        public uint next_child(Identifier root_key_id)
        {
            uint max_n = 0;
            foreach (var o in outputs.Values)
            {
                if (max_n < o.n_child && o.root_key_id == root_key_id)
                {
                    max_n = o.n_child;
                }
            }
            return max_n + 1;
        }
    }
}