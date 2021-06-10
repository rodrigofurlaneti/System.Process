using System.Process.Domain.Constants;
using System.Process.Infrastructure.Adapters;
using System.Process.Infrastructure.Configs;
using System.Proxy.Salesforce.GetCreditCards.Message;
using System.Proxy.Salesforce.UpdateAsset.Messages;

namespace System.Process.Application.Commands.CreditCardAgreement
{
    public class CreditCardAgreementAdapter :
        IAdapter<GetCreditCardsParams, CreditCardAgreementRequest>,
        IAdapter<UpdateAssetParams, string>
    {
        #region Properties

        private RecordTypesConfig RecordTypesConfig { get; }

        #endregion

        #region Constructor

        public CreditCardAgreementAdapter(RecordTypesConfig recordTypesConfig)
        {
            RecordTypesConfig = recordTypesConfig;
        }

        public CreditCardAgreementAdapter()
        {

        }

        #endregion

        #region IAdapter implemenation

        public GetCreditCardsParams Adapt(CreditCardAgreementRequest input)
        {
            return new GetCreditCardsParams
            {
                SystemId = input.SystemId,
                RecordTypeId = RecordTypesConfig.AssetCreditCard
            };
        }

        public UpdateAssetParams Adapt(string input)
        {
            return new UpdateAssetParams
            {
                Id = input,
                UpdateAssetBody = new UpdateAssetBody
                {
                    Status = Constants.PendingActivationStatus
                }
            };
        }

        #endregion
    }
}
