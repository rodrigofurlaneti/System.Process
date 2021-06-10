using FluentValidation.AspNetCore;
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
using System.Phoenix.Web.Filters;
using System.Proxy.Fis.CardReplace;
using System.Proxy.Fis.CardReplace.Messages;
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

namespace System.Process.IntegrationTests.Backend.Api.Process
{
    public class CardReplaceTests
    {
        #region Properties

        private static ServiceProvider Provider;
        private static Mock<IHttpContextAccessor> HttpContextAccessor;
        private static ServiceCollection Services;
        private Mock<IGetTokenClient> GetTokenClient;
        private Mock<IOptions<GetTokenParams>> GetTokenParams;
        private Mock<ICardReplaceClient> CardReplaceClient;
        private Mock<IReissueCardClient> ReissueCardClient;
        private Mock<ILogger<ProcessController>> Logger { get; }
        private Mock<ICardReadRepository> CardReadRepository { get; }
        private Mock<ICardService> CardService { get; }
        private Mock<IOptions<ProcessConfig>> ProcessConfig { get; }
        private Mock<IGetCardClient> GetCardClient { get; }

        #endregion

        #region Constructor

        public CardReplaceTests()
        {
            Services = new ServiceCollection();
            Services.AddMvc(options => options.Filters.Add(new ValidationFilterAttribute()))
               .AddFluentValidation(options =>
               {
                   options.RegisterValidatorsFromAssemblyContaining<CardReplaceValidator>();
               });
            Services.AddMediator();
            Services.AddScoped(typeof(ILoggerFactory), typeof(LoggerFactory));
            Services.AddScoped(typeof(ILogger<>), typeof(Logger<>));

            Provider = Services.BuildServiceProvider();

            Logger = new Mock<ILogger<ProcessController>>();
            GetTokenClient = new Mock<IGetTokenClient>();
            GetTokenParams = new Mock<IOptions<GetTokenParams>>();
            CardReplaceClient = new Mock<ICardReplaceClient>();
            ReissueCardClient = new Mock<IReissueCardClient>();
            CardReadRepository = new Mock<ICardReadRepository>();
            CardService = new Mock<ICardService>();
            GetCardClient = new Mock<IGetCardClient>();
            ProcessConfig = new Mock<IOptions<ProcessConfig>>();
            ProcessConfig.Setup(p => p.Value).Returns(ProcessIntegrationTestsConfiguration.ReadJson<ProcessConfig>("ProcessConfig.json"));
        }

        #endregion

        #region Tests

