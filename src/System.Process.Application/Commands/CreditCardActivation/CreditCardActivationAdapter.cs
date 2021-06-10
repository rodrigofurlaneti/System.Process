using System.Process.Domain.Constants;
using System.Process.Domain.Entities;
using System.Process.Domain.ValueObjects;
using System.Process.Infrastructure.Adapters;
using System.Proxy.Rtdx.CardActivation.Messages;
using System.Proxy.Salesforce.UpdateAsset.Messages;

namespace System.Process.Application.Commands.CreditCardActivation
{
    public class CreditCardActivationAdapter : IAdapter<UpdateAssetParams, Card>,
        IAdapter<CardActivationParams, RtdxCardToken>
    {

        #region Properties

        private ProcessConfig ProcessConfig { get; set; }

        #endregion

        public CreditCardActivationAdapter(ProcessConfig ProcessConfig)
        {
            ProcessConfig = ProcessConfig;
        }

        #region IAdapter implemenation

        public UpdateAssetParams Adapt(Card input)
        {
            return new UpdateAssetParams
            {
                Id = input.AssetId,
                UpdateAssetBody = new UpdateAssetBody
                {
                    Status = Constants.ActiveStatus
                }
            };
        }
        public CardActivationParams Adapt(RtdxCardToken request)
        {
            var cardActivationParams = new CardActivationParams()
            {
                SecurityToken = request.SecurityToken,
                AccountNumber = request.Card.Pan,
                CardActivationStatus = ProcessConfig.CreditCardActivationStatus
            };
            return cardActivationParams;
        }

        #endregion
    }
}
