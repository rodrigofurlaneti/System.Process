using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Process.Domain.ValueObjects;
using System.Phoenix.Common.Exceptions;
using System.Proxy.Fis.GetToken;
using System.Proxy.Fis.GetToken.Messages;
using System.Proxy.Fis.LockUnlock;
using System.Proxy.Fis.LockUnlock.Messages;

namespace System.Process.Application.Commands.CreditCardChangeStatus
{
    public class CreditCardChangeStatusCommand : IRequestHandler<CreditCardChangeStatusRequest, CreditCardChangeStatusResponse>
    {
        #region Properties

        private ILogger<CreditCardChangeStatusCommand> Logger { get; }
        private ICardReadRepository CardReadRepository { get; }
        private ILockUnlockClient LockUnlockClient { get; }
        private GetTokenParams GetTokenParams { get; }
        private IGetTokenClient TokenClient { get; }
        public ProcessConfig ProcessConfig { get; }

        #endregion

        #region Constructor

        public CreditCardChangeStatusCommand(
            ILogger<CreditCardChangeStatusCommand> logger,
            ICardReadRepository cardReadRepository,
            ILockUnlockClient lockUnlockClient,
            IOptions<GetTokenParams> getTokenParams,
            IGetTokenClient tokenClient,
            IOptions<ProcessConfig> ProcessConfig)
        {
            Logger = logger;
            CardReadRepository = cardReadRepository;
            LockUnlockClient = lockUnlockClient;
            GetTokenParams = getTokenParams.Value;
            TokenClient = tokenClient;
            ProcessConfig = ProcessConfig.Value;
        }

        #endregion

        #region IRequestHandler
        public async Task<CreditCardChangeStatusResponse> Handle(CreditCardChangeStatusRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var token = await TokenClient.GetToken(GetTokenParams, cancellationToken);

                var creditCard = CardReadRepository.FindByCardId(request.CardId);

                if (creditCard == null)
                {
                    Logger.LogError($"Cannot find credit card in secure database", $"Card ID {request.CardId} not found");
                    throw new UnprocessableEntityException($"Cannot find credit card in secure database", $"Card ID {request.CardId} not found");
                }


                if (string.IsNullOrWhiteSpace(ProcessConfig.EncryptKey))
                {
                    Logger.LogError("Error on CreditCardChangeStatusCommand", $"EncryptKey is Null or White Spaece");
                    throw new UnprocessableEntityException("Error on CreditCardChangeStatusCommand", $"EncryptKey is Null or White Spaece");
                }

                LockUnlockParams lockUnlockParams = CreateLockUnlockParams(request, creditCard);
                var result = await LockUnlockClient.LockUnlockAsync(lockUnlockParams, token.Result.AccessToken, cancellationToken);

                return await Task.FromResult(new CreditCardChangeStatusResponse
                {
                    Status = request.Action
                });
            }
            catch (Exception ex)
            {
                var message = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Logger.LogInformation($"Error when trying to change credit card status - Description: { message }");
                throw new NotFoundException("Error when trying to change credit card status", $"Description: { message }");
            }
        }

        #endregion

        #region Private methods

        private LockUnlockParams CreateLockUnlockParams(CreditCardChangeStatusRequest request, Card creditCard)
        {

            FisChangeStatusCard fisChangeStatusCard = new FisChangeStatusCard()
            {
                Card = creditCard,
                Action = request.Action,
                EncryptKey = ProcessConfig.EncryptKey
            };

            var creditCardChangeStatusAdapter = new CreditCardChangeStatusAdapter();
            var lockUnlockParams = creditCardChangeStatusAdapter.Adapt(fisChangeStatusCard);
            return lockUnlockParams;
        }

        #endregion
    }
}
