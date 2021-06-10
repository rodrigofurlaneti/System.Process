using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Process.Application.Commands.CreditCardCancellation;
using System.Process.Infrastructure.Configs;
using System.Process.UnitTests.Common;
using System.Phoenix.Common.Exceptions;
using System.Proxy.Salesforce;
using System.Proxy.Salesforce.GetCreditCards;
using System.Proxy.Salesforce.GetCreditCards.Message;
using System.Proxy.Salesforce.GetToken;
using System.Proxy.Salesforce.GetToken.Messages;
using System.Proxy.Salesforce.Messages;
using System.Proxy.Salesforce.UpdateAsset;
using System.Proxy.Salesforce.UpdateAsset.Messages;
using Xunit;

namespace System.Process.UnitTests.Application.Commands.CreditCardCancellation
{
    public class CreditCardCancellationCommandTests
    {
        #region Properties

        private Mock<ILogger<CreditCardCancellationCommand>> Logger { get; }
        private IOptions<RecordTypesConfig> RecordTypesConfig { get; }
        private IOptions<GetTokenParams> ConfigSalesforce { get; }
        private Mock<IGetTokenClient> TokenClient { get; }
        private Mock<IGetCreditCardsClient> GetCreditCardsClient { get; }
        private Mock<IUpdateAssetClient> UpdateAssetClient { get; }
        private CancellationToken CancellationToken { get; }

        #endregion

        #region Constructor

        public CreditCardCancellationCommandTests()
        {
            Logger = new Mock<ILogger<CreditCardCancellationCommand>>();
            var config = ConvertJson.ReadJson<RecordTypesConfig>("RecordTypesConfig.json");
            RecordTypesConfig = Options.Create(config);
            var configSalesforce = new GetTokenParams();
            ConfigSalesforce = Options.Create(configSalesforce);
            TokenClient = new Mock<IGetTokenClient>();
            GetCreditCardsClient = new Mock<IGetCreditCardsClient>();
            UpdateAssetClient = new Mock<IUpdateAssetClient>();
        }

        #endregion

        #region Tests

        [Fact(DisplayName = "Should Handle credit card cancellation Successfully")]
        public async void ShouldCreditCardCancellationSuccessfully()
        {
            TokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(GetSalesforceToken());
            GetCreditCardsClient.Setup(x => x.GetCreditCards(It.IsAny<GetCreditCardsParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetCreditCard());
            UpdateAssetClient.Setup(x => x.UpdateAsset(It.IsAny<UpdateAssetParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetSalesforceResult());
            var command = new CreditCardCancellationCommand(Logger.Object, RecordTypesConfig, ConfigSalesforce, TokenClient.Object, GetCreditCardsClient.Object, UpdateAssetClient.Object);

            var request = GetCreditCardCancellation();

            var result = await command.Handle(request, CancellationToken);

            result.Should().NotBeNull();
        }

        [Fact(DisplayName = "Should returns 'The credit card does not belong to the customer'")]
        public async void CreditCardDoesNotBelongToTheCustomer()
        {
            TokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(GetSalesforceToken());
            GetCreditCardsClient.Setup(x => x.GetCreditCards(It.IsAny<GetCreditCardsParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetCreditCard());
            var command = new CreditCardCancellationCommand(Logger.Object, RecordTypesConfig, ConfigSalesforce, TokenClient.Object, GetCreditCardsClient.Object, UpdateAssetClient.Object);

            var request = GetCreditCardCancellation();
            request.AssetId = "error";

            await Assert.ThrowsAsync<UnprocessableEntityException>(() => command.Handle(request, CancellationToken));
        }

        [Fact(DisplayName = "Should returns The Credit Card asset must have the status 'Approved Pending Acceptance'")]
        public async void CreditCardStatusApprovedPendingAcceptance()
        {
            TokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(GetSalesforceToken());
            var creditCards = GetCreditCard();
            creditCards.Result.Result.Records[0].Status = "error";
            GetCreditCardsClient.Setup(x => x.GetCreditCards(It.IsAny<GetCreditCardsParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(creditCards);
            UpdateAssetClient.Setup(x => x.UpdateAsset(It.IsAny<UpdateAssetParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetSalesforceResult());
            var command = new CreditCardCancellationCommand(Logger.Object, RecordTypesConfig, ConfigSalesforce, TokenClient.Object, GetCreditCardsClient.Object, UpdateAssetClient.Object);

            var request = GetCreditCardCancellation();

            await Assert.ThrowsAsync<UnprocessableEntityException>(() => command.Handle(request, CancellationToken));
        }

        [Fact(DisplayName = "Should Handle Throws UnprocessableEntityException")]
        public async void ShouldThrowUnprocessableEntityException()
        {
            TokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(GetSalesforceToken());
            GetCreditCardsClient.Setup(x => x.GetCreditCards(It.IsAny<GetCreditCardsParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetCreditCard());
            UpdateAssetClient.Setup(x => x.UpdateAsset(It.IsAny<UpdateAssetParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Throws(new Exception());
            var command = new CreditCardCancellationCommand(Logger.Object, RecordTypesConfig, ConfigSalesforce, TokenClient.Object, GetCreditCardsClient.Object, UpdateAssetClient.Object);

            await Assert.ThrowsAsync<UnprocessableEntityException>(() => command.Handle(GetCreditCardCancellation(), CancellationToken));
        }

        #endregion

        #region Private methods

        private CreditCardCancellationRequest GetCreditCardCancellation()
        {
            return ConvertJson.ReadJson<CreditCardCancellationRequest>("CreditCardCancellationRequest.json");
        }

        private Task<BaseResult<GetTokenResult>> GetSalesforceToken()
        {
            return Task.FromResult(new BaseResult<GetTokenResult>
            {
                IsSuccess = true,
                Result = ConvertJson.ReadJson<GetTokenResult>("GetTokenResult.json")
            });
        }

        private Task<BaseResult<QueryResult<Proxy.Salesforce.GetCreditCards.Message.CreditCard>>> GetCreditCard()
        {
            return Task.FromResult(new BaseResult<QueryResult<Proxy.Salesforce.GetCreditCards.Message.CreditCard>>
            {
                IsSuccess = true,
                Result = new QueryResult<Proxy.Salesforce.GetCreditCards.Message.CreditCard>
                {
                    Records = new List<Proxy.Salesforce.GetCreditCards.Message.CreditCard>
                {
                    new Proxy.Salesforce.GetCreditCards.Message.CreditCard
                    {
                          AssetId = "string",
                          CreditLimit =  1,
                          Status = "Approved Pending Acceptance",
                          CreditCardType = "string"
                    }
                }
                }
            });
        }

        private Task<BaseResult<SalesforceResult>> GetSalesforceResult()
        {
            return Task.FromResult(new BaseResult<SalesforceResult>
            {
                IsSuccess = true,
                Result = new SalesforceResult
                {
                    Success = true
                }
            });
        }

        #endregion
    }
}
