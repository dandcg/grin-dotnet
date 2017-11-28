namespace Grin.CoreImpl.Core.Pmmr
{
    /// Trait for an element of the tree that has a well-defined sum and hash that
    /// the tree can sum over
    public interface ISummable
    {

        /// Obtain the sum of the element
        Sum sum();
        /// Length of the Sum type when serialized. Can be used as a hint by
        /// underlying storages.
        uint sum_len();

    }
}