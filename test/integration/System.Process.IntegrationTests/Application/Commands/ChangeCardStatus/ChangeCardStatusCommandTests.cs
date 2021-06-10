using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Process.Api.Controllers.V1;
using System.Process.Application.Clients.Cards;
using System.Process.Application.Commands.ChangeCardStatus;
using System.Process.Base.IntegrationTests;
using System.Process.CrossCutting.DependencyInjection;
using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Process.Domain.ValueObjects;
using System.Process.IntegrationTests.Adapters;
using System.Process.IntegrationTests.Common;
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

namespace System.Process.IntegrationTests.Application.Commands.ChangeCardStatus
{
    public class ChangeCardStatusCommandTests
    {
        #region Properties

        private static ServiceCollection Services;
        private static ServiceProvider Provider;
        private static Mock<IHttpContextAccessor> HttpContextAccessor;

        private Mock<ILogger<ChangeCardStatusCommand>> Logger { get; }
        private Mock<IGetTokenClient> GetTokenClient { get; }
        private Mock<IOptions<GetTokenParams>> GetTokenParams { get; }
        private Mock<ILockUnlockClient> LockUnlockClient { get; }
        private Mock<ICardReadRepository> CardReadRepository { get; }
        private Mock<ICardService> CardService { get; }
        private Mock<IOptions<ProcessConfig>> ProcessConfig { get; }

        #endregion

        #region Constructor

        public ChangeCardStatusCommandTests()
        {
            Logger = new Mock<ILogger<ChangeCardStatusCommand>>();
            GetTokenClient = new Mock<IGetTokenClient>();
            GetTokenParams = new Mock<IOptions<GetTokenParams>>();
            LockUnlockClient = new Mock<ILockUnlockClient>();
            CardReadRepository = new Mock<ICardReadRepository>();
            CardService = new Mock<ICardService>();
            ProcessConfig = new Mock<IOptions<ProcessConfig>>();
            ProcessConfig.Setup(p => p.Value).Returns(ProcessIntegrationTestsConfiguration.ReadJson<ProcessConfig>("ProcessConfig.json"));

            Services = new ServiceCollection();
            Services.AddMediator();
            Services.AddScoped(typeof(ILoggerFactory), typeof(LoggerFactory));
            Services.AddScoped(typeof(ILogger<>), typeof(Logger<>));

            Provider = Services.BuildServiceProvider();
        }

        #endregion

        #region Tests

        [Trait("Integration", "Success")]
        [Fact(DisplayName = "ChangeCardStatusCommand_Success")]
        public async void ShouldChangeCardStatusSuccessfully()
        {
            var adapter = GetChangeCardStatus();

            var cancellationToken = new CancellationToken(false);

            GetTokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(GetBaseTokenResult()));
            LockUnlockClient.Setup(x => x.LockUnlockAsync(It.IsAny<LockUnlockParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetLockUnlockResult());
            CardReadRepository.Setup(x => x.Find(It.IsAny<int>())).Returns(adapter.SuccessCard);
            CardService.Setup(x => x.HandleCardUpdate(It.IsAny<Card>(), cancellationToken));


            Services.AddSingleton(Logger.Object);
            Services.AddSingleton(CardService.Object);
            Services.AddSingleton(CardReadRepository.Object);
            Services.AddSingleton(GetTokenParams.Object);
            Services.AddSingleton(GetTokenClient.Object);
            Services.AddSingleton(LockUnlockClient.Object);
            Services.AddSingleton(ProcessConfig.Object);
            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<ProcessController>>();
            var mediator = GetInstance<IMediator>();
            var controller = new ProcessController(mediator, logger.Object);
            var request = adapter.SuccessRequest;

            var result = await controller.ChangeCardStatus(request, new CancellationToken());

            Assert.IsType<OkObjectResult>(result);

        }

        [Trait("Integration", "Error")]
        [Fact(DisplayName = "ChangeCardStatusCommand_Error")]
        public async void ShouldNotChangeCardStatus()
        {
            var adapter = GetChangeCardStatus();

            CardReadRepository.Setup(x => x.Find(It.IsAny<int>())).Returns(new List<Card>());

            Services.AddSingleton(Logger.Object);
            Services.AddSingleton(CardService.Object);
            Services.AddSingleton(CardReadRepository.Object);
            Services.AddSingleton(GetTokenParams.Object);
            Services.AddSingleton(GetTokenClient.Object);
            Services.AddSingleton(LockUnlockClient.Object);
            Services.AddSingleton(ProcessConfig.Object);
            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<ProcessController>>();
            var mediator = GetInstance<IMediator>();
            var controller = new ProcessController(mediator, logger.Object);
            var request = adapter.SuccessRequest;


            await Assert.ThrowsAsync<UnprocessableEntityException>(() => controller.ChangeCardStatus(request, new CancellationToken()));
        }

