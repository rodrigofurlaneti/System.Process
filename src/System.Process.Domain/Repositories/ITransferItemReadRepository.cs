using System.Process.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace System.Process.Domain.Repositories
{
    public interface ITransferItemReadRepository
    {
        IList<TransferItem> Find(string lifeCycleId);
    }
}
