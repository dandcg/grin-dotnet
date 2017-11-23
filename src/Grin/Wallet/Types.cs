using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Common;
using Grin.Core;
using Grin.Core.Core;
using Grin.Keychain;
using Konscious.Security.Cryptography;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
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

        /// Transaction fee calculation
        public static UInt64 tx_fee(uint input_len, uint output_len, uint? base_fee)
        {
            var use_base_fee = base_fee ?? DEFAULT_BASE_FEE;



           var tx_weight = -1 * ((int) input_len) + 4 * ((int)output_len) + 1;
            if (tx_weight< 1) {
                tx_weight = 1;
            }

            return ((UInt64) tx_weight) * use_base_fee;
        }


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
        public static WalletConfig Default()
        {
            return new WalletConfig
            {
                enable_wallet = false,
                api_http_addr = "0.0.0.0:13416",
                check_node_api_http_addr = "http://127.0.0.1:13413",
                data_file_dir = "."
            };
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


        public OutputData(Identifier root_key_id, Identifier key_id, uint n_child, ulong value, OutputStatus status,
      ulong height, ulong lock_height, bool is_coinbase)
        {
            this.root_key_id = root_key_id;
            this.key_id = key_id;
            this.n_child = n_child;
            this.value = value;
            this.status = status;
            this.height = height;
            this.lock_height = lock_height;
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
            return new OutputData(root_key_id.Clone(),key_id.Clone(),n_child,value,status,height, lock_height,is_coinbase);
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
        private WalletSeed(byte[] value)
        {
            Value = value;
        }

        public byte[] Value { get; }


        public static WalletSeed from_bytes(byte[] bytes)
        {
            var seed = bytes.Take(32).ToArray();
            return new WalletSeed(seed);
        }

        public static WalletSeed from_hex(string hex)
        {
            var bytes = HexUtil.from_hex(hex);

            return from_bytes(bytes);
        }

        public string to_hex()
        {
            var hex = HexUtil.to_hex(Value);
            return hex;
        }

        public Keychain.Keychain derive_keychain(string password)
        {
            var key = Encoding.ASCII.GetBytes(password);
            var blake2b = new HMACBlake2B(key, 64 * 8);
            var seed = blake2b.ComputeHash(Value);
            var result = Keychain.Keychain.From_seed(seed);

            return result;
        }

        public static WalletSeed init_new()

        {
            var seed = ByteUtil.get_random_bytes(RandomNumberGenerator.Create(), 32);
            return new WalletSeed(seed);
        }

        public static WalletSeed init_file(WalletConfig wallet_config)
        {
            // create directory if it doesn't exist
            Directory.CreateDirectory(wallet_config.data_file_dir);


            var seed_file_path = string.Format(
                "{0}{1}{2}",
                wallet_config.data_file_dir,
                Path.PathSeparator,
                Types.SEED_FILE
            );


            Log.Debug("Generating wallet seed file at: {seed_file_path}", seed_file_path);


            if (File.Exists(seed_file_path))
            {
                throw new Exception("Wallet seed file already exists!");
            }

            var seed = init_new();

            using (var seedFile = File.CreateText(seed_file_path))
            {
                seedFile.Write(seed.to_hex());
            }


            return seed;
        }

        public static WalletSeed from_file(WalletConfig wallet_config)
        {
            // create directory if it doesn't exist
            Directory.CreateDirectory(wallet_config.data_file_dir);


            var seed_file_path = string.Format(
                "{0}{1}{2}",
                wallet_config.data_file_dir,
                Path.PathSeparator,
                Types.SEED_FILE
            );

            Log.Debug("Using wallet seed at: {seed_file_path}", seed_file_path);

            if (File.Exists(seed_file_path))
            {
                using (var file = File.Open(seed_file_path, FileMode.Open, FileAccess.Read, FileShare.None))
                using (var sr = new StreamReader(file))
                {
                    var hex = sr.ReadToEnd();
                    var wallet_seed = from_hex(hex);
                    return wallet_seed;
                }
            }
            Log.Error("Run: \"grin wallet init\" to initialize a new wallet.");
            throw new Exception("wallet seed file does not yet exist (grin wallet init)");
        }
    }

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
            this.outputs = new Dictionary<string, OutputData>();
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
            var partial_tx = JsonConvert.DeserializeObject<JSONPartialTx>(json_str);
            
            var blind_bin = HexUtil.from_hex(partial_tx.blind_sum);
            
            // TODO - turn some data into a blinding factor here somehow
            var blinding = BlindingFactor.from_slice(keychain.Secp, blind_bin);
            
            var tx_bin = HexUtil.from_hex(partial_tx.tx);
            Stream stream = new MemoryStream(tx_bin);
            
            var tx = Ser.deserialize(stream, Transaction.Empty());

            return (partial_tx.amount, blinding, tx);
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
        public BlockFees()
        {
            
        }

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
            return new BlockFees {fees = fees, height = height, key_id = key_id_clone()};
        }

        public Identifier key_id_clone()
        {
            return key_id?.Clone();
        }
    }

    /// Response to build a coinbase output.
    public class CbData
    {
        public CbData(string output, string kernel, string keyId)
        {
            this.output = output;
            this.kernel = kernel;
            key_id = keyId;
        }

        public string output { get; }
        public string kernel { get; }
        public string key_id { get; }
    }
}