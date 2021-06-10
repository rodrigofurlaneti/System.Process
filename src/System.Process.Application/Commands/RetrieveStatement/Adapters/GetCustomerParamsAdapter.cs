using System.Process.Domain.Entities;
using System.Proxy.Salesforce.GetCustomerInformations.Message;

namespace System.Process.Application.Commands.RetrieveStatement.Adapters
{
    public class GetCustomerParamsAdapter
    {
        public GetCustomerParams AdaptCustomerParams(Customer customer)
        {
            return new GetCustomerParams
            {
                SystemId = customer.SalesforceId
            };
        }

    }
}
