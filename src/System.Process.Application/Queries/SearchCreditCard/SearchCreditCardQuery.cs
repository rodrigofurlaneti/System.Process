using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Process.Infrastructure.Configs;
using System.Phoenix.Common.Exceptions;
using System.Proxy.Salesforce.GetCreditCards;
using System.Proxy.Salesforce.GetToken;
using System.Proxy.Salesforce.GetToken.Messages;

namespace System.Process.Application.Queries.SearchCreditCard
{
    public class SearchCreditCardQuery : IRequestHandler<SearchCreditCardRequest, SearchCreditCardResponse>
    {
        #region Properties

        private ILogger<SearchCreditCardQuery> Logger { get; }
        private RecordTypesConfig RecordTypesConfig { get; }
        private GetTokenParams ConfigSalesforce { get; }
        private IGetTokenClient TokenClient { get; }
        private IGetCreditCardsClient GetCreditCardsClient { get; }

        #endregion

        #region Constructor

        public SearchCreditCardQuery(
            ILogger<SearchCreditCardQuery> logger,
            IOptions<RecordTypesConfig> recordTypeConfig,
            IOptions<GetTokenParams> configSalesforce,
            IGetTokenClient tokenClient,
            IGetCreditCardsClient getCreditCardsClient)
        {
            Logger = logger;
            RecordTypesConfig = recordTypeConfig.Value;
            ConfigSalesforce = configSalesforce.Value;
            TokenClient = tokenClient;
            GetCreditCardsClient = getCreditCardsClient;
        }

        #endregion

        #region IRequestHandler implementation

        public async Task<SearchCreditCardResponse> Handle(SearchCreditCardRequest request, CancellationToken cancellationToken)
        {
            try
            {
                if (string.IsNullOrEmpty(request.SystemId))
                {
                    Logger.LogError("Cannot search credit card", "SystemId cannot be null or empty");
                    throw new NotFoundException("Cannot search credit card", "SystemId cannot be null or empty");
                }

                var authToken = await TokenClient.GetToken(ConfigSalesforce, cancellationToken);
                var adapter = new SearchCreditCardAdapter(RecordTypesConfig);
                var creditCards = await GetCreditCardsClient.GetCreditCards(adapter.Adapt(request), authToken.Result.AccessToken, cancellationToken);

                return adapter.Adapt(creditCards.Result);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Execution error on SearchCreditCardQuery Handle");
                throw new UnprocessableEntityException("Cannot search credit card in Salesforce", ex.Message);
            }
        }

        #endregion
    }
}
