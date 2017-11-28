using System.IO;

namespace Grin.CoreImpl.Ser
{
    /// Utility wrapper for an underlying byte Writer. Defines higher level methods
    /// to write numbers, byte vectors, hashes, etc.
    public class BinWriter : WriterBase
    {
        public Stream Sink { get; }

        public BinWriter(Stream sink)
        {
            Sink = sink;
        }

        public override SerializationMode serialization_mode()
        {
            return SerializationMode.Full;
        }

        public override void write_fixed_bytes(byte[] bs)
        {
            //bs.Print();
            Sink.Write(bs, 0, bs.Length);
        }
    }
}