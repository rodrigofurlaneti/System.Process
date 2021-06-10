using System.Process.Infrastructure.Adapters;
using System.Process.Infrastructure.Configs;
using System.Process.Infrastructure.Messages;
using System.Proxy.RdaAdmin.UpdateAccount.Messages;
using System.Proxy.RdaAdmin.UpdateAccount.Messages.Request;

namespace System.Process.Application.Commands.CreateCustomerId.Adapters
{
    public class UpdateAccountAdapter : IAdapter<UpdateAccountParams, AccountMessage>
    {
        #region Properties
        private RdaCredentialsConfig RdaConfig { get; set; }

        #endregion

        #region Constructor

        public UpdateAccountAdapter
            (
            RdaCredentialsConfig rdaConfig
            )
        {
            RdaConfig = rdaConfig;
        }

        #endregion

        public UpdateAccountParams Adapt(AccountMessage input)
        {
            return new UpdateAccountParams
            {
                AccountNumber = input.BankAccount.AccountNumber,
                IsEnabled = true,
                Criteria = new UpdateAccountCriteria
                {
                    HomeBankingId = input.SalesforceId,
                    ReferenceId = input.BusinessCif
                },
                Credentials = new Proxy.RdaAdmin.Common.Credentials
                {
                    EntityId = RdaConfig.Credentials.EntityId,
                    StoreId = RdaConfig.Credentials.StoreId,
                    StoreKey = RdaConfig.Credentials.StoreKey
                }
            };
        }
    }
}
