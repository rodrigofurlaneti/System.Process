using System.Process.Infrastructure.Adapters;
using System;

namespace System.Process.Application.Commands.WireTransfer.Adapters
{
    public class WireConsultReceiverAdapter : IAdapter<int, WireTransferAddRequest>
    {
        public int Adapt(WireTransferAddRequest request)
        {
            return Int32.Parse(request.ReceiverId);
        }
    }
}