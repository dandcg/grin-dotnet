using System;
using System.Collections.Generic;
using Grin.CoreImpl.Core.Target;

namespace Grin.CoreImpl
{
    public class Consensus
    {
        //! All the rules required for a cryptocurrency to have reach consensus across
        //! the whole network are complex and hard to completely isolate. Some can be
        //! simple parameters (like block reward), others complex algorithms (like
        //! Merkle sum trees or reorg rules). However, as long as they're simple
        //! enough, consensus-relevant constants and short functions should be kept
        //! here.


        /// A grin is divisible to 10^9, following the SI prefixes
        public const ulong GRIN_BASE = 1_000_000_000;
        /// Milligrin, a thousand of a grin
        public const ulong MILLI_GRIN = GRIN_BASE / 1_000;
        /// Microgrin, a thousand of a milligrin
        public const ulong MICRO_GRIN = MILLI_GRIN / 1_000;
        /// Nanogrin, smallest unit, takes a billion to make a grin
        public const ulong NANO_GRIN= 1;


        /// The block subsidy amount
        public const ulong REWARD = 50 * GRIN_BASE;

        /// Actual block reward for a given total fee amount
        public static ulong Reward(ulong fee)
        {
            return REWARD + fee / 2;
        }

        /// Number of blocks before a coinbase matures and can be spent
        public const ulong COINBASE_MATURITY = 1_000;

        /// Block interval, in seconds, the network will tune its next_target for. Note
        /// that we may reduce this value in the future as we get more data on mining
        /// with Cuckoo Cycle, networks improve and block propagation is optimized
        /// (adjusting the reward accordingly).
        public const ulong BLOCK_TIME_SEC = 60;

        /// Cuckoo-cycle proof size (cycle length)
        public const uint PROOFSIZE = 42;

        /// Default Cuckoo Cycle size shift used for mining and validating.
        public const ulong DEFAULT_SIZESHIFT = 30;

        /// Default Cuckoo Cycle easiness, high enough to have good likeliness to find
        /// a solution.
        public const uint EASINESS = 50;

        /// Default number of blocks in the past when cross-block cut-through will start
        /// happening. Needs to be long enough to not overlap with a long reorg.
        /// Rational
        /// behind the value is the longest bitcoin fork was about 30 blocks, so 5h. We
        /// add an order of magnitude to be safe and round to 48h of blocks to make it
        /// easier to reason about.
        public const uint CUT_THROUGH_HORIZON = 48 * 3600 / (uint) BLOCK_TIME_SEC;

        /// The maximum size we're willing to accept for any message. Enforced by the
        /// peer-to-peer networking layer only for DoS protection.
        public const ulong MAX_MSG_LEN = 20_000_000;

        /// Weight of an input when counted against the max block weigth capacity
        public const uint BLOCK_INPUT_WEIGHT = 1;

        /// Weight of an output when counted against the max block weight capacity
        public const uint BLOCK_OUTPUT_WEIGHT = 10;

        /// Weight of a kernel when counted against the max block weight capacity
        public const uint BLOCK_KERNEL_WEIGHT = 2;

        /// Total maximum block weight
        public const uint MAX_BLOCK_WEIGHT = 80_000;

        /// Maximum inputs for a block (issue#261)
        /// Hundreds of inputs + 1 output might be slow to validate (issue#258)
        public const uint MAX_BLOCK_INPUTS = 300_000; // soft fork down when too_high



        /// Whether a block exceeds the maximum acceptable weight
        public static bool Exceeds_weight(uint input_len, uint output_len, uint kernel_len)

        {
            return input_len * BLOCK_INPUT_WEIGHT + output_len * BLOCK_OUTPUT_WEIGHT +
                   kernel_len * BLOCK_KERNEL_WEIGHT > MAX_BLOCK_WEIGHT;
        }

        /// Fork every 250,000 blocks for first 2 years, simple number and just a
        /// little less than 6 months.
        public const ulong HARD_FORK_INTERVAL = 250_000;

        /// Check whether the block version is valid at a given height, implements
        /// 6 months interval scheduled hard forks for the first 2 years.
        public bool Valid_header_version(ulong height, ushort version)
        {
            // uncomment below as we go from hard fork to hard fork
            if (height <= HARD_FORK_INTERVAL && version == 1)
            {
                return true;
                /* } else if height <= 2 * HARD_FORK_INTERVAL && version == 2 {
                    true */
                /* } else if height <= 3 * HARD_FORK_INTERVAL && version == 3 {
                    true */
                /* } else if height <= 4 * HARD_FORK_INTERVAL && version == 4 {
                    true */
                /* } else if height > 4 * HARD_FORK_INTERVAL && version > 4 {
                    true */
            }
            return false;
        }

