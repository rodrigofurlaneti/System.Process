using System.Process.Domain.Entities;
using System.Process.Infrastructure.Adapters;
using System.Proxy.Rda.Authenticate.Messages;

namespace System.Process.Application.Commands.ResumeTransfer.Adapters
{
    public class AuthenticateRdcAdapter : IAdapter<AuthenticateRequest, Transfer>
    {
        public AuthenticateRequest Adapt(Transfer input)
        {
            return new AuthenticateRequest
            {
                Credentials = new Credentials
                {
                    HomeBankingId = input.SystemId,
                    PhoneKey = input.SystemId
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
