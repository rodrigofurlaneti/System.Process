using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Process.Api.Controllers.V1;
using System.Process.Application.Clients.Cards;
using System.Process.Application.Commands.CreditCardChangeStatus;
using System.Process.Base.IntegrationTests;
using System.Process.CrossCutting.DependencyInjection;
using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Process.Domain.ValueObjects;
using System.Process.Infrastructure.Repositories.EntityFramework;
using System.Proxy.Fis.GetToken;
using System.Proxy.Fis.GetToken.Messages;
using System.Proxy.Fis.LockUnlock;
using System.Proxy.Fis.LockUnlock.Messages;
using System.Proxy.Fis.Messages;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace System.Process.IntegrationTests.Application.Commands.CreditCardChangeStatus
{
    public class CreditCardChangeStatusCommandTests
    {
        #region Properties

        private static ServiceProvider Provider;
        private static Mock<IHttpContextAccessor> HttpContextAccessor;
        private static ServiceCollection Services;

        private Mock<ILogger<CreditCardChangeStatusCommand>> Logger { get; set; }
        private Mock<ICardReadRepository> CardReadRepository { get; set; }
        private Mock<ILockUnlockClient> LockUnlockClient { get; set; }
        private IOptions<GetTokenParams> GetTokenParams { get; set; }
        private IOptions<ProcessConfig> ProcessConfig { get; set; }
        private Mock<IGetTokenClient> TokenClient { get; set; }
        private Mock<ICardService> CardService { get; set; }

        #endregion

        #region Constructor

        static CreditCardChangeStatusCommandTests()
        {
            Services = new ServiceCollection();
            Services.AddMediator();
            Services.AddScoped(typeof(ILoggerFactory), typeof(LoggerFactory));
            Services.AddScoped(typeof(ILogger<>), typeof(Logger<>));
            Services.AddScoped<ICardWriteRepository, CardWriteRepository>();
            Services.AddScoped<IGetTokenClient, GetTokenClient>();


            Provider = Services.BuildServiceProvider();
        }

        #endregion

        #region Main Flow

        [Fact(DisplayName = "Main Flow - Success")]
        public async void ShouldCreditCardBalanceSuccessfully()
        {
            CardService = new Mock<ICardService>();
            Logger = new Mock<ILogger<CreditCardChangeStatusCommand>>();
            CardReadRepository = new Mock<ICardReadRepository>();
            LockUnlockClient = new Mock<ILockUnlockClient>();
            TokenClient = new Mock<IGetTokenClient>();
            var param = new GetTokenParams();
            GetTokenParams = Options.Create(param);
            var ProcessConfig = ProcessIntegrationTestsConfiguration.ReadJson<ProcessConfig>("ProcessConfig.json");
            ProcessConfig = Options.Create(ProcessConfig);

            var creditCard = GetCard();
            creditCard.CardStatus = "Active";
            CardReadRepository.Setup(x => x.FindByCardId(It.IsAny<int>())).Returns(creditCard);
            LockUnlockClient.Setup(x => x.LockUnlockAsync(It.IsAny<LockUnlockParams>(), It.IsAny<string>(),It.IsAny<CancellationToken>())).Returns(GetLockUnlockResult());
            TokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(GetTokenResult());

            Services.AddSingleton(Logger.Object);
            Services.AddSingleton(CardReadRepository.Object);
            Services.AddSingleton(LockUnlockClient.Object);
            Services.AddSingleton(TokenClient.Object);
            Services.AddSingleton(GetTokenParams);
            Services.AddSingleton(ProcessConfig);

            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<CardsController>>();
            var mediator = GetInstance<IMediator>();
            var controller = new CardsController(mediator, logger.Object, CardService.Object);
            var request = new CreditCardChangeStatusRequest
            {
                CardId = 10,
                Action = "lock"
            };
            var result = await controller.ChangeCreditCardStatus(request, new CancellationToken());

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region Methods

        public static T GetInstance<T>()
        {
            T result = Provider.GetRequiredService<T>();
            if (result is ControllerBase controllerBase)
            {
                SetControllerContext(controllerBase);
            }
            if (result is Controller controller)
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

        private Task<BaseResult<LockUnlockResult>> GetLockUnlockResult()
        {
            return Task.FromResult(new BaseResult<LockUnlockResult>
            {
                IsSuccess = true,
                Result = ProcessIntegrationTestsConfiguration.ReadJson<LockUnlockResult>("LockUnlockResult.json")
            });
        }

        private Task<BaseResult<GetTokenResult>> GetTokenResult()
        {
            return Task.FromResult(new BaseResult<GetTokenResult>
            {
                IsSuccess = true,
                Result = new GetTokenResult { AccessToken = "token", TokenType = "a", ExpiresIn = 123312, Scope = "a" }
            });
        }

        public Card GetCard()
        {
            return ProcessIntegrationTestsConfiguration.ReadJson<Card>("Cards.json");
        }

        #endregion
    }
}
