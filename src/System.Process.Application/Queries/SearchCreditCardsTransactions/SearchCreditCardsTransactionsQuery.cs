using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Process.Application.Queries.SearchCreditCardsTransactions.Response;
using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Process.Infrastructure.Configs;
using System.Phoenix.Common.Exceptions;
using System.Proxy.Rtdx.GetToken;
using System.Proxy.Rtdx.GetToken.Messages;
using System.Proxy.Rtdx.PendingActivityDetails;
using System.Proxy.Rtdx.TransactionDetails;

namespace System.Process.Application.Queries.SearchCreditCardsTransactions
{
    public class SearchCreditCardsTransactionsQuery : IRequestHandler<SearchCreditCardsTransactionsRequest, SearchCreditCardsTransactionsResponse>
    {
        #region Properties

        private ILogger<SearchCreditCardsTransactionsQuery> Logger { get; }
        private ICardReadRepository CardReadRepository { get; }
        private RecordTypesConfig RecordTypesConfig { get; }
        private ITransactionDetailsOperation TransactionDetailsOperation { get; }
        private IPendingActivityDetailsOperation PendingActivityDetailsOperation { get; }
        private IGetTokenOperation GetTokenOperation { get; }
        private GetTokenParams RtdxTokenParams { get; }

        #endregion

        #region Constructor

        public SearchCreditCardsTransactionsQuery(
            ILogger<SearchCreditCardsTransactionsQuery> logger,
            ICardReadRepository cardReadRepository,
            IOptions<RecordTypesConfig> recordTypeConfig,
            ITransactionDetailsOperation transactionDetailsOperation,
            IPendingActivityDetailsOperation pendingActivityDetailsOperation,
            IGetTokenOperation getTokenOperation,
            IOptions<GetTokenParams> rtdxTokenParams)
        {
            Logger = logger;
            CardReadRepository = cardReadRepository;
            RecordTypesConfig = recordTypeConfig.Value;
            TransactionDetailsOperation = transactionDetailsOperation;
            RtdxTokenParams = rtdxTokenParams.Value;
            PendingActivityDetailsOperation = pendingActivityDetailsOperation;
            GetTokenOperation = getTokenOperation;
        }

        #endregion

        #region IRequestHandler implementation

        public async Task<SearchCreditCardsTransactionsResponse> Handle(SearchCreditCardsTransactionsRequest request, CancellationToken cancellationToken)
        {
            try
            {
                Logger.LogInformation($"Starting Search Credit Cards Transactions. CardId : {request.CardId}");

                if (!int.TryParse(request.CardId, out int result))
                {
                    Logger.LogError("Cannot search credit card transactions", "CardId must be a numeric characters");
                    throw new NotFoundException("Cannot search credit card transactions", "CardId must be a numeric character");
                }

                if (int.Parse(request.CardId) == 0)
                {
                    Logger.LogError("Cannot search credit card transactions", "CardId cannot be equal 0");
                    throw new NotFoundException("Cannot search credit card transactions", "CardId cannot be equal 0");
                }

                Logger.LogInformation($"Starting Credit Card Validation.");
                var creditCard = CreditCardValidation(request);

                Logger.LogInformation($"Starting RTDX GetToken.");
                var tokenRtdx = await GetTokenOperation.GetTokenAsync(RtdxTokenParams, cancellationToken);

                var adapter = new SearchCreditCardsTransactionsAdapter(RecordTypesConfig, tokenRtdx.Result.SecurityToken);

                Logger.LogInformation($"Starting Transaction Details adapter.");
                var transactionRequest = adapter.Adapt(creditCard);

                Logger.LogInformation($"Transaction Details Request {JsonSerializer.Serialize(transactionRequest)}");
                var transactionDetail = await TransactionDetailsOperation.TransactionDetailsAsync(transactionRequest, cancellationToken);

                Logger.LogInformation($"Starting Pending Activity Details adapter.");
                var pendingActivityRequest = adapter.AdaptPending(creditCard);

                Logger.LogInformation($"Pending Activity Details Request: {JsonSerializer.Serialize(pendingActivityRequest)}");
                var pendingActivityDetails = await PendingActivityDetailsOperation.PendingActivityDetailsAsync(pendingActivityRequest, cancellationToken);

                var creditCardsAdapterResponse = new CreditCardsAdapterResponse(transactionDetail.Result?.StatementLines, pendingActivityDetails.Result?.Authorizations.AuthorizationItems, int.Parse(request.CardId));
                Logger.LogInformation($"Credit Cards Adapter Response: {JsonSerializer.Serialize(creditCardsAdapterResponse)}");

                return adapter.Adapt(creditCardsAdapterResponse);
            }
            catch (NotFoundException ex)
            {
                Logger.LogError(ex, "Execution error on SearchCreditCardsTransactionsQuery Handle");
                throw new UnprocessableEntityException(ex.Message, ex.Details);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Execution error on SearchCreditCardsTransactionsQuery Handle");
                throw new UnprocessableEntityException(ex.Message);
            }
        }


        #endregion

        #region Methods

        private Card CreditCardValidation(SearchCreditCardsTransactionsRequest request)
        {
            try
            {
                Logger.LogInformation($"Starting Get Card in Security Database.");
                var creditCards = CardReadRepository.Find(int.Parse(request.CardId));
                
                if (creditCards == null)
                {
                    Logger.LogError($"Cannot find credit card in secure database", $"Card ID {request.CardId} not found");
                    throw new UnprocessableEntityException($"Cannot find credit card in secure database", $"Card ID {request.CardId}  not found");
                }
                
                Logger.LogInformation($"Card found on secure base by id:{request.CardId}\n Card:{JsonSerializer.Serialize(creditCards)}");

                return creditCards[0];
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