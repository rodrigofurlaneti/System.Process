using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Process.Api.Controllers.V1;
using System.Process.Application.Clients.Cards;
using System.Process.Application.Commands.CreditCardReplace;
using System.Process.Base.IntegrationTests;
using System.Process.CrossCutting.DependencyInjection;
using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Process.Domain.ValueObjects;
using System.Process.Infrastructure.Repositories.EntityFramework;
using System.Proxy.Rtdx.GetToken;
using System.Proxy.Rtdx.GetToken.Messages;
using System.Proxy.Rtdx.Messages;
using System.Proxy.Rtdx.OrderNewPlastic;
using System.Proxy.Rtdx.OrderNewPlastic.Messages;
using Xunit;

namespace System.Process.IntegrationTests.Application.Commands.CreditCardReplace
{
    public class CreditCardReplaceCommandTests
    {
        #region Properties

        private static ServiceProvider Provider;
        private static Mock<IHttpContextAccessor> HttpContextAccessor;
        private static ServiceCollection Services;

        private Mock<ICardReadRepository> CardReadRepository { get; set; }
        private Mock<ILogger<CreditCardReplaceCommand>> Logger { get; set; }
        private Mock<IOrderNewPlasticOperation> OrderNewPlasticOperation { get; set; }
        private Mock<IGetTokenOperation> GetTokenOperation { get; set; }
        private Mock<IOptions<GetTokenParams>> GetTokenParams { get; set; }
        private IOptions<ProcessConfig> ProcessConfig { get; set; }
        private Mock<ICardService> CardService { get; set; }

        #endregion

        #region Constructor

        public CreditCardReplaceCommandTests()
        {
            Services = new ServiceCollection();
            Services.AddMediator();
            Services.AddScoped(typeof(ILoggerFactory), typeof(LoggerFactory));
            Services.AddScoped(typeof(ILogger<>), typeof(Logger<>));
            Services.AddScoped<ICardWriteRepository, CardWriteRepository>();

            Provider = Services.BuildServiceProvider();
        }

        #endregion

        #region Main Flow

        [Fact(DisplayName = "Main Flow - Success")]
        public async void ShouldCreditCardBalanceSuccessfully()
        {
            CardService = new Mock<ICardService>();
            Logger = new Mock<ILogger<CreditCardReplaceCommand>>();
            CardReadRepository = new Mock<ICardReadRepository>();
            OrderNewPlasticOperation = new Mock<IOrderNewPlasticOperation>();
            GetTokenOperation = new Mock<IGetTokenOperation>();
            GetTokenParams = new Mock<IOptions<GetTokenParams>>();
            var ProcessConfig = ProcessIntegrationTestsConfiguration.ReadJson<ProcessConfig>("ProcessConfig.json");
            ProcessConfig = Options.Create(ProcessConfig);

            var creditCard = GetCard();
            creditCard.CardStatus = "Active";
            CardReadRepository.Setup(x => x.FindByCardId(It.IsAny<int>())).Returns(creditCard);
            OrderNewPlasticOperation.Setup(x => x.OrderNewPlasticAsync(It.IsAny<OrderNewPlasticParams>(), It.IsAny<CancellationToken>())).Returns(ReplaceCreditCard());
            GetTokenOperation.Setup(x => x.GetTokenAsync(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(GetTokenResult());

            Services.AddSingleton(Logger.Object);
            Services.AddSingleton(CardReadRepository.Object);
            Services.AddSingleton(OrderNewPlasticOperation.Object);
            Services.AddSingleton(GetTokenOperation.Object);
            Services.AddSingleton(GetTokenParams.Object);
            Services.AddSingleton(ProcessConfig);

            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<CardsController>>();
            var mediator = GetInstance<IMediator>();
            var controller = new CardsController(mediator, logger.Object, CardService.Object);
            var result = await controller.CreditCardReplace(new CreditCardReplaceRequest
            {
                CardId = 12,
                Reason = "damaged",
                AddressType = "home"
            }, new CancellationToken());

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

        private Task<BaseResult<OrderNewPlasticResult>> ReplaceCreditCard()
        {
            return Task.FromResult(new BaseResult<OrderNewPlasticResult>
            {
                IsSuccess = true,
                Result = ProcessIntegrationTestsConfiguration.ReadJson<OrderNewPlasticResult>("OrderNewPlasticResult.json")
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
            return ProcessIntegrationTestsConfiguration.ReadJson<Card>("Cards.json");
        }

        #endregion
    }
}
