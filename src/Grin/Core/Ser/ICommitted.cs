using System;
using Grin.Core.Core.Transaction;

namespace Grin.Core.Ser
{
    public interface ICommitted
    {

        Input[] inputs_commited();

        Output[] outputs_committed();

        Int64 overage();

    }
}