using System.Process.Domain.Entities;
using System.Process.Infrastructure.Adapters;
using System.Proxy.Silverlake.Transaction.Messages.Request;

namespace System.Process.Application.Commands.ResumeTransfer.Adapters
{
    public class StopCheckCancelAdapter : IAdapter<StopCheckCancelRequest, Transfer>
    {
        public StopCheckCancelRequest Adapt(Transfer input)
        {
            return new StopCheckCancelRequest
            {
                AccountId = new Proxy.Silverlake.Transaction.Common.AccountId
                {
                    AccountNumber = input.AccountFromNumber,
                    AccountType = input.AccountFromType
                }, 
                StopSequence = input.StopSequence
            };
        }
    }
}
