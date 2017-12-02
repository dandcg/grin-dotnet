using Grin.CoreImpl.Core.Transaction;

namespace Grin.CoreImpl.Ser
{
    public interface ICommitted
    {

        Input[] inputs_commited();

        Output[] outputs_committed();

        long Overage();

    }
}