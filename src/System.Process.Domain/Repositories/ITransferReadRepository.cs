using System.Process.Domain.Entities;
using System.Collections.Generic;

namespace System.Process.Domain.Repositories
{
    public interface ITransferReadRepository
    {
        IList<Transfer> Find(string id);
        Transfer FindByLifeCycleId(string id);
    }
}
