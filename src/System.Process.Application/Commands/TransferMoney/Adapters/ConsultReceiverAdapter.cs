using System.Process.Infrastructure.Adapters;
using System;

namespace System.Process.Application.Commands.TransferMoney.Adapters
{
    public class ConsultReceiverAdapter : IAdapter<int, TransferMoneyRequest>
    {
        public int Adapt(TransferMoneyRequest request)
        {
            return Int32.Parse(request.ReceiverId);
        }
    }
}