namespace Grin.Core
{
    /// Signal to a serializable object how much of its data should be serialized
    public enum SerializationMode
    {
        /// Serialize everything sufficiently to fully reconstruct the object
        Full,

        /// Serialize the data that defines the object
        Hash,

        /// Serialize everything that a signer of the object should know
        SigHash
    }
}