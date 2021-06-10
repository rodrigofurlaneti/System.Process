using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Process.Infrastructure.Data;
using System.Phoenix.DataAccess.EntityFramework;
using System.Collections.Generic;

namespace System.Process.Infrastructure.Repositories.EntityFramework
{
    public class TransferItemReadRepository : GenericRepository<TransferItem, DataTransferContext>, ITransferItemReadRepository
    {
        public TransferItemReadRepository(DataTransferContext context) : base(context)
        {

        }

        public IList<TransferItem> Find(string lifeCycleId)
        {
            return Find(r => r.LifeCycleId.Equals(lifeCycleId));
        }
    }
}