        /// The minimum mining difficulty we'll allow
        public const ulong MINIMUM_DIFFICULTY = 10;

        /// Time window in blocks to calculate block time median
        public const ulong MEDIAN_TIME_WINDOW = 11;

        /// Number of blocks used to calculate difficulty adjustments
        public const ulong DIFFICULTY_ADJUST_WINDOW = 23;

        /// Average time span of the difficulty adjustment window
        public const ulong BLOCK_TIME_WINDOW = DIFFICULTY_ADJUST_WINDOW * BLOCK_TIME_SEC;

        /// Maximum size time window used for difficulty adjustments
        public const ulong UPPER_TIME_BOUND = BLOCK_TIME_WINDOW* 4 / 3;

        /// Minimum size time window used for difficulty adjustments
        public const ulong LOWER_TIME_BOUND = BLOCK_TIME_WINDOW* 5 / 6;


        /// Error when computing the next difficulty adjustment.
        /// Computes the proof-of-work difficulty that the next block should comply
        /// with. Takes an iterator over past blocks, from latest (highest height) to
        /// oldest (lowest height). The iterator produces pairs of timestamp and
        /// difficulty for each block.
        /// 
        /// The difficulty calculation is based on both Digishield and GravityWave
        /// family of difficulty computation, coming to something very close to Zcash.
        /// The refence difficulty is an average of the difficulty over a window of
        /// 23 blocks. The corresponding timespan is calculated by using the
        /// difference between the median timestamps at the beginning and the end
        /// of the window.
        public Difficulty
            next_difficulty<T>(T cursor) //where T: IntoIterator<Item = Result<(u64, Difficulty), TargetError>>,
        {
            // Block times at the begining and end of the adjustment window, used to
            // calculate medians later.
            var window_begin = new List<DateTime>();
            var window_end = new List<DateTime>();

            // Sum of difficulties in the window, used to calculate the average later.
            var diff_sum = Difficulty.Zero();

            // Enumerating backward over blocks
//	for (n, head_info) in cursor.into_iter().enumerate()
//{
//    let m = n as u64;
//    let(ts, diff) = head_info ?;

//    // Sum each element in the adjustment window. In addition, retain
//    // timestamps within median windows (at ]start;start-11] and ]end;end-11]
//    // to later calculate medians.
//    if (m < DIFFICULTY_ADJUST_WINDOW) {
//        diff_sum = diff_sum + diff;

//        if (m < MEDIAN_TIME_WINDOW) {
//            window_begin.push(ts);
//        }
//    }
//    else if (m < DIFFICULTY_ADJUST_WINDOW + MEDIAN_TIME_WINDOW) {
//        window_end.push(ts);
//    }
//    else
//    {
//        break;
//    }
//}

            // Check we have enough blocks
            if (window_end.Count < (int) MEDIAN_TIME_WINDOW)
            {
                return Difficulty.minimum();
            }

            // Calculating time medians at the beginning and end of the window.
            window_begin.Sort();
            window_end.Sort();
            var begin_ts = window_begin[window_begin.Count / 2];
            var end_ts = window_end[window_end.Count / 2];

// Average difficulty and dampened average time
            var diff_avg = diff_sum.num / Difficulty.From_num(DIFFICULTY_ADJUST_WINDOW).num;
            var ts_damp = (3 * BLOCK_TIME_WINDOW + (ulong) (begin_ts - end_ts).TotalMilliseconds) / 4;

// Apply time bounds

            ulong adj_ts;
            if (ts_damp < LOWER_TIME_BOUND)
            {
                adj_ts = LOWER_TIME_BOUND;
            }
            else if (ts_damp > UPPER_TIME_BOUND)
            {
                adj_ts = UPPER_TIME_BOUND;
            }
            else
            {
                adj_ts = ts_damp;
            }
            
            var diffNum =
                Math.Max(diff_avg * Difficulty.From_num(BLOCK_TIME_WINDOW).num / Difficulty.From_num(adj_ts).num,
                    Difficulty.minimum().num);

            return Difficulty.From_num(diffNum);
        }

        // Consensus rule that collections of items are sorted lexicographically over the wire.
        //public void VerifySortOrder<T> {
            // Verify a collection of items is sorted as required.
            //fn verify_sort_order(&self) -> Result<(), ser::Error>;
        //}
}
}