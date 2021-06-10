using System.Linq;
using System.Process.Infrastructure.Adapters;
using System.Proxy.Rda.Messages;
using System.Proxy.Rda.UpdateBatch.Messages;

namespace System.Process.Application.Commands.ResumeTransfer.Adapters
{
    public class RdcTransferResponseAdapter : IAdapter<TransferResponse, BaseResult<UpdateBatchResponse>>
    {
        public TransferResponse Adapt(BaseResult<UpdateBatchResponse> input)
        {
            return new TransferResponse
            {
                Status = input.Result.ResultMessage,
                Message = input?.Result?.ValidationResults?.FirstOrDefault()?.Message
            };
        }
    }
}
