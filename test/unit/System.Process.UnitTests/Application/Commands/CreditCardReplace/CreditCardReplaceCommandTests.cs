using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Process.Application.Commands.CreditCardReplace;
using System.Process.Base.UnitTests;
using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Process.Domain.ValueObjects;
using System.Process.UnitTests.Common;
using System.Phoenix.Common.Exceptions;
using System.Proxy.Rtdx.GetToken;
using System.Proxy.Rtdx.GetToken.Messages;
using System.Proxy.Rtdx.Messages;
using System.Proxy.Rtdx.OrderNewPlastic;
using System.Proxy.Rtdx.OrderNewPlastic.Messages;
using Xunit;

namespace System.Process.UnitTests.Application.Commands.CreditCardReplace
{
    public class CreditCardReplaceCommandTests
    {
        #region Properties

        private Mock<ILogger<CreditCardReplaceCommand>> Logger { get; }
        private Mock<ICardReadRepository> CardReadRepository { get; }
        private Mock<IOrderNewPlasticOperation> OrderNewPlasticOperation { get; }
        private Mock<IGetTokenOperation> GetTokenOperation { get; }
        private IOptions<GetTokenParams> RtdxTokenParams { get; }
        private IOptions<ProcessConfig> ProcessConfig { get; }

        #endregion

        #region Constructor

        public CreditCardReplaceCommandTests()
        {
            Logger = new Mock<ILogger<CreditCardReplaceCommand>>();
            CardReadRepository = new Mock<ICardReadRepository>();
            OrderNewPlasticOperation = new Mock<IOrderNewPlasticOperation>();
            GetTokenOperation = new Mock<IGetTokenOperation>();
            var ProcessConfig = ProcessUnitTestsConfiguration.ReadJson<ProcessConfig>("ProcessConfig.json");
            ProcessConfig = Options.Create(ProcessConfig);
            var param = new GetTokenParams();
            RtdxTokenParams = Options.Create(param);
        }

        #endregion

        #region Methods

