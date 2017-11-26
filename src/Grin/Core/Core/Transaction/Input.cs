using Secp256k1Proxy;

namespace Grin.Core.Core
{
    /// A transaction input, mostly a reference to an output being spent by the
    /// transaction.
    public class Input : IReadable, IWriteable
    {
        public Input()
        {
            
        }

        public Input(Commitment value)
        {
            Value = value;
        }
 
        public Commitment Value { get; private set; }

        public void read(IReader reader)
        {
            Value = Ser.ReadCommitment(reader);
        }

        public void write(IWriter writer)
        {
            Value.WriteCommitment(writer);
        }

        public Input Clone()
        {
            return new Input(Value.Clone());
        }
    }
}