using Jose;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Process.Application.Clients.Cards;
using System.Process.Application.Commands.ReissueCard;
using System.Process.Domain.Constants;
using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Process.Domain.ValueObjects;
using System.Phoenix.Common.Exceptions;
using System.Proxy.Fis.CardReplace;
using System.Proxy.Fis.GetCard;
using System.Proxy.Fis.GetCard.Messages;
using System.Proxy.Fis.GetToken;
using System.Proxy.Fis.GetToken.Messages;
using System.Proxy.Fis.Messages;
using System.Proxy.Fis.ReissueCard;
using System.Proxy.Fis.ReissueCard.Messages;
using System.Proxy.Fis.ValueObjects;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Process.Application.Commands.CardReplace
{
    public class CardReplaceCommand : IRequestHandler<CardReplaceRequest, CardReplaceResponse>
    {
        #region Properties

        private ILogger<CardReplaceCommand> Logger { get; }
        private IGetTokenClient GetTokenClient { get; }
        private GetTokenParams GetTokenParams { get; }
        private IReissueCardClient ReissueCardClient { get; }
        private ICardReadRepository CardReadRepository { get; }
        private ICardService CardService { get; }
        private ProcessConfig ProcessConfig { get; }
        private IGetCardClient GetCardClient { get; }

        #endregion

        #region Constructor

        public CardReplaceCommand(
            ILogger<CardReplaceCommand> logger,
            IGetTokenClient getTokenClient,
            IOptions<GetTokenParams> getTokenParams,
            IReissueCardClient reissueCardClient,
            ICardReadRepository cardReadRepository,
            ICardService cardService,
            IOptions<ProcessConfig> ProcessConfig,
            IGetCardClient getCardClient)
        {
            Logger = logger;
            GetTokenClient = getTokenClient;
            GetTokenParams = getTokenParams.Value;
            ReissueCardClient = reissueCardClient;
            CardReadRepository = cardReadRepository;
            CardService = cardService;
            ProcessConfig = ProcessConfig.Value;
            GetCardClient = getCardClient;
        }

        #endregion

        #region INotificationHandler implementation

        public async Task<CardReplaceResponse> Handle(CardReplaceRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var card = CardReadRepository.Find(request.CardId).FirstOrDefault();

                if (card == null)
                {
                    throw new NotFoundException($"Card Id: {request.CardId} not found.");
                }

                if (request.CardId != card.CardId || request.Pan != card.LastFour)
                {
                    throw new UnprocessableEntityException("The info provided does not match the card information", "Please check the card information provided");
                }

                var token = await GetTokenClient.GetToken(GetTokenParams, cancellationToken);


                BaseResult<GetCardResult> resultGetCard = await GetCards(card, token, cancellationToken);
                

                ReissueCardParams paramsReissueCard = GenerateReissueCardParams(card, resultGetCard, request);

                var result = await ReissueCardClient.ReissueCardAsync(paramsReissueCard, token.Result.AccessToken, cancellationToken);

                if (!result.IsSuccess || result.Result.Metadata.Messages[0].Code != "00")
                {
                    throw new UnprocessableEntityException(result.Message);
                }

                card.ExpirationDate = result?.Result?.Entity?.ReissueDetails?.ExpirationDate;
                card.CardStatus = Constants.PendingActivation;
                card.Validated = 0;

                await CardService.HandleCardUpdate(card, cancellationToken);

                return await Task.FromResult(new CardReplaceResponse
                {
                    Success = true
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                throw;
            }
        }

        private ReissueCardParams GenerateReissueCardParams(Card card, BaseResult<GetCardResult> resultGetCard, CardReplaceRequest request)
        {
            ReissueCardRequest reissueRequest = new ReissueCardRequest
            {
                CardId = request.CardId,
                Pan = request.Pan,
                Address = new Domain.ValueObjects.Address
                {
                    Type = request.Address.Type
                },
                ReplaceReason = request.ReplaceReason
            };

            ReissueCardAdapter adapter = new ReissueCardAdapter(ProcessConfig);
            var paramsReissueCard = adapter.Adapt(
                new ReissueCardRequestDto
                {
                    Card = card,
                    Request = reissueRequest,
                    EncryptKey = ProcessConfig.EncryptKey
                }
            );

            paramsReissueCard.DateLastMaintained = resultGetCard?.Result?.Entity.DateLastMaintained;
            paramsReissueCard.DateLastUpdated = resultGetCard?.Result?.Entity.DateLastUpdated;
            paramsReissueCard.DateLastUsed = resultGetCard?.Result?.Entity.DateLastUsed;
            return paramsReissueCard;
        }

        private async Task<BaseResult<GetCardResult>> GetCards(Card card, BaseResult<GetTokenResult> token, CancellationToken cancellationToken)
        {
            GetCardParams getCardParams = new GetCardParams()
            {
                Pan = new Pan()
                {
                    Alias = string.Empty,
                    PlainText = string.Empty,
                    CipherText = JWT.Encode(card.Pan, Encoding.ASCII.GetBytes(ProcessConfig.EncryptKey), JweAlgorithm.A256GCMKW, JweEncryption.A256GCM)
                },
                Plastic = string.Empty
            };

            var resultGetCard = await GetCardClient.GetCardAsync(getCardParams, token.Result.AccessToken, cancellationToken);
            return resultGetCard;
        }

        #endregion
    }
}
