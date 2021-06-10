using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Process.Infrastructure.Data;
using System.Phoenix.DataAccess.EntityFramework;
using System.Collections.Generic;
using System.Linq;

namespace System.Process.Infrastructure.Repositories.EntityFramework
{
    public class TransferReadRepository : GenericRepository<Transfer, DataTransferContext>, ITransferReadRepository
    {
        public TransferReadRepository(DataTransferContext context) : base(context)
        {

        }

        public IList<Transfer> Find(string lifeCycleId)
        {
            return Find(r => r.LifeCycleId.Equals(lifeCycleId));
        }

        public Transfer FindByLifeCycleId(string lifeCycleId)
        {
            return Find(r => r.LifeCycleId.Equals(lifeCycleId)).FirstOrDefault();
        }
    }
}
