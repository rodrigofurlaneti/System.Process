using System.Process.Infrastructure.Adapters;
using System.Process.Infrastructure.Configs;
using System.Process.Infrastructure.Messages;
using System.Proxy.RdaAdmin.Common;
using System.Proxy.RdaAdmin.GetProcessCriteriaReference.Messages;

namespace System.Process.Application.Commands.CreateCustomerId.Adapters
{
    public class GetProcessCriteriaReferenceAdapter : IAdapter<GetProcessCriteriaReferenceParams, AccountMessage>
    {
        #region Properties
        private RdaCredentialsConfig RdaConfig { get; set; }

        #endregion

        #region Constructor

        public GetProcessCriteriaReferenceAdapter
            (
            RdaCredentialsConfig rdaConfig
            )
        {
            RdaConfig = rdaConfig;
        }

        #endregion

        public GetProcessCriteriaReferenceParams Adapt(AccountMessage input)
        {
            return new GetProcessCriteriaReferenceParams
            {
                Criteria = new Criteria
                {
                    type = RdaConfig.GetProcessCriteriaReferenceId,
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
