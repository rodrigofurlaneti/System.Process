using System.Process.Infrastructure.Adapters;
using System.Process.Infrastructure.Configs;
using System.Proxy.RdaAdmin.Common;
using System.Proxy.RdaAdmin.GetCustomersCriteria.Messages;

namespace System.Process.Application.Commands.RemoteDepositCapture.Adapters
{
    public class GetCustomersCriteriaAdapter : IAdapter<GetCustomersCriteriaParams, RemoteDepositCaptureRequest>
    {
        #region Properties
        private RdaCredentialsConfig RdaConfig { get; set; }

        #endregion

        #region Constructor

        public GetCustomersCriteriaAdapter(
            RdaCredentialsConfig rdaConfig
            )
        {
            RdaConfig = rdaConfig;
        }

        #endregion

        public GetCustomersCriteriaParams Adapt(RemoteDepositCaptureRequest input)
        {
            return new GetCustomersCriteriaParams
            {
                Criteria = new Criteria
                {
                    HomeBankingId = input.SystemId,
                    type = RdaConfig.TypeGetCustomersCriteria
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
