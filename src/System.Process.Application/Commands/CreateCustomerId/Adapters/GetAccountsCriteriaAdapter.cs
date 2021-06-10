using System.Process.Infrastructure.Adapters;
using System.Proxy.RdaAdmin.GetProcessCriteria.Messages;

namespace System.Process.Application.Commands.CreateCustomerId.Adapters
{
    public class GetProcessCriteriaAdapter : IAdapter<GetProcessCriteriaParams, CreateCustomerIdNotification>
    {
        public GetProcessCriteriaParams Adapt(CreateCustomerIdNotification input)
        {
            return new GetProcessCriteriaParams
            {
                Criteria = new Proxy.RdaAdmin.Common.Criteria
                {
                    HomeBankingId = input.MessageContent.Payload.BankAccount.AccountNumber
                }
            };
        }
    }
}
