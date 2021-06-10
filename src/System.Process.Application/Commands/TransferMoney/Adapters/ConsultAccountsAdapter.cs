using System.Process.Infrastructure.Adapters;
using System.Proxy.Silverlake.Inquiry.Messages.Request;
using System.Collections.Generic;

namespace System.Process.Application.Commands.TransferMoney.Adapters
{
    public class ConsultProcessAdapter : IAdapter<ProcessearchRequest, TransferMoneyRequest>
    {
        #region IAdapter implementation
        public ProcessearchRequest Adapt(TransferMoneyRequest input)
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