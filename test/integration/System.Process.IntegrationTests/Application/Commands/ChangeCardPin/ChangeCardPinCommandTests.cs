using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Process.Api.Controllers.V1;
using System.Process.Application.Clients.Cards;
using System.Process.Application.Commands.ChangeCardPin;
using System.Process.Base.IntegrationTests;
using System.Process.CrossCutting.DependencyInjection;
using System.Process.Domain.Entities;
using System.Process.Domain.ValueObjects;
using System.Proxy.Fis.ActivateCard;
using System.Proxy.Fis.ActivateCard.Messages;
using System.Proxy.Fis.ChangeCardPin;
using System.Proxy.Fis.ChangeCardPin.Messages;
using System.Proxy.Fis.GetToken;
using System.Proxy.Fis.GetToken.Messages;
using System.Proxy.Fis.Messages;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace System.Process.IntegrationTests.Application.Commands.ChangeCardPin
{
    public class ChangeCardPinCommandTests
    {
        #region Properties

        private static ServiceCollection Services;
        private static ServiceProvider Provider;
        private static Mock<IHttpContextAccessor> HttpContextAccessor;

        private Mock<ILogger<ChangeCardPinCommand>> Logger { get; }
        private Mock<IGetTokenClient> GetTokenClient { get; }
        private Mock<IOptions<GetTokenParams>> GetTokenParams { get; }
        private Mock<IChangeCardPinClient> ChangeCardPinClient { get; }
        private Mock<IActivateCardClient> ActivateCardClient { get; }
        private Mock<ICardService> CardService { get; }
        private Mock<IOptions<ProcessConfig>> ProcessConfig { get; }

        #endregion

        #region Constructor

        public ChangeCardPinCommandTests()
        {
            Logger = new Mock<ILogger<ChangeCardPinCommand>>();
            GetTokenClient = new Mock<IGetTokenClient>();
            GetTokenParams = new Mock<IOptions<GetTokenParams>>();
            ChangeCardPinClient = new Mock<IChangeCardPinClient>();
            ActivateCardClient = new Mock<IActivateCardClient>();
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
        [Fact(DisplayName = "ChangeCardPinCommand_Success")]
        public async void ShouldChangeCardPinSuccessfully()
        {
            IList<Card> cards = new List<Card>
            {
                new Card
                {
                    CardId = 1,
                    CustomerId = "SP-340",
                    ExpirationDate = "2312",
                    LastFour = "3133",
                    CardType = "DR",
                    Validated = 1,
                    Pan = "1397587290620175"
                }
            };

            CardService
                .Setup(r => r.FindByCardId(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(cards));
            GetTokenClient
                .Setup(r => r.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(GetBaseTokenResult()));
            ChangeCardPinClient
                .Setup(r => r.ChangeCardPinAsync(It.IsAny<ChangeCardPinParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(new BaseResult<ChangeCardPinResult>
                {
                    IsSuccess = true
                }));
            ActivateCardClient
                .Setup(r => r.ActivateCard(It.IsAny<ActivateCardParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(new BaseResult<ActivateCardResult>
                {
                    IsSuccess = true,
                    Result = new ActivateCardResult
                    {
                        Metadata = new Metadata
                        {
                            Messages = new List<Message>
                            {
                                new Message { Code = "00", Text = "Text" }
                            }
                        }
                    }
                }));
            CardService
                .Setup(r => r.HandleCardUpdate(It.IsAny<Card>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(cards[0]));

            Services.AddSingleton(Logger.Object);
            Services.AddSingleton(ChangeCardPinClient.Object);
            Services.AddSingleton(ActivateCardClient.Object);
            Services.AddSingleton(GetTokenParams.Object);
            Services.AddSingleton(GetTokenClient.Object);
            Services.AddSingleton(CardService.Object);
            Services.AddSingleton(ProcessConfig.Object);
            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<CardsController>>();
            var mediator = GetInstance<IMediator>();
            var controller = new CardsController(mediator, logger.Object, CardService.Object);
            var request = new ChangeCardPinRequest
            {
                CardId = 1,
                CustomerId = "SP-340",
                NewPin = "1234"
            };

            var result = await controller.ChangeCardPin(request, new CancellationToken());

            Assert.IsType<OkObjectResult>(result);

        }

        [Trait("Integration", "Error")]
        [Fact(DisplayName = "ChangeCardPinCommand_Error - Handle should throw 'No card was found' if no card was found")]
        public async void ShouldNotChangeCardPinNotFound()
        {
            IList<Card> cards = new List<Card>();

            var cancellationToken = new CancellationToken();

            CardService
                .Setup(r => r.FindByCardId(It.IsAny<string>(), It.IsAny<int>(), cancellationToken)).Returns(Task.FromResult(cards));

            Services.AddSingleton(Logger.Object);
            Services.AddSingleton(ChangeCardPinClient.Object);
            Services.AddSingleton(ActivateCardClient.Object);
            Services.AddSingleton(GetTokenParams.Object);
            Services.AddSingleton(GetTokenClient.Object);
            Services.AddSingleton(CardService.Object);
            Services.AddSingleton(ProcessConfig.Object);
            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<CardsController>>();
            var mediator = GetInstance<IMediator>();
            var controller = new CardsController(mediator, logger.Object, CardService.Object);
            var request = new ChangeCardPinRequest
            {
                CardId = 1,
                CustomerId = "SP-340",
                NewPin = "1234"
            };

            try
            {
                await controller.ChangeCardPin(request, new CancellationToken());
            }
            catch (Exception ex)
            {
                Assert.Equal("No card was found", ex.Message);
            }

        }

        [Trait("Integration", "Error")]
        [Fact(DisplayName = "ChangeCardPinCommand_Error - Handle should throw 'This resource needs Card Activation before Pin Creation' if card isn't validated")]
        public async void ShouldNotChangeCardPinNotValidated()
        {
            IList<Card> cards = new List<Card>
            {
                new Card
                {
                    CardId = 1,
                    CustomerId = "SP-340",
                    ExpirationDate = "2312",
                    LastFour = "3133",
                    CardType = "DR",
                    Validated = 0
                }
            };

            var cancellationToken = new CancellationToken();

            CardService
                .Setup(r => r.FindByCardId(It.IsAny<string>(), It.IsAny<int>(), cancellationToken)).Returns(Task.FromResult(cards));

            Services.AddSingleton(Logger.Object);
            Services.AddSingleton(ChangeCardPinClient.Object);
            Services.AddSingleton(ActivateCardClient.Object);
            Services.AddSingleton(GetTokenParams.Object);
            Services.AddSingleton(GetTokenClient.Object);
            Services.AddSingleton(CardService.Object);
            Services.AddSingleton(ProcessConfig.Object);
            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<CardsController>>();
            var mediator = GetInstance<IMediator>();
            var controller = new CardsController(mediator, logger.Object, CardService.Object);
            var request = new ChangeCardPinRequest
            {
                CardId = 1,
                CustomerId = "SP-340",
                NewPin = "1234"
            };

            try
            {
                await controller.ChangeCardPin(request, new CancellationToken());
            }
            catch (Exception ex)
            {
                Assert.Equal("This resource needs Card Activation before Pin Creation", ex.Message);
            }

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

        private BaseResult<GetTokenResult> GetBaseTokenResult()
        {
            return new BaseResult<GetTokenResult>
            {
                IsSuccess = true,
                Result = new GetTokenResult
                {
                    AccessToken = "AccessToken"
                }
            };
        }

        #endregion
    }
}
