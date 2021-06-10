using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Process.Application.Clients.Cards;
using System.Process.Application.Commands.CreateAccount;
using System.Process.Domain.Entities;
using System.Phoenix.Common.Exceptions;
using System.Proxy.Salesforce.GetToken;
using System.Proxy.Salesforce.GetToken.Messages;
using System.Proxy.Salesforce.UpdateAsset;
using System;
using System.Threading;
using System.Threading.Tasks;
using SalesforceToken = System.Proxy.Salesforce.GetToken;

namespace System.Process.Application.Commands.ActivateCard
{
    public class ActivateCardCommand : IRequestHandler<ActivateCardRequest, ActivateCardResponse>
    {
        #region Properties

        private ILogger<ActivateCardCommand> Logger { get; }
        private ICardService CardService { get; }
        private IGetTokenClient TokenClient { get; }
        private GetTokenParams ConfigSalesforce { get; }
        private IUpdateAssetClient UpdateAssetClient { get; }

        #endregion

        #region Constructor

        public ActivateCardCommand(
            ILogger<ActivateCardCommand> logger,
            ICardService cardService,
            IGetTokenClient tokenClient,
            IOptions<GetTokenParams> getTokenParams,
            IUpdateAssetClient updateAssetClient
            )
        {
            Logger = logger;
            CardService = cardService;
            TokenClient = tokenClient;
            ConfigSalesforce = getTokenParams.Value;
            UpdateAssetClient = updateAssetClient;
        }

        #endregion

        #region IRequestHandler implementation

        public async Task<ActivateCardResponse> Handle(ActivateCardRequest request, CancellationToken cancellationToken)
        {
            Card card;
            // fetch pan
            try
            {
                card = CardService.FindByCardId(request.CustomerId, request.CardId, cancellationToken).Result[0];
            }
            catch (Exception ex)
            {
                throw new UnprocessableEntityException(
                    "The info provided does not match the info on a card",
                    "Some info on the payload is wrong or incomplete",
                    ex
                );
            }

            // validate pending activation
            if (card.CardStatus == "Active")
            {
                throw new UnprocessableEntityException("Card is already active", "No change needed on this resource");
            }

            // validate request info
            if (
                request.CardId != card.CardId ||
                String.Compare(request.ExpireDate, card.ExpirationDate, true) != 0 ||
                request.Pan != card.LastFour
            )
            {
                throw new UnprocessableEntityException(
                    "The info provided does not match the info on a card",
                    "Some info on the payload is wrong or incomplete"
                );
            }

            // consolidate CardsService
            card.Validated = 1;
            await CardService.HandleCardUpdate(card, cancellationToken);

            await UpdateAssetSalesForce(card, cancellationToken);

            return new ActivateCardResponse
            {
                Code = "00",
                Message = "SUCCESS"
            };

        }

        private async Task UpdateAssetSalesForce(Card card, CancellationToken cancellationToken)
        {
            try
            {
                var authToken = await TokenClient.GetToken(ConfigSalesforce, cancellationToken);
                var updateAdapter = new UpdateAssetAdapter();
                var assetParams = updateAdapter.Adapt(card.AssetId);
                var response = await UpdateAssetClient.UpdateAsset(assetParams, authToken.Result.AccessToken, cancellationToken);

                if (!response.IsSuccess)
                {
                    throw new UnprocessableEntityException("Cannot update asset in Salesforce", response.Message);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error during the RegisterDebitCard request [{card.CustomerId}]");
                throw new UnprocessableEntityException("Cannot update asset in Salesforce", ex);
            }
        }

        #endregion
    }
}