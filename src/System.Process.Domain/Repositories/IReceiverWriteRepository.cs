using System.Process.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace System.Process.Domain.Repositories
{
    public interface IReceiverWriteRepository
    {
        Task Add(Receiver receiver, CancellationToken cancelToken);
        void Remove(Receiver receiver, CancellationToken cancelToken);
    }
}
