using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Process.Application.Clients.Cards;
using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Process.Domain.ValueObjects;
using System.Phoenix.Common.Exceptions;
using System.Proxy.Fis.ChangeCardStatus;
using System.Proxy.Fis.GetToken;
using System.Proxy.Fis.GetToken.Messages;
using System.Proxy.Fis.LockUnlock;
using System.Proxy.Fis.LockUnlock.Messages;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace System.Process.Application.Commands.ChangeCardStatus
{
    public class ChangeCardStatusCommand : IRequestHandler<ChangeCardStatusRequest, ChangeCardStatusResponse>
    {
        #region Properties

        private ILogger<ChangeCardStatusCommand> Logger { get; }
        private IGetTokenClient GetTokenClient { get; }
        private GetTokenParams GetTokenParams { get; }
        private ILockUnlockClient LockUnlockClient { get; }
        private ICardReadRepository CardReadRepository { get; }
        private ICardService CardService { get; }
        public ProcessConfig ProcessConfig { get; }

        #endregion

        #region Constructor

        public ChangeCardStatusCommand(
            ILogger<ChangeCardStatusCommand> logger,
            IGetTokenClient getTokenClient,
            IOptions<GetTokenParams> getTokenParams,
            ILockUnlockClient lockUnlockClient,
            ICardReadRepository cardReadRepository,
            ICardService cardService,
            IOptions<ProcessConfig> ProcessConfig
            )
        {
            Logger = logger;
            GetTokenClient = getTokenClient;
            GetTokenParams = getTokenParams.Value;
            LockUnlockClient = lockUnlockClient;
            CardReadRepository = cardReadRepository;
            CardService = cardService;
            ProcessConfig = ProcessConfig.Value;
        }

        #endregion

        #region IRequestHandler implementation

        public async Task<ChangeCardStatusResponse> Handle(ChangeCardStatusRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var card = CardReadRepository.Find(request.CardId).FirstOrDefault();
                var action = String.Equals(request.Action.ToLower(), "lock") ? 1 : 0;

                if (card == null)
                {
                    throw new UnprocessableEntityException("No card was found");
                }

                if (card.CardStatus != "Active")
                {
                    throw new UnprocessableEntityException("Card is not active", "No change needed on this resource");
                }

                if (card.Locked == action)
                {
                    throw new UnprocessableEntityException($"Card is already on status {request.Action}", "No change needed on this resource");
                }


                if (string.IsNullOrWhiteSpace(ProcessConfig.EncryptKey))
                {
                    Logger.LogError("Error on ChangeCardStatusCommand", $"EncryptKey is Null or White Spaece");
                    throw new UnprocessableEntityException("Error on ChangeCardStatusCommand", $"EncryptKey is Null or White Spaece");
                }

                var token = await GetTokenClient.GetToken(GetTokenParams, cancellationToken);

                LockUnlockParams lockUnlockParams = CreateLockUnlockParams(request, card);
                var result = await LockUnlockClient.LockUnlockAsync(lockUnlockParams, token.Result.AccessToken, cancellationToken);

                if (!result.IsSuccess)
                {
                    throw new UnprocessableEntityException(result.Result?.Metadata.Messages[0].Text);
                }

                card.Locked = action;

                await CardService.HandleCardUpdate(card, cancellationToken);

                return await Task.FromResult(new ChangeCardStatusResponse
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

        #endregion

        #region Private methods

        private LockUnlockParams CreateLockUnlockParams(ChangeCardStatusRequest request, Card creditCard)
        {

            FisChangeStatusCard fisChangeStatusCard = new FisChangeStatusCard()
            {
                Card = creditCard,
                Action = request.Action,
                EncryptKey = ProcessConfig.EncryptKey
            };

            var creditCardChangeStatusAdapter = new ChangeCardStatusAdapter();
            var lockUnlockParams = creditCardChangeStatusAdapter.Adapt(fisChangeStatusCard);
            return lockUnlockParams;
        }

        #endregion
    }
}