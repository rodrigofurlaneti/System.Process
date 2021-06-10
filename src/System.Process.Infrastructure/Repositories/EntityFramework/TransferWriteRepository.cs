using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Process.Infrastructure.Data;
using System.Phoenix.DataAccess.EntityFramework;
using System.Threading;
using System.Threading.Tasks;

namespace System.Process.Infrastructure.Repositories.EntityFramework
{
    public class TransferWriteRepository : GenericRepository<Transfer, DataTransferContext>, ITransferWriteRepository
    {
        public TransferWriteRepository(DataTransferContext context) : base(context)
        {

        }

        public new async Task Add(Transfer transfer, CancellationToken cancelToken)
        {
            await AddAsync(transfer, cancelToken);
            Commit();
        }

        public void Update(Transfer transfer, CancellationToken cancelToken)
        {
            Update(transfer);
            Commit();
        }

        public void Remove(Transfer transfer, CancellationToken cancelToken)
        {
            Remove(transfer);
            Commit();
        }
    }
}
