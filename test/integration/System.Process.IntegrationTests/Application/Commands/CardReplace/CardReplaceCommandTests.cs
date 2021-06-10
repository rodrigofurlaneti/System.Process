using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Process.Api.Controllers.V1;
using System.Process.Application.Clients.Cards;
using System.Process.Application.Commands.CardReplace;
using System.Process.Base.IntegrationTests;
using System.Process.CrossCutting.DependencyInjection;
using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Process.Domain.ValueObjects;
using System.Process.IntegrationTests.Adapters;
using System.Process.IntegrationTests.Common;
using System.Phoenix.Common.Exceptions;
using System.Proxy.Fis.CardReplace;
using System.Proxy.Fis.CardReplace.Messages;
using System.Proxy.Fis.CardReplace.Messages.Result;
using System.Proxy.Fis.GetCard;
using System.Proxy.Fis.GetCard.Messages;
using System.Proxy.Fis.GetToken;
using System.Proxy.Fis.GetToken.Messages;
using System.Proxy.Fis.Messages;
using System.Proxy.Fis.ReissueCard;
using System.Proxy.Fis.ReissueCard.Messages;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace System.Process.IntegrationTests.Application.Commands.CardReplace
{
    public class CardReplaceCommandTests
    {
        #region Properties

        private static ServiceCollection Services;
        private static ServiceProvider Provider;
        private static Mock<IHttpContextAccessor> HttpContextAccessor;

        private Mock<IGetTokenClient> GetTokenClient { get; }
        private Mock<IOptions<GetTokenParams>> GetTokenParams { get; }
        private Mock<ICardReadRepository> CardReadRepository { get; }
        private Mock<ICardService> CardService { get; }
        private Mock<ILogger<CardReplaceCommand>> Logger { get; }
        private Mock<ICardReplaceClient> CardReplaceClient { get; }
        private Mock<IReissueCardClient> ReissueCardClient { get; }
        private Mock<IOptions<ProcessConfig>> ProcessConfig { get; }
        private Mock<IGetCardClient> GetCardClient { get; }

        #endregion

        #region Constructor

        public CardReplaceCommandTests()
        {
            Logger = new Mock<ILogger<CardReplaceCommand>>();
            GetTokenClient = new Mock<IGetTokenClient>();
            GetTokenParams = new Mock<IOptions<GetTokenParams>>();
            CardReplaceClient = new Mock<ICardReplaceClient>();
            ReissueCardClient = new Mock<IReissueCardClient>();
            CardReadRepository = new Mock<ICardReadRepository>();
            GetCardClient = new Mock<IGetCardClient>();
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
        [Fact(DisplayName = "CardReplaceCommand_Success")]
        public async void ShouldReplaceCardSuccessfully()
        {
            var adapterReplace = GetCardReplace();
            var responseRepelace = new BaseResult<CardReplaceResult>
            {
                IsSuccess = true,
                Result = adapterReplace.SuccessResult
            };

            CardReadRepository.Setup(x => x.Find(It.IsAny<int>())).Returns(adapterReplace.SuccessRepositoryResponse);
            GetTokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(GetBaseTokenResult()));
            CardReplaceClient.Setup(x => x.CardReplaceAsync(It.IsAny<CardReplaceParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                                    .Returns(Task.FromResult(responseRepelace));
            ReissueCardClient.Setup(x => x.ReissueCardAsync(It.IsAny<ReissueCardParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(ReissueCardResult("00")));
            GetCardClient.Setup(x => x.GetCardAsync(It.IsAny<GetCardParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(GetCardResult()));

            Services.AddSingleton(Logger.Object);
            Services.AddSingleton(CardReplaceClient.Object);
            Services.AddSingleton(CardReadRepository.Object);
            Services.AddSingleton(GetTokenParams.Object);
            Services.AddSingleton(GetTokenClient.Object);
            Services.AddSingleton(ReissueCardClient.Object);
            Services.AddSingleton(CardService.Object);
            Services.AddSingleton(ProcessConfig.Object);
            Services.AddSingleton(GetCardClient.Object);
            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<ProcessController>>();
            var mediator = GetInstance<IMediator>();
            var controller = new ProcessController(mediator, logger.Object);
            var request = adapterReplace.SuccessRequest;

            var result = await controller.CardReplace(request, new CancellationToken());

            Assert.IsType<OkObjectResult>(result);

        }

        [Trait("Integration", "Error")]
        [Fact(DisplayName = "CardReplaceCommand_Error - Should throw Not Found Exception")]
        public async void ShouldNotReplaceCardNotFound()
        {
            var adapter = GetCardReplace();
            CardReadRepository.Setup(x => x.Find(It.IsAny<int>())).Returns(new List<Card>());

            Services.AddSingleton(Logger.Object);
            Services.AddSingleton(CardReplaceClient.Object);
            Services.AddSingleton(CardReadRepository.Object);
            Services.AddSingleton(GetTokenParams.Object);
            Services.AddSingleton(GetTokenClient.Object);
            Services.AddSingleton(ReissueCardClient.Object);
            Services.AddSingleton(CardService.Object);
            Services.AddSingleton(ProcessConfig.Object);
            Services.AddSingleton(GetCardClient.Object);
            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<ProcessController>>();
            var mediator = GetInstance<IMediator>();
            var controller = new ProcessController(mediator, logger.Object);
            var request = adapter.SuccessRequest;

            await Assert.ThrowsAsync<NotFoundException>(() => controller.CardReplace(request, new CancellationToken()));
        }

        [Trait("Integration", "Error")]
        [Fact(DisplayName = "CardReplaceCommand_Error - Should throw Unprocessable Entity Exception for not match card information")]
        public async void ShouldNotReplaceCardUnprocessableEntity()
        {
            var adapter = GetCardReplace();
            adapter.SuccessRepositoryResponse[0].LastFour = "";

            CardReadRepository.Setup(x => x.Find(It.IsAny<int>())).Returns(adapter.SuccessRepositoryResponse);

            Services.AddSingleton(Logger.Object);
            Services.AddSingleton(CardReplaceClient.Object);
            Services.AddSingleton(CardReadRepository.Object);
            Services.AddSingleton(GetTokenParams.Object);
            Services.AddSingleton(GetTokenClient.Object);
            Services.AddSingleton(ReissueCardClient.Object);
            Services.AddSingleton(CardService.Object);
            Services.AddSingleton(ProcessConfig.Object);
            Services.AddSingleton(GetCardClient.Object);
            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<ProcessController>>();
            var mediator = GetInstance<IMediator>();
            var controller = new ProcessController(mediator, logger.Object);
            var request = adapter.SuccessRequest;

            await Assert.ThrowsAsync<UnprocessableEntityException>(() => controller.CardReplace(request, new CancellationToken()));
        }

        [Trait("Integration", "Error")]
        [Fact(DisplayName = "CardReplaceCommand_Error - Should throw Unprocessable Entity Exception for error with External Provider")]
        public async void ShouldNotReplaceCardUnprocessableEntityExternalProvider()
        {
            var adapter = GetCardReplace();
            var message = new List<Message>() { new Message { Text = "Error" } };

            var response = new BaseResult<CardReplaceResult>
            {
                IsSuccess = false,
                Result = new CardReplaceResult
                {
                    Metadata = new MetadataCardReplace
                    {
                        Messages = message
                    }
                }
            };

            CardReadRepository.Setup(x => x.Find(It.IsAny<int>())).Returns(adapter.SuccessRepositoryResponse);
            GetTokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(GetBaseTokenResult()));
            CardReplaceClient.Setup(x => x.CardReplaceAsync(It.IsAny<CardReplaceParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                                    .Returns(Task.FromResult(response));
            ReissueCardClient.Setup(x => x.ReissueCardAsync(It.IsAny<ReissueCardParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(ReissueCardResult("171")));
            GetCardClient.Setup(x => x.GetCardAsync(It.IsAny<GetCardParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(GetCardResult()));

            Services.AddSingleton(Logger.Object);
            Services.AddSingleton(CardReplaceClient.Object);
            Services.AddSingleton(CardReadRepository.Object);
            Services.AddSingleton(GetTokenParams.Object);
            Services.AddSingleton(GetTokenClient.Object);
            Services.AddSingleton(ReissueCardClient.Object);
            Services.AddSingleton(CardService.Object);
            Services.AddSingleton(ProcessConfig.Object);
            Services.AddSingleton(GetCardClient.Object);
            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<ProcessController>>();
            var mediator = GetInstance<IMediator>();
            var controller = new ProcessController(mediator, logger.Object);
            var request = adapter.SuccessRequest;

            await Assert.ThrowsAsync<UnprocessableEntityException>(() => controller.CardReplace(request, new CancellationToken()));
        }

        [Trait("Integration", "Error")]
        [Fact(DisplayName = "CardReplaceCommand_Error - Should throw Unprocessable Entity Exception for not match card information")]
        public async void ShouldNotReplaceCardUnprocessableEntityNoCardMatch()
        {
            var adapter = GetCardReplace();
            adapter.SuccessRepositoryResponse[0].LastFour = "";

            CardReadRepository.Setup(x => x.Find(It.IsAny<int>())).Returns(adapter.SuccessRepositoryResponse);

            Services.AddSingleton(Logger.Object);
            Services.AddSingleton(CardReplaceClient.Object);
            Services.AddSingleton(CardReadRepository.Object);
            Services.AddSingleton(GetTokenParams.Object);
            Services.AddSingleton(GetTokenClient.Object);
            Services.AddSingleton(ReissueCardClient.Object);
            Services.AddSingleton(CardService.Object);
            Services.AddSingleton(ProcessConfig.Object);
            Services.AddSingleton(GetCardClient.Object);
            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<ProcessController>>();
            var mediator = GetInstance<IMediator>();
            var controller = new ProcessController(mediator, logger.Object);
            var request = adapter.SuccessRequest;

            await Assert.ThrowsAsync<UnprocessableEntityException>(() => controller.CardReplace(request, new CancellationToken()));
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

        private CardReplaceJsonAdapter GetCardReplace()
        {
            return ConvertJson.ReadJson<CardReplaceJsonAdapter>("CardReplace.json");
        }

        private BaseResult<GetCardResult> GetCardResult()
        {
            return ConvertJson.ReadJson<BaseResult<GetCardResult>>("GetCardResult.json");
        }

        private BaseResult<ReissueCardResult> ReissueCardResult(string statusCode)
        {
            var listMessage = new List<Message>();
            listMessage.Add(new Message()
            {
                Code = statusCode,
                Text = string.Empty
            });

            return new BaseResult<ReissueCardResult>()
            {
                IsSuccess = true,
                Result = new ReissueCardResult()
                {
                    Metadata = new Proxy.Fis.ReissueCard.Messages.Result.MetadataReissueCard()
                    {
                        Messages = listMessage
                    }
                }
            };
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
