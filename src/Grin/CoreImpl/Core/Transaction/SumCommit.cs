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
        public Commitment Commit { get; private set; }

        /// Secp256k1 used to sum
        public Secp256K1 Secp { get; private set; }


        public void Write(IWriter writer)
        {
            Commit.WriteCommitment(writer);
        }

        public void Read(IReader reader)
        {
            Secp = Secp256K1.WithCaps(ContextFlag.Commit);
            Commit = Ser.Ser.ReadCommitment(reader);
        }


        public SumCommit Add(SumCommit other)

        {
            throw new NotImplementedException();
        }
    }
}