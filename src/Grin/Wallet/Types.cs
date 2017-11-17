using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Common;
using Grin.Core;
using Grin.Core.Core;
using Grin.Keychain;
using Newtonsoft.Json;
using Polly;
using Serilog;

namespace Grin.Wallet
{
    public class Types
    {
        public const string DAT_FILE = "wallet.dat";
        public const string LOCK_FILE = "wallet.lock";
        public const string SEED_FILE = "wallet.seed";

        public const ulong DEFAULT_BASE_FEE = 10;
    }

    /// Wallet errors, mostly wrappers around underlying crypto or I/O errors.
    public enum WalletError
    {
        NotEnoughFunds, //(u64),

        FeeDispute, //{sender_fee: u64, recipient_fee: u64

        Keychain, //(keychain::Error),

        Transaction, //(transaction::Error),

        Secp, //(secp::Error),

        WalletData, //(String),

        /// An error in the format of the JSON structures exchanged by the wallet
        Format, //(String),

        /// An IO Error
        IOError, //(io::Error),

        /// Error when contacting a node through its API
        Node, //(api::Error),

        /// Error originating from hyper.
        Hyper, //(hyper::Error),

        /// Error originating from hyper uri parsing.
        Uri //(hyper::error::UriError),
    }


//#[derive(Debug, Clone, Serialize, Deserialize)]
    public class WalletConfig
    {
        public WalletConfig()
        {
            enable_wallet = false;
            api_http_addr = "0.0.0.0:13416";
            check_node_api_http_addr = "http://127.0.0.1:13413";
            data_file_dir = ".";
        }


        // Whether to run a wallet
        public bool enable_wallet { get; set; }

        // The api address that this api server (i.e. this wallet) will run
        public string api_http_addr { get; set; }

        // The api address of a running server node, against which transaction inputs will be checked
        // during send
        public string check_node_api_http_addr { get; set; }

        // The directory in which wallet files are stored
        public string data_file_dir { get; set; }
    }


    /// Status of an output that's being tracked by the wallet. Can either be
    /// unconfirmed, spent, unspent, or locked (when it's been used to generate
    /// a transaction but we don't have confirmation that the transaction was
    /// broadcasted or mined).

//#[derive(Serialize, Deserialize, Debug, Clone, PartialEq, Eq)]
    public enum OutputStatus
    {
        Unconfirmed,
        Unspent,
        Locked,
        Spent
    }


    /// Information about an output that's being tracked by the wallet. Must be
    /// enough to reconstruct the commitment associated with the ouput when the
    /// root private key is known.
    public class OutputData
    {
        private object value1;
        private int height1;
        private int lock_height1;

        public OutputData(Identifier root_key_id, Identifier key_id, uint n_child, ulong value, OutputStatus status,
            int height, int lock_height, bool is_coinbase)
        {
            this.root_key_id = root_key_id;
            this.key_id = key_id;
            this.n_child = n_child;
            value1 = value;
            this.status = status;
            height1 = height;
            lock_height1 = lock_height;
            this.is_coinbase = is_coinbase;
        }

        /// Root key_id_set that the key for this output is derived from
        public Identifier root_key_id { get; }

        /// Derived key for this output
        public Identifier key_id { get; }

        /// How many derivations down from the root key
        public uint n_child { get; }

        /// Value of the output, necessary to rebuild the commitment
        public ulong value { get; }

        /// Current status of the output
        public OutputStatus status { get; private set; }

        /// Height of the output
        public ulong height { get; }

        /// Height we are locked until
        public ulong lock_height { get; }

        /// Is this a coinbase output? Is it subject to coinbase locktime?
        public bool is_coinbase { get; }

        public OutputData clone()
        {
            throw new NotImplementedException();
        }

        /// Lock a given output to avoid conflicting use
        public void Lock()
        {
            status = OutputStatus.Locked;
        }

        /// How many confirmations has this output received?
        /// If height == 0 then we are either Unconfirmed or the output was cut-through
        /// so we do not actually know how many confirmations this output had (and never will).
        public ulong num_confirmations(ulong current_height)
        {
            if (status == OutputStatus.Unconfirmed)
            {
                return 0;
            }
            if (status == OutputStatus.Spent && height == 0)
            {
                return 0;
            }
            return current_height - height;
        }

        /// Check if output is eligible for spending based on state and height.
        public bool eligible_to_spend(
            ulong current_height,
            ulong minimum_confirmations
        )
        {
            if (new[]
            {
                OutputStatus.Spent,
                OutputStatus.Locked
            }.Contains(status))
            {
                return false;
            }
            if (status == OutputStatus.Unconfirmed && is_coinbase)
            {
                return false;
            }
            if (lock_height > current_height)
            {
                return false;
            }
            if (status == OutputStatus.Unspent && height + minimum_confirmations <= current_height)
            {
                return true;
            }
            if (status == OutputStatus.Unconfirmed && minimum_confirmations == 0)
            {
                return true;
            }
            return false;
        }
    }

