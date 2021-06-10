using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Process.Application.Commands.CreditCardActivation;
using System.Process.Base.IntegrationTests;
using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Process.Domain.ValueObjects;
using System.Process.UnitTests.Common;
using System.Phoenix.Common.Exceptions;
using System.Proxy.Rtdx.CardActivation;
using System.Proxy.Rtdx.CardActivation.Messages;
using System.Proxy.Rtdx.GetToken;
using System.Proxy.Rtdx.Messages;
using System.Proxy.Salesforce;
using System.Proxy.Salesforce.GetToken;
using System.Proxy.Salesforce.GetToken.Messages;
using System.Proxy.Salesforce.UpdateAsset;
using System.Proxy.Salesforce.UpdateAsset.Messages;
using Xunit;
using RtdxGetToken = System.Proxy.Rtdx.GetToken.Messages;

namespace System.Process.UnitTests.Application.Commands.CreditCardActivation
{
    public class CreditCardActivationCommandTests
    {
        #region Properties

        private Mock<ILogger<CreditCardActivationCommand>> Logger { get; }
        private Mock<ICardReadRepository> CardReadRepository { get; }
        private Mock<IUpdateAssetClient> UpdateAssetClient { get; }
        private Mock<IGetTokenClient> TokenClient { get; }
        private Mock<IGetTokenOperation> TokenOperation { get; }
        private Mock<ICardActivationOperation> CardActivationOperation { get; }
        private IOptions<GetTokenParams> ConfigSalesforce { get; }
        private IOptions<Proxy.Rtdx.GetToken.Messages.GetTokenParams> RtdxTokenParams { get; }
        private Mock<ICardWriteRepository> CardWriteRepository { get; }
        private Mock<IOptions<ProcessConfig>> ProcessConfig { get; }

        #endregion

        #region Constructor

        public CreditCardActivationCommandTests()
        {
            Logger = new Mock<ILogger<CreditCardActivationCommand>>();
            CardReadRepository = new Mock<ICardReadRepository>();
            UpdateAssetClient = new Mock<IUpdateAssetClient>();
            TokenClient = new Mock<IGetTokenClient>();
            TokenClient = new Mock<IGetTokenClient>();
            CardActivationOperation = new Mock<ICardActivationOperation>();
            var param = new GetTokenParams();
            ConfigSalesforce = Options.Create(param);
            var paramRtdx = new Proxy.Rtdx.GetToken.Messages.GetTokenParams();
            RtdxTokenParams = Options.Create(paramRtdx);
            TokenOperation = new Mock<IGetTokenOperation>();
            CardWriteRepository = new Mock<ICardWriteRepository>();
            ProcessConfig = new Mock<IOptions<ProcessConfig>>();
        }

        #endregion

        #region Tests

