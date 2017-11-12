using System;

namespace Grin.Core
{
    public static class Global
    {
        //! Values that should be shared across all modules, without necessarily
        //! having to pass them all over the place, but aren't consensus values.
        //! should be used sparingly.

        /// An enum collecting sets of parameters used throughout the
        /// code wherever mining is needed. This should allow for
        /// different sets of parameters for different purposes,
        /// e.g. CI, User testing, production values

        //use std::sync::RwLock;

        //use consensus::PROOFSIZE;
        //use consensus::DEFAULT_SIZESHIFT;
        //use consensus::COINBASE_MATURITY;
        /// Define these here, as they should be developer-set, not really tweakable
        /// by users
        /// Automated testing sizeshift
        public const byte AUTOMATED_TESTING_SIZESHIFT = 10;

        /// Automated testing proof size
        public const uint AUTOMATED_TESTING_PROOF_SIZE = 4;

        /// User testing sizeshift
        public const byte USER_TESTING_SIZESHIFT = 16;

        /// User testing proof size
        public const uint USER_TESTING_PROOF_SIZE = 42;

        /// Automated testing coinbase maturity
        public const ulong AUTOMATED_TESTING_COINBASE_MATURITY = 3;

        /// User testing coinbase maturity
        public const ulong USER_TESTING_COINBASE_MATURITY = 3;

        /// Mining parameter modes
        public enum MiningParameterMode
        {
            /// For CI testing
            AutomatedTesting,

            /// For User testing
            UserTesting,

            /// For production, use the values in consensus.rs
            Production
        }

        //    lazy_static! {
        //        /// The mining parameter mode
        //        pub static ref MINING_PARAMETER_MODE: RwLock<MiningParameterMode> =
        //    RwLock::new (MiningParameterMode::Production);
        //}

        /// Set the mining mode
        public static void set_mining_mode(MiningParameterMode mode)
        {
            throw new NotImplementedException();
            //let mut param_ref = MINING_PARAMETER_MODE.write().unwrap();
            //    *param_ref = mode;
        }

        /// The sizeshift
        public static byte sizeshift()

        {
            throw new NotImplementedException();
            //var param_ref = MINING_PARAMETER_MODE.read().unwrap();

            //match* param_ref
            //{
            //    MiningParameterMode:: AutomatedTesting => AUTOMATED_TESTING_SIZESHIFT,
            //    MiningParameterMode:: UserTesting => USER_TESTING_SIZESHIFT,
            //    MiningParameterMode:: Production => DEFAULT_SIZESHIFT,
            //}
        }

        /// The proofsize
        public static uint proofsize()
        {
            throw new NotImplementedException();
            //        let param_ref = MINING_PARAMETER_MODE.read().unwrap();

            //match* param_ref
            //{
            //    MiningParameterMode:: AutomatedTesting => AUTOMATED_TESTING_PROOF_SIZE,
            //    MiningParameterMode:: UserTesting => USER_TESTING_PROOF_SIZE,
            //    MiningParameterMode:: Production => PROOFSIZE,
            //}
        }

        /// Coinbase maturity
        public static ulong coinbase_maturity()

        {
            throw new NotImplementedException();
            //let param_ref = MINING_PARAMETER_MODE.read().unwrap();

            //match* param_ref
            //{
            //    MiningParameterMode:: AutomatedTesting => AUTOMATED_TESTING_COINBASE_MATURITY,
            //    MiningParameterMode:: UserTesting => USER_TESTING_COINBASE_MATURITY,
            //    MiningParameterMode:: Production => COINBASE_MATURITY,
            //}
        }

        /// Are we in automated testing mode?
        public static bool is_automated_testing_mode()
        {
            throw new NotImplementedException();
            //let param_ref = MINING_PARAMETER_MODE.read().unwrap();
            //    if let MiningParameterMode::AutomatedTesting = *param_ref {
            //    return true;
            //} else {
            //    return false;
            //}
        }

        /// Are we in production mode?
        public static bool is_production_mode()
        {
            throw new NotImplementedException();
            //let param_ref = MINING_PARAMETER_MODE.read().unwrap();
            //    if let MiningParameterMode::Production = *param_ref {
            //    return true;
            //} else {
            //    return false;
            //}
        }


        /// Helper function to get a nonce known to create a valid POW on
        /// the genesis block, to prevent it taking ages. Should be fine for now
        /// as the genesis block POW solution turns out to be the same for every new
        /// block chain
        /// at the moment
        public static ulong get_genesis_nonce()
        {
            throw new NotImplementedException();
            //        let param_ref = MINING_PARAMETER_MODE.read().unwrap();

            //match* param_ref
            //{
            //    // won't make a difference
            //    MiningParameterMode:: AutomatedTesting => 0,
            //    // Magic nonce for current genesis block at cuckoo16
            //    MiningParameterMode:: UserTesting => 22141,
            //    // Magic nonce for current genesis at cuckoo30
            //    MiningParameterMode:: Production => 1429942738856787200,
            //}
        }

        /// Returns the genesis POW for production, because it takes far too long to
        /// mine at production values
        /// using the internal miner
        public static uint[] get_genesis_pow()

        {
            return new uint[42]

// TODO: This is diff 26, probably just want a 10: mine one
                {
                    7444824,
                    11926557,
                    28520390,
                    30594072,
                    50854023,
                    52797085,
                    57882033,
                    59816511,
                    61404804,
                    84947619,
                    87779345,
                    115270337,
                    162618676,
                    166860710,
                    178656003,
                    178971372,
                    200454733,
                    209197630,
                    221231015,
                    228598741,
                    241012783,
                    245401183,
                    279080304,
                    295848517,
                    327300943,
                    329741709,
                    366394532,
                    382493153,
                    389329248,
                    404353381,
                    406012911,
                    418813499,
                    426573907,
                    452566575,
                    456930760,
                    463021458,
                    474340589,
                    476248039,
                    478197093,
                    487576917,
                    495653489,
                    501862896
                };
        }
    }
}