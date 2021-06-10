using System.Process.Infrastructure.Adapters;
using System;

namespace System.Process.Application.Commands.DeleteReceiver
{
    public class DeleteReceiverAdapter : IAdapter<int, DeleteReceiverRequest>
    {
        public int Adapt(DeleteReceiverRequest request)
        {
            return Int32.Parse(request.ReceiverId);
        }
    }
}
