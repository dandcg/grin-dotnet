using Grin.Core.Ser;
using Secp256k1Proxy;
using Secp256k1Proxy.Pedersen;

namespace Grin.Core.Core.Transaction
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
            Value = Ser.Ser.ReadCommitment(reader);
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