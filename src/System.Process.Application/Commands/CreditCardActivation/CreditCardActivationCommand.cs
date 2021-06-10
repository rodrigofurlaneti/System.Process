using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Process.Domain.Constants;
using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Process.Domain.ValueObjects;
using System.Phoenix.Common.Exceptions;
using System.Proxy.Rtdx.CardActivation;
using System.Proxy.Rtdx.GetToken;
using System.Proxy.Salesforce.GetToken;
using System.Proxy.Salesforce.GetToken.Messages;
using System.Proxy.Salesforce.UpdateAsset;
using RtdxToken = System.Proxy.Rtdx.GetToken;

namespace System.Process.Application.Commands.CreditCardActivation
{
    public class CreditCardActivationCommand : IRequestHandler<CreditCardActivationRequest, CreditCardActivationResponse>
    {
        #region Properties

        private ILogger<CreditCardActivationCommand> Logger { get; }
        private ICardReadRepository CardReadRepository { get; }
        private IUpdateAssetClient UpdateAssetClient { get; }
        private IGetTokenClient TokenClient { get; }
        private IGetTokenOperation TokenOperation { get; }
        private ICardActivationOperation CardActivationOperation { get; }
        private GetTokenParams ConfigSalesforce { get; }
        private RtdxToken.Messages.GetTokenParams RtdxTokenParams { get; }
        private ICardWriteRepository CardWriteRepository { get; }
        public ProcessConfig ProcessConfig { get; }

        #endregion

        #region Constructor

        public CreditCardActivationCommand(
            ILogger<CreditCardActivationCommand> logger,
            ICardReadRepository cardReadRepository,
            IUpdateAssetClient updateAssetClient,
            IGetTokenClient tokenClient,
            IGetTokenOperation tokenOperation,
            ICardActivationOperation cardActivationOperation,
            IOptions<GetTokenParams> getTokenParams,
            IOptions<RtdxToken.Messages.GetTokenParams> rtdxTokenParams,
            ICardWriteRepository cardWriteRepository,
            IOptions<ProcessConfig> ProcessConfig
            )
        {
            Logger = logger;
            CardReadRepository = cardReadRepository;
            UpdateAssetClient = updateAssetClient;
            TokenClient = tokenClient;
            TokenOperation = tokenOperation;
            CardActivationOperation = cardActivationOperation;
            ConfigSalesforce = getTokenParams.Value;
            RtdxTokenParams = rtdxTokenParams.Value;
            CardWriteRepository = cardWriteRepository;
            ProcessConfig = ProcessConfig.Value;
        }

        #endregion

        #region IRequestHandler

        public async Task<CreditCardActivationResponse> Handle(CreditCardActivationRequest request, CancellationToken cancellationToken)
        {
            try
            {

                var creditCard = CreditCardValidation(request);
                await CardActitvationRtdx(creditCard, cancellationToken);
                await UpdateCreditCardStatus(creditCard, cancellationToken);
                SaveCreditCardOnDatabase(creditCard, cancellationToken);

                return await Task.FromResult(new CreditCardActivationResponse
                {
                    Success = true
                });
            }
            catch (Exception ex)
            {
                var message = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Logger.LogInformation($"Error when trying to active credit card  - Description: { message }");
                throw;
            }
        }

        #endregion

        #region Methods

        private Card CreditCardValidation(CreditCardActivationRequest request)
        {
            try
            {
                var creditCard = CardReadRepository.FindByCardId(request.CardId);

                var formatedExpirateDate = FormatExpirationDateToYYMM(request.ExpireDate);

                if (!creditCard.LastFour.Equals(request.LastFour) && creditCard.ExpirationDate.Equals(formatedExpirateDate))
                {
                    Logger.LogError($"Cannot find credit card in database", $"The last 4 digits {request.LastFour} doesn't match to card ID {request.CardId}");
                    throw new UnprocessableEntityException($"Cannot find credit card in database", $"The last 4 digits {request.LastFour} doesn't match to card ID {request.CardId}");
                }

                if (creditCard.CardStatus != Constants.PendingActivationStatus)
                {
                    Logger.LogError("The credit card status must be 'Pending Activation'", $"The credit card status is {creditCard.CardStatus}, card ID {request.CardId}");
                    throw new UnprocessableEntityException("The credit card status must be 'Pending Activation'", $"The credit card status is {creditCard.CardStatus}, card ID {request.CardId}");
                }

                return creditCard;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Cannot find credit card in database. CardId {request.CardId}");
                throw;
            }
        }

        private async Task UpdateCreditCardStatus(Card creditCard, CancellationToken cancellationToken)
        {
            try
            {
                var authToken = await TokenClient.GetToken(ConfigSalesforce, cancellationToken);

                var adapter = new CreditCardActivationAdapter(ProcessConfig);
                var response = await UpdateAssetClient.UpdateAsset(adapter.Adapt(creditCard), authToken.Result.AccessToken, cancellationToken);

                if (!response.IsSuccess)
                {
                    throw new UnprocessableEntityException("Cannot update asset in Salesforce", response.Message);
                }

            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Cannot update the field Status of the Credit Card asset in Salesforce. CardId {creditCard.CardId}");
                throw new UnprocessableEntityException($"Cannot update the field Status of the Credit Card asset in Salesforce. CardId {creditCard.CardId}", ex.Message);
            }
        }

        private string FormatExpirationDateToYYMM(string expireDate)
        {
            var month = expireDate.Substring(0, 2);
            var year = expireDate.Substring(3, 2);

            return string.Concat(year, month);
        }

        private async Task CardActitvationRtdx(Card creditCard, CancellationToken cancellationToken)
        {
            try
            {
                var token = await TokenOperation.GetTokenAsync(RtdxTokenParams, cancellationToken);

                var adapter = new CreditCardActivationAdapter(ProcessConfig);
                var rtdxCardToken = new RtdxCardToken()
                {
                    SecurityToken = token.Result.SecurityToken,
                    Card = creditCard
                };

                var cardActivationResponse = await CardActivationOperation.CardActivationAsync(adapter.Adapt(rtdxCardToken), cancellationToken);

                if (!cardActivationResponse.IsSuccess || cardActivationResponse.Result.ResponseCode != "00")
                {
                    Logger.LogError($"Cannot activate credit card in RTDX. \n Card ID {creditCard.CardId}. \n Error: {cardActivationResponse.ErrorMessage}");
                    throw new UnprocessableEntityException($"Cannot activate credit card in RTDX.", $"Card ID {creditCard.CardId} not found. Error: {cardActivationResponse.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Cannot active card in RTDX. CardId {creditCard.CardId}");
                throw new UnprocessableEntityException($"Cannot active card in RTDX. CardId {creditCard.CardId}", ex.Message);
            }
        }

        private void SaveCreditCardOnDatabase(Card creditCard, CancellationToken cancellationToken)
        {
            try
            {
                creditCard.CardStatus = Constants.ActiveStatus;

                CardWriteRepository.Update(creditCard, cancellationToken);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Execution error on SaveCreditCardOnDatabase");
                throw new UnprocessableEntityException("Cannot add credit card in database", ex.Message);
            }
        }

        #endregion
    }
}
