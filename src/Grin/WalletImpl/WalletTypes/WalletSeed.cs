using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Common;
using Grin.KeychainImpl;
using Konscious.Security.Cryptography;
using Serilog;

namespace Grin.WalletImpl.WalletTypes
{
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

        public Keychain derive_keychain(string password)
        {
            var key = Encoding.ASCII.GetBytes(password);
            var blake2b = new HMACBlake2B(key, 64 * 8);
            var seed = blake2b.ComputeHash(Value);
            var result = Keychain.From_seed(seed);

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


            var seed_file_path = Path.Combine(wallet_config.data_file_dir,Types.SEED_FILE);


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


            var seed_file_path = Path.Combine(wallet_config.data_file_dir, Types.SEED_FILE);
                
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

            return init_file(wallet_config);

            //Log.Error("Run: \"grin wallet init\" to initialize a new wallet.");
            //throw new Exception("wallet seed file does not yet exist (grin wallet init)");
        }
    }
}