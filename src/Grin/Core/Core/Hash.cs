namespace Grin.Core.Core
{
    public class Hash
    {
        public byte[] Value { get; }




        private Hash(byte[] value)
        {
            Value = value;
        }

        /// A hash consisting of all zeroes, used as a sentinel. No known preimage.
        public static Hash ZERO_HASH()
        {
            return new Hash(new byte[32]);
        }
    }

    /// A trait for types that have a canonical hash
    public interface IHashed
    {

        Hash hash();
        /// Hash the object together with another writeable object
        Hash hash_with(IWriteable other) ;

    }




}
