using System;
using System.Linq;
using Common;
using Grin.CoreImpl.Ser;

namespace Grin.CoreImpl.Core.Target
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
        public static readonly byte[] MaxTarget = {0xf, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff};

        public ulong Num { get; private set; }

        private Difficulty()
        {
        }

        private Difficulty(ulong num)
        {
            Num = num;
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
        public static Difficulty Minimum()
        {
            return new Difficulty(Consensus.MinimumDifficulty); 
          
        }

        /// Convert a `u32` into a `Difficulty`
        public static Difficulty From_num(ulong num)
        {
            return new Difficulty(num);
        }




        /// Computes the difficulty from a hash. Divides the maximum target by the
        /// provided hash.
        public static Difficulty From_hash(Hash.Hash hash)
        {
            var mt = MaxTarget;
            mt.BigEndian();
            var maxTarget = BitConverter.ToUInt64(mt, 0);

            // Use the first 64 bits of the given hash
            var inVec = hash.Value.Take(8).ToArray();
            inVec.BigEndian();
            var num = BitConverter.ToUInt64(inVec, 0);
            return new Difficulty(maxTarget / num);
        }


        /// Converts the difficulty into a u64
        public ulong into_num()
        {
            return Num;
        }


    public Difficulty Clone()
        {
            return new Difficulty(Num);
        }

        public void Write(IWriter writer)
        {
            writer.write_u64(Num);
        }

        public void Read(IReader reader)
        {
            Num = reader.read_u64();
        }

        public static Difficulty Readnew(IReader reader)
        {
            var d = new Difficulty();
            d.Read(reader);
            return d;
        }

    }
}