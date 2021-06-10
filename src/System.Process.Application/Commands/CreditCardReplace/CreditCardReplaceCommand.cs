using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Process.Application.Commands.CreditCardReplace.Adapters;
using System.Process.Domain.Constants;
using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Process.Domain.ValueObjects;
using System.Phoenix.Common.Exceptions;
using System.Proxy.Rtdx.GetToken;
using System.Proxy.Rtdx.GetToken.Messages;
using System.Proxy.Rtdx.Messages;
using System.Proxy.Rtdx.OrderNewPlastic;
using System.Proxy.Rtdx.OrderNewPlastic.Messages;

namespace System.Process.Application.Commands.CreditCardReplace
{
    public class CreditCardReplaceCommand : IRequestHandler<CreditCardReplaceRequest, CreditCardReplaceResponse>
    {
        #region Properties

        private ILogger<CreditCardReplaceCommand> Logger { get; }
        private ICardReadRepository CardReadRepository { get; }
        private IOrderNewPlasticOperation OrderNewPlasticOperation { get; }
        private IGetTokenOperation GetTokenOperation { get; }
        private GetTokenParams RtdxTokenParams { get; }
        private ProcessConfig ProcessConfig { get; }

        #endregion

        #region Constructor

        public CreditCardReplaceCommand(
            ILogger<CreditCardReplaceCommand> logger,
            ICardReadRepository cardReadRepository,
            IOrderNewPlasticOperation balanceInquiryOperation,
            IGetTokenOperation getTokenOperation,
            IOptions<GetTokenParams> rtdxTokenParams,
            IOptions<ProcessConfig> ProcessConfig)
        {
            Logger = logger;
            CardReadRepository = cardReadRepository;
            OrderNewPlasticOperation = balanceInquiryOperation;
            GetTokenOperation = getTokenOperation;
            RtdxTokenParams = rtdxTokenParams.Value;
            ProcessConfig = ProcessConfig.Value;
        }

        #endregion

        #region IRequestHandler

        public async Task<CreditCardReplaceResponse> Handle(CreditCardReplaceRequest request, CancellationToken cancellationToken)
        {
            var creditCard = FindCreditCard(request);
            CreditCardValidation(creditCard);

            var replaceCardResponse = await CreditCardReplace(creditCard, cancellationToken);

            if (!replaceCardResponse.IsSuccess || replaceCardResponse.Result.ResponseCode != "00")
            {
                Logger.LogError($"Cannot replace credit card in RTDX. \n Card ID {creditCard.CardId}. \n Error: {replaceCardResponse.ErrorMessage}");
                throw new UnprocessableEntityException($"Cannot replace credit card in RTDX.", $"Card ID {creditCard.CardId} not found. Error: {replaceCardResponse.ErrorMessage}");
            }
            else
            {

                return await Task.FromResult(new CreditCardReplaceResponse
                {
                    Status = true,

                });
            }
        }

        #endregion

        #region Private Methods

        private Card FindCreditCard(CreditCardReplaceRequest request)
        {
            try
            {
                var creditCard = CardReadRepository.FindByCardId(request.CardId);

                if (creditCard == null)
                {
                    Logger.LogError($"Cannot find credit card in secure database", $"Card ID {request?.CardId} not found");
                    throw new UnprocessableEntityException($"Cannot find credit card in secure database", $"Card ID {request.CardId} not found");
                }

                return creditCard;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Cannot find credit card in database. Card ID {request.CardId}");
                throw new UnprocessableEntityException($"Cannot find credit card in database. Card ID  {request.CardId}", ex);
            }
        }

        private void CreditCardValidation(Card creditCard)
        {
            if (creditCard.CardStatus != Constants.ActivedStatus && creditCard.CardStatus != Constants.ActiveStatus)
            {
                Logger.LogError($"The card status must be 'Active'", $"Card ID {creditCard.CardId}, current status card '{creditCard.CardStatus}'");
                throw new UnprocessableEntityException($"The card status must be 'Active'", $"Card ID {creditCard.CardId}, current status card '{creditCard.CardStatus}'");
            }
        }

        private async Task<BaseResult<OrderNewPlasticResult>> CreditCardReplace(Card creditCard, CancellationToken cancellationToken)
        {
            try
            {
                var adapter = new CreditCardReplaceAdapter(ProcessConfig);
                var token = await GetTokenOperation.GetTokenAsync(RtdxTokenParams, cancellationToken);

                var adaptedRequest = adapter.Adapt(creditCard, token.Result.SecurityToken);

                var result = await OrderNewPlasticOperation.OrderNewPlasticAsync(adaptedRequest, cancellationToken);
                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Cannot replace credit card in Rtdx. AssetId {creditCard.AssetId}");
                throw new UnprocessableEntityException($"Cannot replace credit card in Rtdx. AssetId {creditCard.AssetId}", ex);
            }
        }



        #endregion
    }
}
