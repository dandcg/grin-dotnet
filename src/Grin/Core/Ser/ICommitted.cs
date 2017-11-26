using System;
using Grin.Core.Core;

namespace Grin.Core
{
    public interface ICommitted
    {

        Input[] inputs_commited();

        Output[] outputs_committed();

        Int64 overage();

    }
}