using System.Process.Application.Commands.AchTransferMoney;
using System.Process.Infrastructure.Adapters;
using System;

namespace System.Process.Application.Commands.ACHTransferMoney.Adapters
{
    public class AchConsultReceiverAdapter : IAdapter<int, AchTransferMoneyRequest>
    {
        public int Adapt(AchTransferMoneyRequest request)
        {
            return Int32.Parse(request.ReceiverId);
        }
    }
}