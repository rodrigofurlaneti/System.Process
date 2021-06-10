using System;
using System.Linq;
using System.Process.Domain.Enums;
using System.Process.Domain.ValueObjects;
using System.Process.Infrastructure.Adapters;
using System.Process.Infrastructure.Configs;
using System.Process.Infrastructure.Messages;
using System.Proxy.RdaAdmin.AddAccount.Messages;

namespace System.Process.Application.Commands.CreateCustomerId.Adapters
{
    public class AddAccountAdapter : IAdapter<AddAccountParams, AccountMessage>
    {
        #region Properties
        private RdaCredentialsConfig RdaConfig { get; set; }

        #endregion

        #region Constructor

        public AddAccountAdapter
            (
            RdaCredentialsConfig rdaConfig
            )
        {
            RdaConfig = rdaConfig;
        }

        #endregion

        public AddAccountParams Adapt(AccountMessage input)
        {
            var account = input.Process.FirstOrDefault(x => x.Origin == OriginAccount.S);
            return new AddAccountParams
            {
                RequestType = RdaConfig.TypeAddAccount,
                HomeBankingId = input.SalesforceId,
                AccountNumber  = account.Number,
                ReferenceId = input.BusinessCif,
                Name = input.Principals.FirstOrDefault(x => x.MainIndicator = true).FirstName,
                RoutingNumber = account.RoutingNumber,
                IsEnabled = true,
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