        [Fact(DisplayName = "Should Handle credit card activation Successfully")]
        public async void ShouldCreditCardActivationSuccessfully()
        {
            TokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(GetSalesforceToken());
            UpdateAssetClient.Setup(x => x.UpdateAsset(It.IsAny<UpdateAssetParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetSalesforceResult());
            var creditCard = GetCard();
            creditCard.CardStatus = "Pending Activation";
            CardReadRepository.Setup(x => x.FindByCardId(It.IsAny<int>())).Returns(creditCard);
            CardWriteRepository.Setup(x => x.Add(It.IsAny<Card>(), It.IsAny<CancellationToken>()));
            CardActivationOperation.Setup(x => x.CardActivationAsync(It.IsAny<CardActivationParams>(), It.IsAny<CancellationToken>())).Returns(GetCardActivationResult());
            TokenOperation.Setup(x => x.GetTokenAsync(It.IsAny<RtdxGetToken.GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(GetTokenResult());
            ProcessConfig.Setup(p => p.Value).Returns(ProcessIntegrationTestsConfiguration.ReadJson<ProcessConfig>("ProcessConfig.json"));

            var command = new CreditCardActivationCommand(Logger.Object, CardReadRepository.Object, UpdateAssetClient.Object, TokenClient.Object, TokenOperation.Object, CardActivationOperation.Object, ConfigSalesforce, RtdxTokenParams, CardWriteRepository.Object, ProcessConfig.Object);

            var request = GetCreditCardActivationRequest();

            var result = await command.Handle(request, new CancellationToken());

            result.Should().NotBeNull();
        }

        [Fact(DisplayName = "Should FindByAssetId throws new exception")]
        public async void ShouldFindByAssetIdThrowsNewException()
        {
            var creditCard = GetCard();
            CardReadRepository.Setup(x => x.FindByCardId(It.IsAny<int>())).Returns(creditCard);
            TokenOperation.Setup(x => x.GetTokenAsync(It.IsAny<RtdxGetToken.GetTokenParams>(), It.IsAny<CancellationToken>())).Throws(new Exception());
            CardActivationOperation.Setup(x => x.CardActivationAsync(It.IsAny<CardActivationParams>(), It.IsAny<CancellationToken>())).Returns(GetCardActivationResult());
            CardWriteRepository.Setup(x => x.Add(It.IsAny<Card>(), It.IsAny<CancellationToken>()));
            TokenOperation.Setup(x => x.GetTokenAsync(It.IsAny<RtdxGetToken.GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(GetTokenResult());
            ProcessConfig.Setup(p => p.Value).Returns(ProcessIntegrationTestsConfiguration.ReadJson<ProcessConfig>("ProcessConfig.json"));
            var command = new CreditCardActivationCommand(Logger.Object, CardReadRepository.Object, UpdateAssetClient.Object, TokenClient.Object, TokenOperation.Object, CardActivationOperation.Object, ConfigSalesforce, RtdxTokenParams, CardWriteRepository.Object, ProcessConfig.Object);
            var request = GetCreditCardActivationRequest();

            await Assert.ThrowsAsync<UnprocessableEntityException>(() => command.Handle(request, new CancellationToken()));
        }

        [Fact(DisplayName = "Should CreditCardValidation throws new exception")]
        public async void ShouldCreditCardValidationError()
        {
            var assetResponse = GetCard();
            assetResponse.LastFour = "error";
            CardReadRepository.Setup(x => x.FindByCardId(It.IsAny<int>())).Returns(assetResponse);
            TokenOperation.Setup(x => x.GetTokenAsync(It.IsAny<RtdxGetToken.GetTokenParams>(), It.IsAny<CancellationToken>())).Throws(new Exception());
            CardWriteRepository.Setup(x => x.Add(It.IsAny<Card>(), It.IsAny<CancellationToken>()));
            CardActivationOperation.Setup(x => x.CardActivationAsync(It.IsAny<CardActivationParams>(), It.IsAny<CancellationToken>())).Returns(GetCardActivationResult());
            ProcessConfig.Setup(p => p.Value).Returns(ProcessIntegrationTestsConfiguration.ReadJson<ProcessConfig>("ProcessConfig.json"));
            var command = new CreditCardActivationCommand(Logger.Object, CardReadRepository.Object, UpdateAssetClient.Object, TokenClient.Object, TokenOperation.Object, CardActivationOperation.Object, ConfigSalesforce, RtdxTokenParams, CardWriteRepository.Object, ProcessConfig.Object);
            var request = GetCreditCardActivationRequest();

            await Assert.ThrowsAsync<UnprocessableEntityException>(() => command.Handle(request, new CancellationToken()));
        }

        [Fact(DisplayName = "Should UpdateCreditCardStatus throws new exception")]
        public async void ShouldUpdateCreditCardStatusThrowsNewException()
        {
            CardReadRepository.Setup(x => x.FindByCardId(It.IsAny<int>())).Returns(GetCard());
            TokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Throws(new Exception());
            UpdateAssetClient.Setup(x => x.UpdateAsset(It.IsAny<UpdateAssetParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetSalesforceResult());
            CardWriteRepository.Setup(x => x.Add(It.IsAny<Card>(), It.IsAny<CancellationToken>()));
            TokenOperation.Setup(x => x.GetTokenAsync(It.IsAny<RtdxGetToken.GetTokenParams>(), It.IsAny<CancellationToken>())).Throws(new Exception());
            CardActivationOperation.Setup(x => x.CardActivationAsync(It.IsAny<CardActivationParams>(), It.IsAny<CancellationToken>())).Returns(GetCardActivationResult());
            ProcessConfig.Setup(p => p.Value).Returns(ProcessIntegrationTestsConfiguration.ReadJson<ProcessConfig>("ProcessConfig.json"));
            var command = new CreditCardActivationCommand(Logger.Object, CardReadRepository.Object, UpdateAssetClient.Object, TokenClient.Object, TokenOperation.Object, CardActivationOperation.Object, ConfigSalesforce, RtdxTokenParams, CardWriteRepository.Object, ProcessConfig.Object);

            var request = GetCreditCardActivationRequest();

            await Assert.ThrowsAsync<UnprocessableEntityException>(() => command.Handle(request, new CancellationToken()));
        }

        #endregion

        #region Methods

        public Card GetCard()
        {
            return ConvertJson.ReadJson<Card>("Cards.json");
        }

        private Task<Proxy.Salesforce.Messages.BaseResult<GetTokenResult>> GetSalesforceToken()
        {
            return Task.FromResult(new System.Proxy.Salesforce.Messages.BaseResult<GetTokenResult>
            {
                IsSuccess = true,
                Result = ConvertJson.ReadJson<GetTokenResult>("GetTokenResult.json")
            });
        }

        private Task<Proxy.Salesforce.Messages.BaseResult<SalesforceResult>> GetSalesforceResult()
        {
            return Task.FromResult(new Proxy.Salesforce.Messages.BaseResult<SalesforceResult>
            {
                IsSuccess = true,
                Result = new SalesforceResult
                {
                    Success = true
                }
            });
        }

        private CreditCardActivationRequest GetCreditCardActivationRequest()
        {
            return ConvertJson.ReadJson<CreditCardActivationRequest>("CreditCardActivationRequest.json");
        }


        private Task<BaseResult<CardActivationResult>> GetCardActivationResult()
        {
            return Task.FromResult(new BaseResult<CardActivationResult>
            {
                IsSuccess = true,
                Result = new CardActivationResult
                {
                    AccountNumber = "",
                    Message = "",
                    ResponseCode = "00"
                }
            });
        }

        private Task<BaseResult<RtdxGetToken.GetTokenResult>> GetTokenResult()
        {
            return Task.FromResult(new BaseResult<RtdxGetToken.GetTokenResult>
            {
                IsSuccess = true,
                Result = new RtdxGetToken.GetTokenResult
                {
                    SecurityToken = "string"
                }
            });
        }

        #endregion
    }
}
