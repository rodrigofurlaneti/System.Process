using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Process.Application.Clients.Cards;
using System.Process.Application.Commands.ActivateCard;
using System.Process.Domain.Entities;
using System.Process.Domain.ValueObjects;
using System.Phoenix.Common.Exceptions;
using System.Proxy.Fis.ActivateCard;
using System.Proxy.Fis.ChangeCardPin;
using System.Proxy.Fis.GetToken;
using System.Proxy.Fis.GetToken.Messages;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace System.Process.Application.Commands.ChangeCardPin
{
    public class ChangeCardPinCommand : IRequestHandler<ChangeCardPinRequest, ChangeCardPinResponse>
    {
        #region Properties

        private ILogger<ChangeCardPinCommand> Logger { get; }
        private IGetTokenClient GetTokenClient { get; }
        private GetTokenParams GetTokenParams { get; }
        private IChangeCardPinClient ChangeCardPinClient { get; }
        private IActivateCardClient ActivateCardClient { get; }
        private ICardService CardService { get; }
        public ProcessConfig ProcessConfig { get; }

        #endregion

        #region Constructor

        public ChangeCardPinCommand(
            ILogger<ChangeCardPinCommand> logger,
            IGetTokenClient getTokenClient,
            IOptions<GetTokenParams> getTokenParams,
            IChangeCardPinClient changeCardPinClient,
            IActivateCardClient activateCardClient,
            ICardService cardService,
            IOptions<ProcessConfig> ProcessConfig
            )
        {
            Logger = logger;
            GetTokenClient = getTokenClient;
            GetTokenParams = getTokenParams.Value;
            ChangeCardPinClient = changeCardPinClient;
            ActivateCardClient = activateCardClient;
            CardService = cardService;
            ProcessConfig = ProcessConfig.Value;
        }

        #endregion

        #region IRequestHandler implementation

        public async Task<ChangeCardPinResponse> Handle(ChangeCardPinRequest request, CancellationToken cancellationToken)
        {
            try
            {
                Logger.LogInformation($"Start changeCarPinCommand");
                Card card;
                // fetch pan
                try
                {
                    card = CardService.FindByCardId(request.CustomerId, request.CardId, cancellationToken).Result[0];
                }
                catch (Exception ex)
                {
                    throw new UnprocessableEntityException("No card was found", ex.Message, ex);
                }

                // validate activateCard verification
                if (card.Validated != 1)
                {
                    throw new UnprocessableEntityException("This resource needs Card Activation before Pin Creation", "This resource needs Card Activation before Pin Creation");
                }

                Logger.LogInformation($"Start GetToken FIS");
                // change pin
                var token = await GetTokenClient.GetToken(GetTokenParams, cancellationToken);
                var changePinAdapter = new ChangeCardPinAdapter();

                if (string.IsNullOrWhiteSpace(ProcessConfig.EncryptKey))
                {
                    Logger.LogError("Error on ChangeCardPinCommand", $"EncryptKey is Null or White Spaece");
                    throw new UnprocessableEntityException("Error on ChangeCardPinCommand", $"EncryptKey is Null or White Spaece");
                }

                Logger.LogInformation($"Start Adapt ChangeCardPinRequestDto");
                var paramsChangePin = changePinAdapter.Adapt(new ChangeCardPinRequestDto
                {
                    Request = request,
                    Card = card,
                    EncryptKey = ProcessConfig.EncryptKey
                }
                );

                Logger.LogInformation($"Start ChangeCardPinAsync FIS");
                var changePinResult = await ChangeCardPinClient.ChangeCardPinAsync(paramsChangePin, token.Result.AccessToken, cancellationToken);

                if (!changePinResult.IsSuccess)
                {
                    throw new UnprocessableEntityException(changePinResult.Message);
                }

                // activate card
                var activateCardAdapter = new ActivateCardAdapter(ProcessConfig);
                Logger.LogInformation($"Start Adapt paramsActivateCard");
                var paramsActivateCard = activateCardAdapter.Adapt(card);

                Logger.LogInformation($"Start ActivateCard FIS");
                var activateCardResult = await ActivateCardClient.ActivateCard(paramsActivateCard, token.Result.AccessToken, cancellationToken);

                if (!activateCardResult.IsSuccess)
                {
                    throw new UnprocessableEntityException(changePinResult.Message);
                }

                // consolidate CardsService
                card.CardStatus = ProcessConfig.CardStatus.Active;

                await CardService.HandleCardUpdate(card, cancellationToken);

                return new ChangeCardPinResponse
                {
                    Code = activateCardResult.Result.Metadata.Messages[0].Code,
                    Message = activateCardResult.Result.Metadata.Messages[0].Text
                };
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, ex.Message);
                throw new UnprocessableEntityException(ex.Message, ex);
            }
        }

        #endregion
    }
}