using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Process.Infrastructure.Data;
using System.Phoenix.DataAccess.EntityFramework;
using System.Threading;
using System.Threading.Tasks;

namespace System.Process.Infrastructure.Repositories.EntityFramework
{
    public class ReceiverWriteRepository : GenericRepository<Receiver, DataContext>, IReceiverWriteRepository
    {
        public ReceiverWriteRepository(DataContext context) : base(context)
        {
        }

        public new async Task Add(Receiver receiver, CancellationToken cancelToken)
        {
            await AddAsync(receiver, cancelToken);
            Commit();
        }

        public void Remove(Receiver receiver, CancellationToken cancelToken)
        {
            Remove(receiver);
            Commit();
        }
    }
}
