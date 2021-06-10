using System.Process.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace System.Process.Domain.Repositories
{
    public interface ICardWriteRepository
    {
        Task Add(Card card, CancellationToken cancelToken);
        void Update(Card card, CancellationToken cancelToken);
        void Remove(Card card, CancellationToken cancelToken);
    }
}
