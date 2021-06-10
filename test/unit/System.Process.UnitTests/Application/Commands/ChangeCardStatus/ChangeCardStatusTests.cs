using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Process.Application.Clients.Cards;
using System.Process.Application.Commands.ChangeCardStatus;
using System.Process.Base.IntegrationTests;
using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Process.Domain.ValueObjects;
using System.Process.UnitTests.Adapters;
using System.Process.UnitTests.Common;
using System.Phoenix.Common.Exceptions;
using System.Proxy.Fis.ChangeCardStatus;
using System.Proxy.Fis.ChangeCardStatus.Messages;
using System.Proxy.Fis.GetToken;
using System.Proxy.Fis.GetToken.Messages;
using System.Proxy.Fis.LockUnlock;
using System.Proxy.Fis.LockUnlock.Messages;
using System.Proxy.Fis.Messages;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace System.Process.UnitTests.Application.Commands.ChangeCardStatus
{
    public class ChangeCardStatusTests
    {
        #region Properties

        private Mock<ILogger<ChangeCardStatusCommand>> Logger { get; }
        private Mock<IGetTokenClient> GetTokenClient { get; }
        private Mock<IOptions<GetTokenParams>> GetTokenParams { get; }
        private Mock<ILockUnlockClient> LockUnlockClient { get; }
        private Mock<ICardReadRepository> CardReadRepository { get; }
        private Mock<ICardService> CardService { get; }
        private ChangeCardStatusCommand ChangeCardStatusCommand { get; }
        private Mock<IOptions<ProcessConfig>> ProcessConfig { get; }
        #endregion

        #region Constructor

        public ChangeCardStatusTests()
        {
            Logger = new Mock<ILogger<ChangeCardStatusCommand>>();
            GetTokenClient = new Mock<IGetTokenClient>();
            GetTokenParams = new Mock<IOptions<GetTokenParams>>();
            LockUnlockClient = new Mock<ILockUnlockClient>();
            CardReadRepository = new Mock<ICardReadRepository>();
            CardService = new Mock<ICardService>();
            ProcessConfig = new Mock<IOptions<ProcessConfig>>();
            ProcessConfig.Setup(p => p.Value).Returns(ProcessIntegrationTestsConfiguration.ReadJson<ProcessConfig>("ProcessConfig.json"));

            ChangeCardStatusCommand = new ChangeCardStatusCommand(
                Logger.Object,
                GetTokenClient.Object,
                GetTokenParams.Object,
                LockUnlockClient.Object,
                CardReadRepository.Object,
                CardService.Object,
                ProcessConfig.Object);
        }

        #endregion

        #region Tests

        [Fact(DisplayName = "Should Send Handle Change Card Status Successfully")]
        public async Task ShouldSendHandleChangeCardStatusAsync()
        {
            var adapter = GetChangeCardStatus();
            var response = new BaseResult<ChangeCardStatusResult>
            {
                IsSuccess = true,
                Result = adapter.SuccessResult
            };

            var cancellationToken = new CancellationToken(false);

            GetTokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(GetBaseTokenResult()));
            LockUnlockClient.Setup(x => x.LockUnlockAsync(It.IsAny<LockUnlockParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetLockUnlockResult());
            CardReadRepository.Setup(x => x.Find(It.IsAny<int>())).Returns(adapter.SuccessCard);
            CardService.Setup(x => x.HandleCardUpdate(It.IsAny<Card>(), cancellationToken));

            var request = adapter.SuccessRequest;

            var result = await ChangeCardStatusCommand.Handle(request, cancellationToken);

            Assert.IsType<ChangeCardStatusResponse>(result);
            Assert.NotNull(result);
        }

        [Fact(DisplayName = "Should throw Unprocessable Entity Exception for Card not found")]
        public async Task ShouldSendUnprocessableEntityExceptionCardNotFound()
        {
            var adapter = GetChangeCardStatus();

            CardReadRepository.Setup(x => x.Find(It.IsAny<int>())).Returns(new List<Card>());

            var cancellationToken = new CancellationToken(false);

            await Assert.ThrowsAsync<UnprocessableEntityException>(() => ChangeCardStatusCommand.Handle(adapter.SuccessRequest, cancellationToken));
        }

        [Fact(DisplayName = "Should throw Unprocessable Entity Exception for Card not active")]
        public async Task ShouldSendUnprocessableEntityExceptionCardNotActive()
        {
            var adapter = GetChangeCardStatus();
            adapter.SuccessCard[0].CardStatus = "Pending Activation";

            CardReadRepository.Setup(x => x.Find(It.IsAny<int>())).Returns(adapter.SuccessCard);

            var cancellationToken = new CancellationToken(false);

            await Assert.ThrowsAsync<UnprocessableEntityException>(() => ChangeCardStatusCommand.Handle(adapter.SuccessRequest, cancellationToken));
        }

        [Fact(DisplayName = "Should throw Unprocessable Entity Exception for Card with same lock status")]
        public async Task ShouldSendUnprocessableEntityExceptionCardSameLockStatus()
        {
            var adapter = GetChangeCardStatus();
            adapter.SuccessCard[0].Locked = 0;
            adapter.SuccessRequest.Action = "unlock";

            CardReadRepository.Setup(x => x.Find(It.IsAny<int>())).Returns(adapter.SuccessCard);

            var cancellationToken = new CancellationToken(false);

            await Assert.ThrowsAsync<UnprocessableEntityException>(() => ChangeCardStatusCommand.Handle(adapter.SuccessRequest, cancellationToken));
        }

        [Fact(DisplayName = "Should throw Unprocessable Entity Exception for FIS error")]
        public async Task ShouldSendUnprocessableEntityExceptionExternalProvider()
        {
            var adapter = GetChangeCardStatus();
            var response = new BaseResult<LockUnlockResult>
            {
                IsSuccess = false
            };

            var cancellationToken = new CancellationToken(false);

            GetTokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(GetBaseTokenResult()));
            LockUnlockClient.Setup(x => x.LockUnlockAsync(It.IsAny<LockUnlockParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(response));
            CardReadRepository.Setup(x => x.Find(It.IsAny<int>())).Returns(adapter.SuccessCard);
            CardService.Setup(x => x.HandleCardUpdate(It.IsAny<Card>(), cancellationToken));

            var request = adapter.SuccessRequest;

            await Assert.ThrowsAsync<UnprocessableEntityException>(() => ChangeCardStatusCommand.Handle(adapter.SuccessRequest, cancellationToken));
        }
        #endregion

        #region Methods

        private ChangeCardStatusJsonAdapter GetChangeCardStatus()
        {
            return ConvertJson.ReadJson<ChangeCardStatusJsonAdapter>("ChangeCardStatus.json");
        }

        private BaseResult<GetTokenResult> GetBaseTokenResult()
        {
            return new BaseResult<GetTokenResult>
            {
                IsSuccess = true,
                Result = new GetTokenResult
                {
                    AccessToken = "D923GDM9-2943-9385-6B98-HFJC8742X901"
                }
            };
        }

        private Task<BaseResult<LockUnlockResult>> GetLockUnlockResult()
        {
            return Task.FromResult(new BaseResult<LockUnlockResult>
            {
                IsSuccess = true,
                Result = ProcessIntegrationTestsConfiguration.ReadJson<LockUnlockResult>("LockUnlockResult.json")
            });
        }

        #endregion
    }
}
