using System;
using Grin.CoreImpl.Ser;
using Serilog;

namespace Grin.CoreImpl.Core.Hash
{
    /// A trait for types that have a canonical hash
    public static class Hashed
    {
        public static Hash Hash<T>(this T self) where T : IWriteable
        {
            var hasher = HashWriter.Default();

            self.Write(hasher);

            var ret = hasher.FinalizeHash();

            return new Hash(ret);
        }

        public static Hash hash_with<T,T2>(this T self, T2 other) where T : IWriteable where  T2 : IWriteable
        {
            var hasher = HashWriter.Default();

            self.Write(hasher);

            Log.Verbose("Hashing with additional data");

            other.Write(hasher);

            var ret = hasher.FinalizeHash();

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