        [Fact(DisplayName = "Should Order New Credit card Balance Successfully")]
        public async void ShouldOrderNewCreditCardSuccessfully()
        {
            CardReadRepository.Setup(x => x.FindByCardId(It.IsAny<int>())).Returns(GetCard());
            OrderNewPlasticOperation.Setup(x => x.OrderNewPlasticAsync(It.IsAny<OrderNewPlasticParams>(), It.IsAny<CancellationToken>())).Returns(OrderNewPlastic());
            GetTokenOperation.Setup(x => x.GetTokenAsync(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(GetTokenResult());
            var command = new CreditCardReplaceCommand(Logger.Object, CardReadRepository.Object,
                OrderNewPlasticOperation.Object, GetTokenOperation.Object, RtdxTokenParams, ProcessConfig);
            var request = ConvertJson.ReadJson<CreditCardReplaceRequest>("CreditCardReplaceRequest.json");

            var result = await command.Handle(request, new CancellationToken());

            result.Should().NotBeNull();
        }

        [Fact(DisplayName = "Should CreditCardValidation Throws new Exception")]
        public async void ShouldCreditCardValidationThrowsException()
        {
            var creditCard = GetCard();
            creditCard.CardStatus = "error";
            CardReadRepository.Setup(x => x.FindByCardId(It.IsAny<int>())).Returns(creditCard);
            OrderNewPlasticOperation.Setup(x => x.OrderNewPlasticAsync(It.IsAny<OrderNewPlasticParams>(), It.IsAny<CancellationToken>())).Returns(OrderNewPlastic());
            var command = new CreditCardReplaceCommand(Logger.Object, CardReadRepository.Object,
                OrderNewPlasticOperation.Object, GetTokenOperation.Object, RtdxTokenParams, ProcessConfig);
            var request = ConvertJson.ReadJson<CreditCardReplaceRequest>("CreditCardReplaceRequest.json");

            await Assert.ThrowsAsync<UnprocessableEntityException>(() => command.Handle(request, new CancellationToken()));
        }

        [Fact(DisplayName = "Should FindCreditCard new Throws new Exception")]
        public async void ShouldFindCreditCardNewThrowsException()
        {
            var command = new CreditCardReplaceCommand(Logger.Object, CardReadRepository.Object, OrderNewPlasticOperation.Object,
                GetTokenOperation.Object, RtdxTokenParams, ProcessConfig);
            var request = ConvertJson.ReadJson<CreditCardReplaceRequest>("CreditCardReplaceRequest.json");

            await Assert.ThrowsAsync<UnprocessableEntityException>(() => command.Handle(request, new CancellationToken()));
        }

        [Fact(DisplayName = "Should FindCreditCard Throws new Exception")]
        public async void ShouldFindCreditCardThrowsException()
        {
            CardReadRepository.Setup(x => x.FindByCardId(It.IsAny<int>())).Throws(new Exception());
            var command = new CreditCardReplaceCommand(Logger.Object, CardReadRepository.Object,
                OrderNewPlasticOperation.Object, GetTokenOperation.Object, RtdxTokenParams, ProcessConfig);
            var request = ConvertJson.ReadJson<CreditCardReplaceRequest>("CreditCardReplaceRequest.json");

            await Assert.ThrowsAsync<UnprocessableEntityException>(() => command.Handle(request, new CancellationToken()));
        }

        [Fact(DisplayName = "Should FindByAssetId Throws new Exception")]
        public async void ShouldFindByAssetIdThrowsException()
        {
            Card creditCard = null;
            CardReadRepository.Setup(x => x.FindByCardId(It.IsAny<int>())).Returns(creditCard);
            var command = new CreditCardReplaceCommand(Logger.Object, CardReadRepository.Object,
                OrderNewPlasticOperation.Object, GetTokenOperation.Object, RtdxTokenParams, ProcessConfig);
            var request = ConvertJson.ReadJson<CreditCardReplaceRequest>("CreditCardReplaceRequest.json");

            await Assert.ThrowsAsync<UnprocessableEntityException>(() => command.Handle(request, new CancellationToken()));
        }

        [Fact(DisplayName = "Should CreditReplace Throws new Exception")]
        public async void ShouldCreditReplaceThrowsException()
        {
            CardReadRepository.Setup(x => x.FindByAssetId(It.IsAny<string>())).Returns(GetCard());
            OrderNewPlasticOperation.Setup(x => x.OrderNewPlasticAsync(It.IsAny<OrderNewPlasticParams>(), It.IsAny<CancellationToken>())).Throws(new Exception());
            var command = new CreditCardReplaceCommand(Logger.Object, CardReadRepository.Object,
                OrderNewPlasticOperation.Object, GetTokenOperation.Object, RtdxTokenParams, ProcessConfig);
            var request = ConvertJson.ReadJson<CreditCardReplaceRequest>("CreditCardReplaceRequest.json");


            await Assert.ThrowsAsync<UnprocessableEntityException>(() => command.Handle(request, new CancellationToken()));
        }

        #endregion

        #region Private Methods

        private Task<BaseResult<OrderNewPlasticResult>> OrderNewPlastic()
        {
            return Task.FromResult(new BaseResult<OrderNewPlasticResult>
            {
                IsSuccess = true,
                Result = ConvertJson.ReadJson<OrderNewPlasticResult>("OrderNewPlasticResult.json")
            });
        }

        private Task<BaseResult<GetTokenResult>> GetTokenResult()
        {
            return Task.FromResult(new BaseResult<GetTokenResult>
            {
                IsSuccess = true,
                Result = new GetTokenResult { SecurityToken = "token" }
            });
        }

        public Card GetCard()
        {
            return ConvertJson.ReadJson<Card>("Cards.json");
        }

        #endregion
    }
}
