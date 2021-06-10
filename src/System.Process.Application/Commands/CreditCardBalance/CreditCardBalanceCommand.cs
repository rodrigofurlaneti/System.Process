using System;
using System.Globalization;
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
using System.Proxy.Rtdx.BalanceInquiry;
using System.Proxy.Rtdx.BalanceInquiry.Messages;
using System.Proxy.Rtdx.GetToken;
using System.Proxy.Rtdx.GetToken.Messages;
using System.Proxy.Rtdx.Messages;

namespace System.Process.Application.Commands.CreditCardBalance
{
    public class CreditCardBalanceCommand : IRequestHandler<CreditCardBalanceRequest, CreditCardBalanceResponse>
    {
        #region Properties

        private ILogger<CreditCardBalanceCommand> Logger { get; }
        private ICardReadRepository CardReadRepository { get; }
        private IBalanceInquiryOperation BalanceInquiryOperation { get; }
        private IGetTokenOperation GetTokenOperation { get; }
        private GetTokenParams RtdxTokenParams { get; }
        private ProcessConfig ProcessConfig { get; }

        #endregion

        #region Constructor

        public CreditCardBalanceCommand(
            ILogger<CreditCardBalanceCommand> logger,
            ICardReadRepository cardReadRepository,
            IBalanceInquiryOperation balanceInquiryOperation,
            IGetTokenOperation getTokenOperation,
            IOptions<GetTokenParams> rtdxTokenParams,
            IOptions<ProcessConfig> ProcessConfig
            )
        {
            Logger = logger;
            CardReadRepository = cardReadRepository;
            BalanceInquiryOperation = balanceInquiryOperation;
            GetTokenOperation = getTokenOperation;
            RtdxTokenParams = rtdxTokenParams.Value;
            ProcessConfig = ProcessConfig.Value;
        }

        #endregion

        #region IRequestHandler implementation

        public async Task<CreditCardBalanceResponse> Handle(CreditCardBalanceRequest request, CancellationToken cancellationToken)
        {
            var creditCard = FindCreditCard(request);
            CreditCardValidation(creditCard);
            var balance = await InquiryBalance(creditCard, cancellationToken);

            return await Task.FromResult(new CreditCardBalanceResponse
            {
                CardId = request.CardId,
                AvailableCredit = GetValueFormat(balance?.Result?.AvailableCredit.ToString()),
                TotalNewBalance = GetValueFormat(balance?.Result?.TotalNewBalance)
            });
        }

        #endregion

        #region Methods

        private static string GetValueFormat(string value)
        {
            return (Convert.ToDecimal(value) / 100).ToString(new CultureInfo("en-US"));
        }

        private Card FindCreditCard(CreditCardBalanceRequest request)
        {
            try
            {
                var creditCard = CardReadRepository.FindByCardId(request.CardId);

                if (creditCard == null)
                {
                    Logger.LogError($"Cannot find credit card in secure database", $"Card ID {creditCard?.CardId} not found");
                    throw new UnprocessableEntityException($"Cannot find credit card in secure database", $"Card ID {creditCard.CardId} not found");
                }

                return creditCard;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Cannot find credit card in database. Card ID {request.CardId}");
                throw new UnprocessableEntityException($"Cannot find credit card in database. Card ID  {request.CardId}", ex.Message);
            }
        }

        private void CreditCardValidation(Card creditCard)
        {
            if (creditCard.CardStatus != Constants.ActiveStatus)
            {
                Logger.LogError($"The card status must be 'Active'", $"Card ID {creditCard.CardId}, current status card '{creditCard.CardStatus}'");
                throw new UnprocessableEntityException($"The card status must be 'Active'", $"Card ID {creditCard.CardId}, current status card '{creditCard.CardStatus}'");
            }
        }

        private async Task<BaseResult<BalanceInquiryResult>> InquiryBalance(Card creditCard, CancellationToken cancellationToken)
        {
            try
            {
                var adapter = new CreditCardBalanceAdapter(ProcessConfig);
                var token = await GetTokenOperation.GetTokenAsync(RtdxTokenParams, cancellationToken);

                var adaptedRequest = adapter.Adapt(creditCard, token.Result.SecurityToken);

                var result = await BalanceInquiryOperation.BalanceInquiryAsync(adaptedRequest, cancellationToken);

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Cannot the balance inquiry in Rtdx. AssetId {creditCard.AssetId}");
                throw new UnprocessableEntityException($"Cannot the balance inquiry in Rtdx. AssetId {creditCard.AssetId}", ex.Message);
            }
        }

        #endregion
    }
}