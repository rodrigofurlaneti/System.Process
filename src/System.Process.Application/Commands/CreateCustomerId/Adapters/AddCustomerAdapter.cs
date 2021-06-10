using System.Linq;
using System.Process.Infrastructure.Adapters;
using System.Process.Infrastructure.Configs;
using System.Process.Infrastructure.Messages;
using System.Proxy.RdaAdmin.AddCustomer.Messages;

namespace System.Process.Application.Commands.CreateCustomerId.Adapters
{
    public class AddCustomerAdapter : IAdapter<AddCustomerParams, AccountMessage>
    {
        #region Properties
        private RdaCredentialsConfig RdaConfig { get; set; }

        #endregion

        #region Constructor

        public AddCustomerAdapter(
            RdaCredentialsConfig rdaConfig
            )
        {
            RdaConfig = rdaConfig;
        }

        #endregion

        public AddCustomerParams Adapt(AccountMessage input)
        {
            var result = new AddCustomerParams
            {
                Type = RdaConfig.TypeAddCustomer,
                HomeBankingId = input.SalesforceId,
                EmailAddress = input.Principals.FirstOrDefault(x=> x.MainIndicator = true).Contacts.FirstOrDefault(x => x.Type == "Email").Value,
                Username = input.Principals.FirstOrDefault(x => x.MainIndicator = true).FirstName, 
                FirstName = input.Principals.FirstOrDefault(x => x.MainIndicator = true).FirstName,
                LastName = input.Principals.FirstOrDefault(x => x.MainIndicator = true).LastName, 
                IsEnabled = true,
                Credentials = new Proxy.RdaAdmin.Common.Credentials
                {
                    EntityId = RdaConfig.Credentials.EntityId,
                    StoreId = RdaConfig.Credentials.StoreId,
                    StoreKey = RdaConfig.Credentials.StoreKey
                }
            };

            return result;
        }
    }
}
