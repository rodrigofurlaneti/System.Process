using System.Process.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Process.Domain.Repositories
{
    public interface ITransferItemWriteRepository
    {
        Task Add(TransferItem transferItem, CancellationToken cancelToken);
        void Update(TransferItem transfer, CancellationToken cancelToken);
        void Remove(TransferItem transfer, CancellationToken cancelToken);
    }
}
