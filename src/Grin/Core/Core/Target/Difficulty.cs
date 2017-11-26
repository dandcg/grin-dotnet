using System;
using System.Linq;
using Common;

namespace Grin.Core.Core
{
    //! Definition of the maximum target value a proof-of-work block hash can have
    //! and
    //! the related difficulty, defined as the maximum target divided by the hash.
    //!
    //! Note this is now wrapping a simple U64 now, but it's desirable to keep the
    //! wrapper in case the internal representation needs to change again

    public class Difficulty : IWriteable, IReadable
    {
        /// The target is the 32-bytes hash block hashes must be lower than.
        public static readonly byte[] MAX_TARGET = {0xf, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff};

        public ulong num { get; private set; }

        private Difficulty()
        {
        }

        private Difficulty(ulong num)
        {
            this.num = num;
        }

        public static Difficulty From_num(ulong num)
        {
            return new Difficulty(num);
        }

        /// Difficulty of zero, which is invalid (no target can be
        /// calculated from it) but very useful as a start for additions.
        public static Difficulty Zero()
        {
            return new Difficulty(0);
        }

        /// Difficulty of one, which is the minumum difficulty (when the hash
        /// equals the max target)
        /// TODO - is this the minimum dificulty or is consensus::MINIMUM_DIFFICULTY the minimum?
        public static Difficulty One()
        {
            return new Difficulty(1);
        }

        /// Minimum difficulty according to our consensus rules.
        public static Difficulty minimum()
        {
            return new Difficulty(Consensus.MINIMUM_DIFFICULTY); 
          
        }


public Difficulty Clone()
        {
            return new Difficulty(num);
        }

        public void write(IWriter writer)
        {
            writer.write_u64(num);
        }

        public void read(IReader reader)
        {
            num = reader.read_u64();
        }

        public static Difficulty readnew(IReader reader)
        {
            var d = new Difficulty();
            d.read(reader);
            return d;
        }


        /// Computes the difficulty from a hash. Divides the maximum target by the
        /// provided hash.
        public static Difficulty From_hash(Hash hash)
        {
            var mt = MAX_TARGET;
            mt.BigEndian();
            var max_target = BitConverter.ToUInt64(mt, 0);

            // Use the first 64 bits of the given hash
            var in_vec = hash.Value.Take(8).ToArray();
            in_vec.BigEndian();
            var num = BitConverter.ToUInt64(in_vec, 0);
            return new Difficulty(max_target / num);
        }
    }
}