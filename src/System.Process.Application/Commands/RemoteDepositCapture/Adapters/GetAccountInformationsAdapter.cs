using System.Process.Infrastructure.Adapters;
using System.Proxy.Salesforce.GetAccountInformations.Messages;

namespace System.Process.Application.Commands.RemoteDepositCapture.Adapters
{
    public class GetAccountInformationsAdapter : IAdapter<GetAccountInformationsParams, RemoteDepositCaptureRequest>
    {
        public GetAccountInformationsParams Adapt(RemoteDepositCaptureRequest input)
        {
            return new GetAccountInformationsParams
            {
                SystemId = input.SystemId
            };
        }
    }
}
