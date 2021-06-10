using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Phoenix.Common.Exceptions;
using System.Proxy.Rtdx.GetToken;
using System.Proxy.Rtdx.GetToken.Messages;
using System.Proxy.Rtdx.PaymentInquiry;
using System.Proxy.Rtdx.PaymentInquiry.Messages;

namespace System.Process.Application.Commands.CreditCardMakePayment
{
    public class CreditCardMakePaymentCommand : IRequestHandler<CreditCardMakePaymentRequest, CreditCardMakePaymentResponse>
    {
        #region Properties

        private ILogger<CreditCardMakePaymentCommand> Logger { get; }
        private ICardReadRepository CardReadRepository { get; }
        private IGetTokenOperation GetTokenOperation { get; }
        private GetTokenParams RtdxTokenParams { get; }
        private IPaymentInquiryOperation PaymentInquiryOperation { get; }

        #endregion

        #region Constructor

        public CreditCardMakePaymentCommand(
            ILogger<CreditCardMakePaymentCommand> logger,
            ICardReadRepository cardReadRepository,
            IGetTokenOperation getTokenOperation,
            IOptions<GetTokenParams> rtdxTokenParams,
            IPaymentInquiryOperation paymentInquiryOperation)
        {
            Logger = logger;
            CardReadRepository = cardReadRepository;
            GetTokenOperation = getTokenOperation;
            RtdxTokenParams = rtdxTokenParams.Value;
            PaymentInquiryOperation = paymentInquiryOperation;
        }

        #endregion

        #region IRequestHandler implementation

        public async Task<CreditCardMakePaymentResponse> Handle(CreditCardMakePaymentRequest request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.CardId == 0)
                {
                    Logger.LogError("Cannot search credit card transactions", "CardId cannot be equal 0");
                    throw new NotFoundException("Cannot search credit card transactions", "CardId cannot be equal 0");
                }

                var creditCard = CreditCardValidation(request);
                var tokenRtdx = await GetTokenOperation.GetTokenAsync(RtdxTokenParams, cancellationToken);

                PaymentInquiryParams paymentInquiryParams = new PaymentInquiryParams()
                {
                    AccountNumber = creditCard.Pan,
                    SecurityToken = tokenRtdx.Result.SecurityToken
                };

                var paymentInquiryResult = await PaymentInquiryOperation.PaymentInquiryAsync(paymentInquiryParams, cancellationToken);

                var adapter = new CreditCardMakePaymentAdapter();
                var result = adapter.Adapt(paymentInquiryResult.Result);

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Execution error on CreditCardMakePaymentQuery Handle");
                throw new UnprocessableEntityException("Cannot make a payment", ex.Message);
            }
        }

        #endregion

        #region Methods

        private Card CreditCardValidation(CreditCardMakePaymentRequest request)
        {
            try
            {
                var creditCards = CardReadRepository.FindByCardId(request.CardId);

                if (creditCards == null)
                {
                    Logger.LogError($"Cannot find credit card in secure database", $"Card ID {request.CardId} not found");
                    throw new UnprocessableEntityException($"Cannot find credit card in secure database", $"Card ID {request.CardId}  not found");
                }

                return creditCards;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Cannot find credit card in database. Card ID {request.CardId}");
                throw new UnprocessableEntityException($"Cannot find credit card in database. Card ID {request.CardId} ", ex.Message);
            }
        }

        #endregion
    }
}
