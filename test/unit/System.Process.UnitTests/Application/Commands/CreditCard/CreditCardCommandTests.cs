using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Process.Application.Commands.CreditCard;
using System.Process.Infrastructure.Configs;
using System.Process.UnitTests.Common;
using System.Phoenix.Common.Exceptions;
using System.Proxy.Salesforce;
using System.Proxy.Salesforce.GetBusinessInformation;
using System.Proxy.Salesforce.GetBusinessInformation.Message;
using System.Proxy.Salesforce.GetCreditCards;
using System.Proxy.Salesforce.GetCreditCards.Message;
using System.Proxy.Salesforce.GetTerms;
using System.Proxy.Salesforce.GetTerms.Messages;
using System.Proxy.Salesforce.GetToken;
using System.Proxy.Salesforce.GetToken.Messages;
using System.Proxy.Salesforce.Messages;
using System.Proxy.Salesforce.RegisterAsset;
using System.Proxy.Salesforce.RegisterAsset.Messages;
using Xunit;

namespace System.Process.UnitTests.Application.Commands.CreditCard
{
    public class CreditCardCommandTests
    {
        #region Properties

        private Mock<ILogger<CreditCardCommand>> Logger { get; }
        private IOptions<RecordTypesConfig> RecordTypesConfig { get; }
        private IOptions<GetTokenParams> ConfigSalesforce { get; }
        private Mock<IGetTokenClient> TokenClient { get; }
        private Mock<IRegisterAssetClient> RegisterAssetClient { get; }
        private Mock<IGetCreditCardsClient> GetCreditCardsClient { get; }
        private Mock<IGetTermsClient> GetTermsClient { get; }
        private Mock<IGetBusinessInformationClient> GetBusinessInformationClient { get; }

        #endregion

        #region Constructor

        public CreditCardCommandTests()
        {
            Logger = new Mock<ILogger<CreditCardCommand>>();
            var config = new RecordTypesConfig
            {
                AssetCreditCard = "test"
            };
            RecordTypesConfig = Options.Create(config);
            ConfigSalesforce = Options.Create(new GetTokenParams());
            TokenClient = new Mock<IGetTokenClient>();
            RegisterAssetClient = new Mock<IRegisterAssetClient>();
            GetCreditCardsClient = new Mock<IGetCreditCardsClient>();
            GetTermsClient = new Mock<IGetTermsClient>();
            GetBusinessInformationClient = new Mock<IGetBusinessInformationClient>();
        }

        #endregion

        #region Tests

        [Fact(DisplayName = "Should Handle Request Credit Card Successfully")]
        public async void ShouldRequestCreditCardSuccessfully()
        {
            TokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(GetSalesforceToken());
            RegisterAssetClient.Setup(x => x.RegisterAsset(It.IsAny<RegisterAssetParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetSalesforceResult());
            GetCreditCardsClient.Setup(x => x.GetCreditCards(It.IsAny<GetCreditCardsParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetCreditCards());
            GetTermsClient.Setup(x => x.GetTerms(It.IsAny<GetTermsParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetTermsResult());
            GetBusinessInformationClient.Setup(x => x.GetBusinessInformation(It.IsAny<GetBusinessInformationParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetBusinessInformationResult());
            var searchCreditCardQuery = new CreditCardCommand(Logger.Object, RecordTypesConfig, ConfigSalesforce, TokenClient.Object, RegisterAssetClient.Object, GetCreditCardsClient.Object, GetTermsClient.Object, GetBusinessInformationClient.Object);

            var result = await searchCreditCardQuery.Handle(GetCreditCardRequest(), new CancellationToken());

            result.Should().NotBeNull();
        }

        [Fact(DisplayName = "Should Handle Request Credit Card Throws UnprocessableEntityException")]
        public async void ShouldRequestCreditCardError()
        {
            TokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Throws(new Exception());
            GetTermsClient.Setup(x => x.GetTerms(It.IsAny<GetTermsParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetTermsResult());
            GetBusinessInformationClient.Setup(x => x.GetBusinessInformation(It.IsAny<GetBusinessInformationParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetBusinessInformationResult());
            var searchCreditCardQuery = new CreditCardCommand(Logger.Object, RecordTypesConfig, ConfigSalesforce, TokenClient.Object, RegisterAssetClient.Object, GetCreditCardsClient.Object, GetTermsClient.Object, GetBusinessInformationClient.Object);

            await Assert.ThrowsAsync<UnprocessableEntityException>(() => searchCreditCardQuery.Handle(GetCreditCardRequest(), new CancellationToken()));
        }

        #endregion

        #region Private Methods

        private CreditCardRequest GetCreditCardRequest()
        {
            return ConvertJson.ReadJson<CreditCardRequest>("CreditCardRequest.json");
        }

        private Task<BaseResult<GetTokenResult>> GetSalesforceToken()
        {
            return Task.FromResult(new BaseResult<GetTokenResult>
            {
                IsSuccess = true,
                Result = ConvertJson.ReadJson<GetTokenResult>("GetTokenResult.json")
            });
        }

        private Task<BaseResult<SalesforceResult>> GetSalesforceResult()
        {
            return Task.FromResult(new BaseResult<SalesforceResult>
            {
                IsSuccess = true,
                Result = ConvertJson.ReadJson<SalesforceResult>("SalesforceResult.json")
            });
        }

        private Task<BaseResult<QueryResult<Proxy.Salesforce.GetCreditCards.Message.CreditCard>>> GetCreditCards()
        {
            return Task.FromResult(new BaseResult<QueryResult<Proxy.Salesforce.GetCreditCards.Message.CreditCard>>
            {
                IsSuccess = true,
                Result = new QueryResult<Proxy.Salesforce.GetCreditCards.Message.CreditCard>
                {
                    Records = new List<Proxy.Salesforce.GetCreditCards.Message.CreditCard>
                    {

                    }
                }
            });
        }


        private Task<BaseResult<QueryResult<Terms>>> GetTermsResult()
        {
            return Task.FromResult(new BaseResult<QueryResult<Terms>>
            {
                IsSuccess = true,
                Result = new QueryResult<Terms>
                {
                    Records = new List<Terms>
                    {
                         new Terms
                            {
                                Version = 0,
                                Type = "Prohibited_Activities",
                                Name = "Card Scheme prohibited"
                            }
                    }
                }
            });
        }

        private Task<BaseResult<QueryResult<GetBusinessInformationResponse>>> GetBusinessInformationResult()
        {
            return Task.FromResult(new BaseResult<QueryResult<GetBusinessInformationResponse>>
            {
                IsSuccess = true,
                Result = new QueryResult<GetBusinessInformationResponse>
                {
                    Records = new List<GetBusinessInformationResponse>
                    {
                        ConvertJson.ReadJson<GetBusinessInformationResponse>("GetBusinessInformationResponse.json")
                    }
                }
            });
        }

        #endregion
    }
}
