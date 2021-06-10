using System.Linq;
using System.Process.Infrastructure.Adapters;
using System.Process.Infrastructure.Configs;
using System.Proxy.Rda.Authenticate.Messages;
using System.Proxy.RdaAdmin.GetCustomersCriteria.Messages;
using System.Proxy.RdaAdmin.Messages;

namespace System.Process.Application.Commands.RemoteDepositCapture.Adapters
{
    public class AuthenticateAdapter : IAdapter<AuthenticateRequest, AdminBaseResult<GetCustomersCriteriaResponse>>
    {
        private RdaCredentialsConfig Config { get; set; }

        public AuthenticateAdapter(RdaCredentialsConfig config)
        {
            Config = config;
        }

        public AuthenticateRequest Adapt(AdminBaseResult<GetCustomersCriteriaResponse> input)
        {
            return new AuthenticateRequest
            {
                Credentials = new Proxy.Rda.Authenticate.Messages.Credentials
                {
                    Type = Config.TypeAuthenticate,
                    HomeBankingId = input.Result.Customers.First().HomeBankingId,
                    PhoneKey = input.Result.Customers.First().HomeBankingId
                },
                DeviceTracking = new DeviceTracking
                {
                    AppBundleId = "com.testbank.deposit",
                    AppVersion = "1.0.1433",
                    DeviceModel = "iPhone 6s",
                    DeviceSystemName = "iPhone OS",
                    DeviceSystemVersion = "9.3.2",
                    Vendor = "System"
                }
            };
        }
    }
}