        [Fact(DisplayName = "Main Flow - Success")]
        public async void ShouldSendCardReplaceSuccessfully()
        {
            var adapter = GetCardReplace();
            var response = new BaseResult<CardReplaceResult>
            {
                IsSuccess = true,
                Result = adapter.SuccessResult
            };

            CardReadRepository.Setup(x => x.Find(It.IsAny<int>())).Returns(adapter.SuccessRepositoryResponse);
            GetTokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(GetBaseTokenResult()));
            CardReplaceClient.Setup(x => x.CardReplaceAsync(It.IsAny<CardReplaceParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                                    .Returns(Task.FromResult(response));
            GetCardClient.Setup(x => x.GetCardAsync(It.IsAny<GetCardParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(GetCardResult()));
            ReissueCardClient.Setup(x => x.ReissueCardAsync(It.IsAny<ReissueCardParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(ReissueCardResult("00")));

            Services.AddSingleton(CardReplaceClient.Object);
            Services.AddSingleton(ReissueCardClient.Object);
            Services.AddSingleton(CardReadRepository.Object);
            Services.AddSingleton(GetTokenClient.Object);
            Services.AddSingleton(GetTokenParams.Object);
            Services.AddSingleton(CardService.Object);
            Services.AddSingleton(GetCardClient.Object);
            Services.AddSingleton(ProcessConfig.Object);

            Provider = Services.BuildServiceProvider();

            var mediator = GetInstance<IMediator>();
            var controller = new ProcessController(mediator, Logger.Object);

            var request = adapter.SuccessRequest;

            var validate = new CardReplaceValidator();
            validate.Validate(request);

            var result = await controller.CardReplace(request, new CancellationToken());

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact(DisplayName = "AF1 - Required information not provided")]
        public void ShouldThrowRequestNotCompliantError()
        {
            var requestError = new CardReplaceRequest { CardId = 0, Pan = null, Address = null, ReplaceReason = null };
            var addressError = new Address { City = null, Country = null, Line1 = null, State = null, Type = null, ZipCode = null };
            var request = new CardReplaceRequest { CardId = 1, Pan = "1234", Address = addressError, ReplaceReason = "damaged" };

            var validate = new CardReplaceValidator();

            var error = validate.Validate(requestError);
            var errorAddress = validate.Validate(request);

            Assert.False(error.IsValid);
            Assert.False(errorAddress.IsValid);
        }

        [Fact(DisplayName = "AF2 - Should throw Not Found Exception")]
        public async Task ShouldThrowNotFoundException()
        {
            var adapter = GetCardReplace();

            CardReadRepository.Setup(x => x.Find(It.IsAny<int>())).Returns(new List<Card>());
            GetCardClient.Setup(x => x.GetCardAsync(It.IsAny<GetCardParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(GetCardResult()));

            Services.AddSingleton(CardReplaceClient.Object);
            Services.AddSingleton(ReissueCardClient.Object);
            Services.AddSingleton(CardReadRepository.Object);
            Services.AddSingleton(GetTokenClient.Object);
            Services.AddSingleton(GetTokenParams.Object);
            Services.AddSingleton(CardService.Object);
            Services.AddSingleton(GetCardClient.Object);
            Services.AddSingleton(ProcessConfig.Object);

            Provider = Services.BuildServiceProvider();

            var mediator = GetInstance<IMediator>();
            var controller = new ProcessController(mediator, Logger.Object);

            var request = adapter.SuccessRequest;

            var validate = new CardReplaceValidator();
            validate.Validate(request);

            await Assert.ThrowsAsync<NotFoundException>(() => controller.CardReplace(request, new CancellationToken()));
        }

        [Fact(DisplayName = "EF1 - Should throw Unprocessable Exception with FIS")]
        public async Task ShouldThrowUnprocessableException()
        {
            var adapter = GetCardReplace();
            var message = new List<Message>() { new Message { Text = "Error" } };

            var response = new BaseResult<CardReplaceResult>
            {
                IsSuccess = false,
                Result = new CardReplaceResult
                {
                    Metadata = new Proxy.Fis.CardReplace.Messages.Result.MetadataCardReplace
                    {
                        Messages = message
                    }
                }
            };

            CardReadRepository.Setup(x => x.Find(It.IsAny<int>())).Returns(adapter.SuccessRepositoryResponse);
            GetTokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(GetBaseTokenResult()));
            CardReplaceClient.Setup(x => x.CardReplaceAsync(It.IsAny<CardReplaceParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                                    .Returns(Task.FromResult(response));
            GetCardClient.Setup(x => x.GetCardAsync(It.IsAny<GetCardParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(GetCardResult()));
            ReissueCardClient.Setup(x => x.ReissueCardAsync(It.IsAny<ReissueCardParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(ReissueCardResult("171")));

            Services.AddSingleton(CardReplaceClient.Object);
            Services.AddSingleton(ReissueCardClient.Object);
            Services.AddSingleton(CardReadRepository.Object);
            Services.AddSingleton(GetTokenClient.Object);
            Services.AddSingleton(GetTokenParams.Object);
            Services.AddSingleton(CardService.Object);
            Services.AddSingleton(GetCardClient.Object);
            Services.AddSingleton(ProcessConfig.Object);

            Provider = Services.BuildServiceProvider();

            var mediator = GetInstance<IMediator>();
            var controller = new ProcessController(mediator, Logger.Object);

            var request = adapter.SuccessRequest;

            var validate = new CardReplaceValidator();
            validate.Validate(request);

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

        #endregion
    }
}
