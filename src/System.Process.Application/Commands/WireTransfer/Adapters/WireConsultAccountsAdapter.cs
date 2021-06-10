using System.Process.Infrastructure.Adapters;
using System.Proxy.Silverlake.Inquiry.Messages.Request;
using System.Collections.Generic;

namespace System.Process.Application.Commands.WireTransfer.Adapters
{
    public class WireConsultProcessAdapter : IAdapter<ProcessearchRequest, WireTransferAddRequest>
    {
        #region IAdapter implementation
        public ProcessearchRequest Adapt(WireTransferAddRequest input)
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