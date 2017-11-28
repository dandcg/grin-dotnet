using System;
using Grin.CoreImpl.Ser;
using Secp256k1Proxy.Lib;
using Secp256k1Proxy.Pedersen;

namespace Grin.CoreImpl.Core.Transaction
{
    /// Wrapper to Output commitments to provide the Summable trait.
    public class SumCommit : IWriteable, IReadable
    {
        /// Output commitment
        public Commitment commit { get; private set; }

        /// Secp256k1 used to sum
        public Secp256k1 secp { get; private set; }


        public void write(IWriter writer)
        {
            commit.WriteCommitment(writer);
        }

        public void read(IReader reader)
        {
            secp = Secp256k1.WithCaps(ContextFlag.Commit);
            commit = Ser.Ser.ReadCommitment(reader);
        }


        public SumCommit add(SumCommit other)

        {
            throw new NotImplementedException();
        }
    }
}