using System.Process.Domain.Constants;
using System.Process.Infrastructure.Adapters;
using System.Process.Infrastructure.Configs;
using System.Proxy.Salesforce.GetCreditCards.Message;
using System.Proxy.Salesforce.UpdateAsset.Messages;

namespace System.Process.Application.Commands.CreditCardCancellation
{
    public class CreditCardCancellationAdapter :
        IAdapter<GetCreditCardsParams, CreditCardCancellationRequest>,
        IAdapter<UpdateAssetParams, string>
    {
        #region Properties

        private RecordTypesConfig RecordTypesConfig { get; }

        #endregion

        #region Constructor

        public CreditCardCancellationAdapter(RecordTypesConfig recordTypesConfig)
        {
            RecordTypesConfig = recordTypesConfig;
        }

        public CreditCardCancellationAdapter()
        {

        }

        #endregion

        #region IAdapter

        public GetCreditCardsParams Adapt(CreditCardCancellationRequest input)
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
                    Status = Constants.DeclinedByCustomerStatus
                }
            };
        }

        #endregion
    }
}
