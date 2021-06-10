using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Phoenix.Common.Exceptions;
using System.Proxy.Fis.Exceptions;
using System.Proxy.Fis.GetToken;
using System.Proxy.Fis.GetToken.Messages;
using System.Proxy.Fis.Messages;
using System.Proxy.Fis.SearchTransactions;
using System.Proxy.Fis.SearchTransactions.Messages;
using System.Proxy.Fis.SearchTransactions.Messages.Result;

namespace System.Process.Application.Commands.SearchTransactions
{
    public class SearchTransactionsCommand : IRequestHandler<ConsultCardsByidCardRequest, SearchTransactionsResponse>
    {
        #region Properties

        private ILogger<SearchTransactionsCommand> Logger { get; }
        private IGetTokenClient GetTokenClient { get; }
        private GetTokenParams GetTokenParams { get; }
        private ISearchTransactionsClient SearchTransactionsClient { get; }
        private ICardReadRepository CardReadRepository { get; }

        #endregion

        #region Constructor

        public SearchTransactionsCommand(
            ILogger<SearchTransactionsCommand> logger,
            IGetTokenClient getTokenClient,
            IOptions<GetTokenParams> getTokenParams,
            ISearchTransactionsClient searchTransactionsClient,
            ICardReadRepository cardReadRepository
            )
        {
            Logger = logger;
            GetTokenClient = getTokenClient;
            GetTokenParams = getTokenParams.Value;
            SearchTransactionsClient = searchTransactionsClient;
            CardReadRepository = cardReadRepository;
        }

        #endregion

        #region IRequestHandler implementation

        public async Task<SearchTransactionsResponse> Handle(ConsultCardsByidCardRequest request, CancellationToken cancellationToken)
        {
            Logger.LogInformation("Step 1 - Get cards on oracle by by CardId");
            var cards = GetCardDetailsByCardId(request);

            var requestSearch = new SearchTransactionsRequest
            {
                Pan = cards.Pan,
                MaxRows = 20
            };
            Logger.LogInformation("Step 2 - Search Transactions by card pan");
            var result = await SearchTransactions(requestSearch, cancellationToken);

            return new SearchTransactionsResponse
            {
                Metadata = result.Result?.Entity?.Metadata,
                Transactions = result.Result?.Entity?.Transactions ?? new List<Transactions>()
            };
        }

        private Card GetCardDetailsByCardId(ConsultCardsByidCardRequest request)
        {
            var card = CardReadRepository.Find(request.CardId)?.FirstOrDefault();

            if (card == null)
            {
                throw new NotFoundException($"CardId {request.CardId} not found");
            }

            return card;
        }

        private async Task<BaseResult<SearchTransactionsResult>> SearchTransactions(SearchTransactionsRequest requestSearch, CancellationToken cancellationToken)
        {
            try
            {
                var adapter = new SearchTransactionsAdapter();

                var paramsSearchTransactions = adapter.Adapt(requestSearch);
                var token = await GetTokenClient.GetToken(GetTokenParams, cancellationToken);
                var result = await SearchTransactionsClient.SearchTransactionsAsync(paramsSearchTransactions, token.Result.AccessToken, cancellationToken);

                if (!result.IsSuccess)
                {
                    Logger.LogError($"Error during SearchTransactions {result.Message}");
                    var error = JsonConvert.DeserializeObject<ReturnMetadata>(result.Message);
                    throw new UnprocessableEntityException(error.Metadata.Messages[0].Text);
                }

                return result;
            }
            catch (FisException ex)
            {
                Logger.LogError(ex, "Error during the search transactions operation");
                throw new UnprocessableEntityException("Error during the search transactions operation", ex);
            }
            
        }

        #endregion
    }
}