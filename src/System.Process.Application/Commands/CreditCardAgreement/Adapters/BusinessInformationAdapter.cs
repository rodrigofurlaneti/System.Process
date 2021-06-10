using System.Process.Infrastructure.Adapters;
using System.Proxy.Salesforce.GetBusinessInformation.Message;

namespace System.Process.Application.Commands.CreditCardAgreement
{
    public class BusinessInformationAdapter : IAdapter<GetBusinessInformationParams, CreditCardAgreementRequest>
    {
        public GetBusinessInformationParams Adapt(CreditCardAgreementRequest input)
        {
            return new GetBusinessInformationParams
            {
                SystemId = input.SystemId
            };
        }
    }
}
