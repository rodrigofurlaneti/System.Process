using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Process.Application.Commands.CreditCardChangeStatus;
using System.Process.Base.UnitTests;
using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Process.Domain.ValueObjects;
using System.Phoenix.Common.Exceptions;
using System.Proxy.Fis.GetToken;
using System.Proxy.Fis.GetToken.Messages;
using System.Proxy.Fis.LockUnlock;
using System.Proxy.Fis.LockUnlock.Messages;
using System.Proxy.Fis.Messages;
using Xunit;

namespace System.Process.UnitTests.Application.Commands.CreditCardChangeStatus
{
    public class CreditCardChangeStatusCommandTests
    {
        #region Properties

        private Mock<ILogger<CreditCardChangeStatusCommand>> Logger { get; }
        private Mock<ICardReadRepository> CardReadRepository { get; }
        private Mock<ILockUnlockClient> LockUnlockClient { get; }
        private IOptions<GetTokenParams> FisTokenParams { get; }
        private Mock<IGetTokenClient> TokenClient { get; }
        private IOptions<ProcessConfig> ProcessConfig { get; set; }

        #endregion

        #region Constructor

        public CreditCardChangeStatusCommandTests()
        {
            Logger = new Mock<ILogger<CreditCardChangeStatusCommand>>();
            CardReadRepository = new Mock<ICardReadRepository>();
            LockUnlockClient = new Mock<ILockUnlockClient>();
            TokenClient = new Mock<IGetTokenClient>();
            var param = new GetTokenParams();
            FisTokenParams = Options.Create(param);
            var ProcessConfig = ProcessUnitTestsConfiguration.ReadJson<ProcessConfig>("ProcessConfig.json");
            ProcessConfig = Options.Create(ProcessConfig);
        }

        #endregion

        #region Methods

        [Fact(DisplayName = "Should Handle credit card change status Successfully")]
        public async void ShouldCreditCardChangeStatusSuccessfully()
        {
            CardReadRepository.Setup(x => x.FindByCardId(It.IsAny<int>())).Returns(GetCard());
            LockUnlockClient.Setup(x => x.LockUnlockAsync(It.IsAny<LockUnlockParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetLockUnlockResult());
            TokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(GetTokenResult());
            var command = new CreditCardChangeStatusCommand(Logger.Object, CardReadRepository.Object,
                LockUnlockClient.Object, FisTokenParams, TokenClient.Object, ProcessConfig);
            var request = ProcessUnitTestsConfiguration.ReadJson<CreditCardChangeStatusRequest>("CreditCardChangeStatusRequest.json");

            var result = await command.Handle(request, new CancellationToken());

            result.Should().NotBeNull();
        }

        [Fact(DisplayName = "Should CreditCardValidation Throws new Exception")]
        public async void ShouldCreditCardValidationThrowsException()
        {
            var creditCard = GetCard();
            creditCard = null;
            CardReadRepository.Setup(x => x.FindByCardId(It.IsAny<int>())).Returns(creditCard);
            LockUnlockClient.Setup(x => x.LockUnlockAsync(It.IsAny<LockUnlockParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetLockUnlockResult());
            TokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(GetTokenResult());
            var command = new CreditCardChangeStatusCommand(Logger.Object, CardReadRepository.Object,
                           LockUnlockClient.Object, FisTokenParams, TokenClient.Object, ProcessConfig);
            var request = ProcessUnitTestsConfiguration.ReadJson<CreditCardChangeStatusRequest>("CreditCardChangeStatusRequest.json");
            request.CardId = 0;

            await Assert.ThrowsAsync<NotFoundException>(() => command.Handle(request, new CancellationToken()));
        }

        [Fact(DisplayName = "Should FindCreditCard new Throws new Exception")]
        public async void ShouldFindCreditCardNewThrowsException()
        {
            TokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(GetTokenResult());
            var command = new CreditCardChangeStatusCommand(Logger.Object, CardReadRepository.Object,
                           LockUnlockClient.Object, FisTokenParams, TokenClient.Object, ProcessConfig);
            var request = ProcessUnitTestsConfiguration.ReadJson<CreditCardChangeStatusRequest>("CreditCardChangeStatusRequest.json");

            await Assert.ThrowsAsync<NotFoundException>(() => command.Handle(request, new CancellationToken()));
        }

        [Fact(DisplayName = "Should FindCreditCard Throws new Exception")]
        public async void ShouldFindCreditCardThrowsException()
        {
            CardReadRepository.Setup(x => x.FindByCardId(It.IsAny<int>())).Throws(new Exception());
            TokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(GetTokenResult());
            var command = new CreditCardChangeStatusCommand(Logger.Object, CardReadRepository.Object,
                                       LockUnlockClient.Object, FisTokenParams, TokenClient.Object, ProcessConfig);

            var request = ProcessUnitTestsConfiguration.ReadJson<CreditCardChangeStatusRequest>("CreditCardChangeStatusRequest.json");

            await Assert.ThrowsAsync<NotFoundException>(() => command.Handle(request, new CancellationToken()));
        }

        [Fact(DisplayName = "Should LockUnlock Throws new Exception")]
        public async void ShouldLockUnlockThrowsException()
        {
            CardReadRepository.Setup(x => x.FindByAssetId(It.IsAny<string>())).Returns(GetCard());
            TokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(GetTokenResult());
            LockUnlockClient.Setup(x => x.LockUnlockAsync(It.IsAny<LockUnlockParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Throws(new Exception());
            var command = new CreditCardChangeStatusCommand(Logger.Object, CardReadRepository.Object,
                                       LockUnlockClient.Object, FisTokenParams, TokenClient.Object, ProcessConfig);
            var request = ProcessUnitTestsConfiguration.ReadJson<CreditCardChangeStatusRequest>("CreditCardChangeStatusRequest.json");

            await Assert.ThrowsAsync<NotFoundException>(() => command.Handle(request, new CancellationToken()));
        }

        #endregion

        #region Private Methods

        private Task<BaseResult<LockUnlockResult>> GetLockUnlockResult()
        {
            return Task.FromResult(new BaseResult<LockUnlockResult>
            {
                IsSuccess = true,
                Result = ProcessUnitTestsConfiguration.ReadJson<LockUnlockResult>("LockUnlockResult.json")
            });
        }

        private Task<BaseResult<GetTokenResult>> GetTokenResult()
        {
            return Task.FromResult(new BaseResult<GetTokenResult>
            {
                IsSuccess = true,
                Result = new GetTokenResult { AccessToken = "token" }
            });
        }

        public Card GetCard()
        {
            return ProcessUnitTestsConfiguration.ReadJson<Card>("Cards.json");
        }

        #endregion
    }
}
