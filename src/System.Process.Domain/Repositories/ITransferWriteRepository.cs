using System.Process.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace System.Process.Domain.Repositories
{
    public interface ITransferWriteRepository
    {
        Task Add(Transfer transfer, CancellationToken cancelToken);
        void Update(Transfer transfer, CancellationToken cancelToken);
        void Remove(Transfer transfer, CancellationToken cancelToken);
    }
}
