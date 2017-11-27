using System;
using Grin.Core.Ser;
using Serilog;

namespace Grin.Core.Core.Hash
{
    /// A trait for types that have a canonical hash
    public static class Hashed
    {
        public static Hash hash<T>(this T self) where T : IWriteable
        {
            var hasher = HashWriter.Default();

            self.write(hasher);

            var ret = hasher.finalize();

            return new Hash(ret);
        }

        public static Hash hash_with<T,T2>(this T self, T2 other) where T : IWriteable where  T2 : IWriteable
        {
            var hasher = HashWriter.Default();

            self.write(hasher);

            Log.Verbose("Hashing with additional data");

            other.write(hasher);

            var ret = hasher.finalize();

            return new Hash(ret);
        }



        public static void verify_sort_order(this byte[] self)
        {

            throw new NotImplementedException();

            //match self.iter()
            //        .map(|item| item.hash())
            //    .collect::<Vec<_>>()
            //    .windows(2)
            //    .any(|pair| pair[0] > pair[1])
            //{
            //    true => Err(ser::Error::BadlySorted),
            //    false => Ok(()),
            //}
        }


    }
}