    public class WalletSeed
    {
        public byte[] Value { get; }
    }

    public class WalletData
    {
        public WalletData(Dictionary<string, OutputData> outputs)
        {
            this.outputs = outputs;
        }

        public Dictionary<string, OutputData> outputs { get; }

        /// Allows for reading wallet data (without needing to acquire the write lock).
        public static T read_wallet<T>(string data_file_dir, Func<WalletData, T> f)
        {
            // open the wallet readonly and do what needs to be done with it
            var data_file_path = string.Format("{0}{1}{2}", data_file_dir, Path.PathSeparator, Types.DAT_FILE);
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

            var data_file_path = string.Format("{0}{1}{2}", data_file_dir, Path.PathSeparator, Types.DAT_FILE);
            var lock_file_path = string.Format("{0}{1}{2}", data_file_dir, Path.PathSeparator, Types.LOCK_FILE);

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
                throw new WalletDataException("Failed to acquire lock file");
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
                throw new WalletDataException("Could not remove wallet lock file. Maybe insufficient rights?", ex);
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
                throw new WalletDataException($"Could not open {data_file_path}: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new WalletDataException($"Error reading {data_file_path}: {ex.Message}", ex);
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
                throw new WalletDataException($"Could not create {data_file_path}: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new WalletDataException($"Error serializing wallet data: {ex.Message}", ex);
            }
        }

        /// Append a new output data to the wallet data.
        /// TODO - we should check for overwriting here - only really valid for
        /// unconfirmed coinbase
        public void add_output(OutputData outd)
        {
            outputs.Add(outd.key_id.Hex, outd.clone());
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

        /// Select spendable coins from the wallet
        public OutputData[] select(
            Identifier root_key_id,
            ulong current_height,
            ulong minimum_confirmations
        )
        {
            return outputs
                .Values
                .Where(o => o.root_key_id == root_key_id
                            && o.eligible_to_spend(current_height, minimum_confirmations)).ToArray();
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


    public class JSONPartialTx
    {
        public ulong amount { get; private set; }
        public string blind_sum { get; private set; }
        public string tx { get; private set; }

        /// Encodes the information for a partial transaction (not yet completed by the
        /// receiver) into JSON.
        public static string partial_tx_to_json(ulong receive_amount,
            BlindingFactor blind_sum,
            Transaction tx)
        {
            var partial_tx = new JSONPartialTx
            {
                amount = receive_amount,
                blind_sum = HexUtil.to_hex(blind_sum.Key.Value),
                tx = HexUtil.to_hex(Ser.ser_vec(tx))
            };

            return"";

//            partial_tx
        }

        /// Reads a partial transaction encoded as JSON into the amount, sum of blinding
        /// factors and the transaction itself.
        public (ulong, BlindingFactor, Transaction) partial_tx_from_json(Keychain.Keychain keychain, string json_str)
        {
            throw new NotImplementedException();

//        JSONPartialTx partial_tx:  = serde_json::from_str(json_str)?;

//        var blind_bin = util::from_hex(partial_tx.blind_sum) ?;

//// TODO - turn some data into a blinding factor here somehow
//// let blinding = SecretKey::from_slice(&secp, &blind_bin[..])?;
//            var blinding = BlindingFactor::from_slice(keychain.secp(), &blind_bin[..]) ?;

//            let tx_bin = util::from_hex(partial_tx.tx) ?;
//            let tx = ser::deserialize(&mut & tx_bin[..])
//                .map_err(| _ | {

//                Error::Format("Could not deserialize transaction, invalid format.".to_string())
//            })?;


//        return (partial_tx.amount, blinding, tx);
//    }
        }
    }

    /// Amount in request to build a coinbase output.
    public class WalletReceiveRequest
    {
        public BlockFees Coinbase { get; set; }
        public string PartialTransaction { get; set; }
        public string Finalize { get; set; }
    }






    public class BlockFees
    {
        [JsonProperty]
        public ulong fees { get; private set; }
        [JsonProperty]
        public ulong height { get; private set; }
        [JsonProperty]
        public Identifier key_id { get; private set; }

   

        public void key_id_set(Identifier keyId)
        {
            key_id = keyId;
        }

        public BlockFees Clone()
        {
            return new BlockFees() {fees = fees, height = height, key_id = key_id_clone()};
        }

        public Identifier key_id_clone()
        {
            return key_id.Clone();
        }
    }

    public class CbData
    {
        public string output { get; }
        public string kernel { get; }
        public string key_id { get; }
    }
}