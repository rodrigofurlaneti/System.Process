using System.Linq;
using System.Process.Infrastructure.Adapters;
using System.Process.Infrastructure.Configs;
using System.Proxy.RdaAdmin.Common;
using System.Proxy.RdaAdmin.GetProcessCriteriaReference.Messages;
using System.Proxy.Salesforce.GetAccountInformations.Messages;
using System.Proxy.Salesforce.Messages;

namespace System.Process.Application.Commands.RemoteDepositCapture.Adapters
{
    public class GetProcessCriteriaReferenceAdapter : IAdapter<GetProcessCriteriaReferenceParams, BaseResult<QueryResult<GetAccountInformationsResponse>>>
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

        public GetProcessCriteriaReferenceParams Adapt(BaseResult<QueryResult<GetAccountInformationsResponse>> input)
        {
            return new GetProcessCriteriaReferenceParams
            {
                Criteria = new Criteria
                {
                    type = RdaConfig.GetProcessCriteriaReferenceId,
                    HomeBankingId = input.Result.Records.First().SystemId,
                    ReferenceId = input.Result.Records.First().CifId
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
