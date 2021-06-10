using System;
using System.Threading;
using System.Threading.Tasks;

namespace System.Process.Worker.Clients.Product
{
    public interface IProductClient
    {
        Task<ProductMessage> Get(Guid id, CancellationToken cancellation);
    }
}
