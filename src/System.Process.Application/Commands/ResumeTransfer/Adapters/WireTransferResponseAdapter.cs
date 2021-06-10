using System.Linq;
using System.Process.Infrastructure.Adapters;
using System.Proxy.Silverlake.TranferWire.Messages.Response;

namespace System.Process.Application.Commands.ResumeTransfer.Adapters
{
    public class WireTransferResponseAdapter : IAdapter<TransferResponse, TransferWireAddResponse>
    {
        public TransferResponse Adapt(TransferWireAddResponse input)
        {
            return new TransferResponse
            {
                Status = input.ResponseStatus,
                Message = input.MessageRecordsInfo.FirstOrDefault().ErrorDescription
            };
        }
    }
}
