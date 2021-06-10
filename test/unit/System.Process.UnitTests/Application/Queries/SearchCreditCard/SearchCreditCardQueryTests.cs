using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Process.Application.Queries.SearchCreditCard;
using System.Process.Infrastructure.Configs;
using System.Phoenix.Common.Exceptions;
using System.Proxy.Salesforce.GetCreditCards;
using System.Proxy.Salesforce.GetCreditCards.Message;
using System.Proxy.Salesforce.GetToken;
using System.Proxy.Salesforce.GetToken.Messages;
using System.Proxy.Salesforce.Messages;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace System.Process.UnitTests.Application.Queries.SearchCreditCard
{
    public class SearchCreditCardQueryTests
    {
        #region Properties

        private Mock<ILogger<SearchCreditCardQuery>> Logger { get; }
        private IOptions<RecordTypesConfig> RecordTypesConfig { get; }
        private IOptions<GetTokenParams> ConfigSalesforce { get; }
        private Mock<IGetTokenClient> TokenClient { get; }
        private Mock<IGetCreditCardsClient> GetCreditCardsClient { get; }

        #endregion

        #region Constructor

        public SearchCreditCardQueryTests()
        {
            Logger = new Mock<ILogger<SearchCreditCardQuery>>();
            var config = new RecordTypesConfig
            {
                AssetCreditCard = "test"
            };
            RecordTypesConfig = Options.Create(config);
            ConfigSalesforce = Options.Create(new GetTokenParams());
            TokenClient = new Mock<IGetTokenClient>();
            GetCreditCardsClient = new Mock<IGetCreditCardsClient>();
        }

        #endregion

        #region Tests

        [Fact(DisplayName = "Should Handle Search Credit Card Successfully")]
        public async void ShouldSearchCreditCardSuccessfully()
        {
            TokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(GetSalesforceToken());
            GetCreditCardsClient.Setup(x => x.GetCreditCards(It.IsAny<GetCreditCardsParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetCreditCards());
            var searchCreditCardQuery = new SearchCreditCardQuery(Logger.Object, RecordTypesConfig, ConfigSalesforce, TokenClient.Object, GetCreditCardsClient.Object);

            var result = await searchCreditCardQuery.Handle(GetSearchCreditCardRequest(), new CancellationToken());

            result.Should().NotBeNull();
        }

        [Fact(DisplayName = "Should Handle Search Credit Card Throws UnprocessableEntityException")]
        public async void ShouldSearchCreditCardError()
        {
            TokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Throws(new Exception());
            var searchCreditCardQuery = new SearchCreditCardQuery(Logger.Object, RecordTypesConfig, ConfigSalesforce, TokenClient.Object, GetCreditCardsClient.Object);

            await Assert.ThrowsAsync<UnprocessableEntityException>(() => searchCreditCardQuery.Handle(GetSearchCreditCardRequest(), new CancellationToken()));
        }

        #endregion

        #region Private Methods

        private SearchCreditCardRequest GetSearchCreditCardRequest()
        {
            return new SearchCreditCardRequest
            {
                SystemId = "test"
            };
        }

        private Task<BaseResult<GetTokenResult>> GetSalesforceToken()
        {
            return Task.FromResult(new BaseResult<GetTokenResult>
            {
                IsSuccess = true,
                Result = new GetTokenResult
                {
                    AccessToken = "test"
                }
            });
        }

        private Task<BaseResult<QueryResult<CreditCard>>> GetCreditCards()
        {
            return Task.FromResult(new BaseResult<QueryResult<CreditCard>>
            {
                IsSuccess = true,
                Result = new QueryResult<CreditCard>
                {
                    Records = new List<CreditCard> {
                        new CreditCard
                        {
                            AssetId = "tests",
                            CreditCardType = "tests",
                            CreditLimit = 210,
                            Status = "tests",
                            Product = "MCB",
                            Subproduct = "001"
                        }
                    }
                }
            });
        }
        #endregion
    }
}
