using System.Process.Application.Commands.AchTransferMoney;
using System.Process.Infrastructure.Adapters;
using System.Proxy.Silverlake.Inquiry.Messages.Request;
using System.Collections.Generic;

namespace System.Process.Application.Commands.ACHTransferMoney.Adapters
{
    public class AchConsultProcessAdapter : IAdapter<ProcessearchRequest, AchTransferMoneyRequest>
    {
        #region IAdapter implementation
        public ProcessearchRequest Adapt(AchTransferMoneyRequest input)
        {
            return new ProcessearchRequest
            {
                MaximumRecords = 4000,
                CustomerId = input.CustomerId,
                IncXtendElemArray = new List<IncXtendElemInfoRequest>()
            };
        }
        #endregion
    }
}