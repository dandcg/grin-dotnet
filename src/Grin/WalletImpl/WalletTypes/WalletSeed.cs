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
        public override string ToString()
        {
            return HexValue;
        }


        private WalletSeed(byte[] value)
        {
            Value = value;
        }

        public byte[] Value { get; }

        public string HexValue => HexUtil.to_hex(Value);

        public static WalletSeed from_bytes(byte[] bytes)
        {
            var idx = Math.Min(32, bytes.Length);
            var seed = bytes.Take(idx).ToArray();
            var ws= new WalletSeed(seed);
            return ws;
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
            var blake2B = new HMACBlake2B(key, 64 * 8);
            var seed = blake2B.ComputeHash(Value);
            var result = Keychain.From_seed(seed);

            return result;
        }

        public static WalletSeed init_new()

        {
            var seed = ByteUtil.Get_random_bytes(RandomNumberGenerator.Create(),32);
            return new WalletSeed(seed);
        }

        public static WalletSeed init_file(WalletConfig walletConfig)
        {
            // create directory if it doesn't exist
            Directory.CreateDirectory(walletConfig.DataFileDir);


            var seedFilePath = Path.Combine(walletConfig.DataFileDir,Types.SeedFile);


            Log.Debug("Generating wallet seed file at: {seed_file_path}", seedFilePath);


            if (File.Exists(seedFilePath))
            {
                throw new Exception("Wallet seed file already exists!");
            }

            var seed = init_new();

            using (var seedFile = File.CreateText(seedFilePath))
            {
                seedFile.Write(seed.to_hex());
            }


            return seed;
        }

        public static WalletSeed from_file(WalletConfig walletConfig)
        {
            // create directory if it doesn't exist
            Directory.CreateDirectory(walletConfig.DataFileDir);


            var seedFilePath = Path.Combine(walletConfig.DataFileDir, Types.SeedFile);
                
            Log.Debug("Using wallet seed at: {seed_file_path}", seedFilePath);

            if (File.Exists(seedFilePath))
            {
                using (var file = File.Open(seedFilePath, FileMode.Open, FileAccess.Read, FileShare.None))
                using (var sr = new StreamReader(file))
                {
                    var hex = sr.ReadToEnd();
                    var walletSeed = from_hex(hex);
                    return walletSeed;
                }
            }

            return init_file(walletConfig);

            //Log.Error("Run: \"grin wallet init\" to initialize a new wallet.");
            //throw new Exception("wallet seed file does not yet exist (grin wallet init)");
        }
    }
}