        [Trait("Integration", "Error")]
        [Fact(DisplayName = "ChangeCardStatusCommand_Error - Card Not Active")]
        public async void ShouldNotChangeCardStatusCardNotActive()
        {
            var adapter = GetChangeCardStatus();
            adapter.SuccessCard[0].CardStatus = "Pending Activation";

            CardReadRepository.Setup(x => x.Find(It.IsAny<int>())).Returns(adapter.SuccessCard);

            Services.AddSingleton(Logger.Object);
            Services.AddSingleton(CardService.Object);
            Services.AddSingleton(CardReadRepository.Object);
            Services.AddSingleton(GetTokenParams.Object);
            Services.AddSingleton(GetTokenClient.Object);
            Services.AddSingleton(LockUnlockClient.Object);
            Services.AddSingleton(ProcessConfig.Object);
            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<ProcessController>>();
            var mediator = GetInstance<IMediator>();
            var controller = new ProcessController(mediator, logger.Object);
            var request = adapter.SuccessRequest;


            await Assert.ThrowsAsync<UnprocessableEntityException>(() => controller.ChangeCardStatus(request, new CancellationToken()));
        }

        [Trait("Integration", "Error")]
        [Fact(DisplayName = "ChangeCardStatusCommand_Error - Card With Same Lock Status")]
        public async void ShouldNotChangeCardStatusCardSameLockStatus()
        {
            var adapter = GetChangeCardStatus();
            adapter.SuccessCard[0].Locked = 0;
            adapter.SuccessRequest.Action = "unlock";

            CardReadRepository.Setup(x => x.Find(It.IsAny<int>())).Returns(adapter.SuccessCard);

            Services.AddSingleton(Logger.Object);
            Services.AddSingleton(CardService.Object);
            Services.AddSingleton(CardReadRepository.Object);
            Services.AddSingleton(GetTokenParams.Object);
            Services.AddSingleton(GetTokenClient.Object);
            Services.AddSingleton(LockUnlockClient.Object);
            Services.AddSingleton(ProcessConfig.Object);
            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<ProcessController>>();
            var mediator = GetInstance<IMediator>();
            var controller = new ProcessController(mediator, logger.Object);
            var request = adapter.SuccessRequest;


            await Assert.ThrowsAsync<UnprocessableEntityException>(() => controller.ChangeCardStatus(request, new CancellationToken()));
        }

        [Trait("Integration", "Error")]
        [Fact(DisplayName = "ChangeCardStatusCommand_Error - FIS error")]
        public async void ShouldNotChangeCardStatusExternalProvider()
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

            Services.AddSingleton(Logger.Object);
            Services.AddSingleton(CardService.Object);
            Services.AddSingleton(CardReadRepository.Object);
            Services.AddSingleton(GetTokenParams.Object);
            Services.AddSingleton(GetTokenClient.Object);
            Services.AddSingleton(LockUnlockClient.Object);
            Services.AddSingleton(ProcessConfig.Object);
            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<ProcessController>>();
            var mediator = GetInstance<IMediator>();
            var controller = new ProcessController(mediator, logger.Object);
            var request = adapter.SuccessRequest;


            await Assert.ThrowsAsync<UnprocessableEntityException>(() => controller.ChangeCardStatus(request, new CancellationToken()));
        }

        #endregion

        #region Methods

        public static T GetInstance<T>()
        {
            T result = Provider.GetRequiredService<T>();
            ControllerBase controllerBase = result as ControllerBase;
            if (controllerBase != null)
            {
                SetControllerContext(controllerBase);
            }
            Controller controller = result as Controller;
            if (controller != null)
            {
                SetControllerContext(controller);
            }
            return result;
        }

        private static void SetControllerContext(Controller controller)
        {
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = HttpContextAccessor.Object.HttpContext
            };
        }

        private static void SetControllerContext(ControllerBase controller)
        {
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = HttpContextAccessor.Object.HttpContext
            };
        }

        private ChangeCardStatusJsonAdapter GetChangeCardStatus()
        {
            return ConvertJson.ReadJson<ChangeCardStatusJsonAdapter>("ChangeCardStatus.json");
        }

        private Task<BaseResult<LockUnlockResult>> GetLockUnlockResult()
        {
            return Task.FromResult(new BaseResult<LockUnlockResult>
            {
                IsSuccess = true,
                Result = ProcessIntegrationTestsConfiguration.ReadJson<LockUnlockResult>("LockUnlockResult.json")
            });
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

        #endregion
    }
}
