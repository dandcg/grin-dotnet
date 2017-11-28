using Grin.CoreImpl.Ser;
using Secp256k1Proxy.Pedersen;

namespace Grin.CoreImpl.Core.Transaction
{
    /// A transaction input, mostly a reference to an output being spent by the
    /// transaction.
    public class Input : IReadable, IWriteable
    {
        public Input()
        {
            
        }

        public Input(Commitment commitment)
        {
            Commitment = commitment;
        }
 
        public Commitment Commitment { get; private set; }

        public void read(IReader reader)
        {
            Commitment = Ser.Ser.ReadCommitment(reader);
        }

        public void write(IWriter writer)
        {
            Commitment.WriteCommitment(writer);
        }

        public Input Clone()
        {
            return new Input(Commitment.Clone());
        }
    }
}