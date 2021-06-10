using System.Linq;
using System.Process.Infrastructure.Adapters;
using System.Proxy.Silverlake.Transaction.Messages;

namespace System.Process.Application.Commands.ResumeTransfer.Adapters
{
    public class TransferResponseAdapter : IAdapter<TransferResponse, TransferAddResponse>
    {
        public TransferResponse Adapt(TransferAddResponse input)
        {
            return new TransferResponse
            {
                Status = input.ResponseStatus,
                Message = input.ResponseHeaderInfo.RecordInformationMessage.FirstOrDefault().ErrorDescription
            };
        }
    }
}
