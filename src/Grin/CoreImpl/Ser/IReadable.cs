namespace Grin.CoreImpl.Ser
{
    /// Trait that every type that can be deserialized from binary must implement.
    /// Reads directly to a Reader, a utility type thinly wrapping an
    /// underlying Read implementation.
    public interface IReadable
    {
        /// Reads the data necessary to this Readable from the provided reader
        void read(IReader reader);
    }
}