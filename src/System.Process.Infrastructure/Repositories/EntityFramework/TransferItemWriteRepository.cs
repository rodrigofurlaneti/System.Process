using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Process.Infrastructure.Data;
using System.Phoenix.DataAccess.EntityFramework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace System.Process.Infrastructure.Repositories.EntityFramework
{
    public class TransferItemWriteRepository : GenericRepository<TransferItem, DataTransferContext>, ITransferItemWriteRepository
    {
        public TransferItemWriteRepository(DataTransferContext context) : base(context)
        {

        }

        public new async Task Add(TransferItem transfer, CancellationToken cancelToken)
        {
            await AddAsync(transfer, cancelToken);
            Commit();
        }

        public void Remove(TransferItem transfer, CancellationToken cancelToken)
        {
            Remove(transfer);
            Commit();
        }

        public void Update(TransferItem transfer, CancellationToken cancelToken)
        {
            throw new NotImplementedException();
        }
    }
}
