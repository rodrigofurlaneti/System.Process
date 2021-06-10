using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Process.Api.Controllers.V1;
using System.Process.Application.Clients.Cards;
using System.Process.Application.Commands.SearchTransactions;
using System.Process.Base.IntegrationTests;
using System.Process.CrossCutting.DependencyInjection;
using System.Process.Domain.Repositories;
using System.Process.IntegrationTests.Adapters;
using System.Process.IntegrationTests.Common;
using System.Phoenix.Common.Exceptions;
using System.Proxy.Fis.GetToken;
using System.Proxy.Fis.GetToken.Messages;
using System.Proxy.Fis.Messages;
using System.Proxy.Fis.SearchTransactions;
using System.Proxy.Fis.SearchTransactions.Messages;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace System.Process.IntegrationTests.Application.Commands.SearchTransactions
{
    public class SearchTransactionsCommandTests
    {
        #region Properties

        private static ServiceCollection Services;
        private static ServiceProvider Provider;
        private static Mock<IHttpContextAccessor> HttpContextAccessor;

        private Mock<ILogger<SearchTransactionsCommand>> Logger { get; }
        private Mock<IGetTokenClient> GetTokenClient { get; }
        private Mock<IOptions<GetTokenParams>> GetTokenParams { get; }
        private Mock<ISearchTransactionsClient> SearchTransactionsClient { get; }
        private Mock<ICardReadRepository> CardReadRepository { get; }
        private Mock<ICardService> CardService { get; }

        #endregion

        #region Constructor

        public SearchTransactionsCommandTests()
        {
            Logger = new Mock<ILogger<SearchTransactionsCommand>>();
            GetTokenClient = new Mock<IGetTokenClient>();
            GetTokenParams = new Mock<IOptions<GetTokenParams>>();
            SearchTransactionsClient = new Mock<ISearchTransactionsClient>();
            CardReadRepository = new Mock<ICardReadRepository>();
            CardService = new Mock<ICardService>();

            Services = new ServiceCollection();
            Services.AddMediator();
            Services.AddScoped(typeof(ILoggerFactory), typeof(LoggerFactory));
            Services.AddScoped(typeof(ILogger<>), typeof(Logger<>));

            Provider = Services.BuildServiceProvider();
        }

        #endregion

        #region Tests

        [Trait("Integration", "Success")]
        [Fact(DisplayName = "SearchTransactionsCommand_Success")]
        public async Task ShouldSearchTransactionsSuccessfully()
        {
            var adapter = GetSearchTransactions();
            var response = new BaseResult<SearchTransactionsResult>
            {
                IsSuccess = true,
                Result = ProcessIntegrationTestsConfiguration.ReadJson<SearchTransactionsResult>("FisSuccess.json")
            };

            CardReadRepository.Setup(x => x.Find(It.IsAny<int>())).Returns(adapter.SuccessRepositoryResponse);
            GetTokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(GetBaseTokenResult()));
            SearchTransactionsClient.Setup(x => x.SearchTransactionsAsync(It.IsAny<SearchTransactionsParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                                    .Returns(Task.FromResult(response));

            Services.AddSingleton(Logger.Object);
            Services.AddSingleton(SearchTransactionsClient.Object);
            Services.AddSingleton(CardReadRepository.Object);
            Services.AddSingleton(GetTokenParams.Object);
            Services.AddSingleton(GetTokenClient.Object);
            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<CardsController>>();
            var mediator = GetInstance<IMediator>();
            var controller = new CardsController(mediator, logger.Object, CardService.Object);
            var request = adapter.SuccessRequest;

            var result = await controller.SearchTransactions(request, new CancellationToken());

            Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(((SearchTransactionsResponse)((OkObjectResult)result).Value).Transactions);
        }

        [Trait("Integration", "Error")]
        [Fact(DisplayName = "SearchTransactionsCommand_Error")]
        public async Task ShouldNotSearchTransactions()
        {
            var adapter = GetSearchTransactions();
            var response = new BaseResult<SearchTransactionsResult>
            {
                IsSuccess = false,
                Message = "{\"metadata\":{\"messages\":[{\"code\":\"10\",\"text\":\"Invalid card number\"}]}}"
            };

            CardReadRepository.Setup(x => x.Find(It.IsAny<int>())).Returns(adapter.SuccessRepositoryResponse);
            GetTokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(GetBaseTokenResult()));
            SearchTransactionsClient.Setup(x => x.SearchTransactionsAsync(It.IsAny<SearchTransactionsParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                                   .Returns(Task.FromResult(response));

            Services.AddSingleton(Logger.Object);
            Services.AddSingleton(SearchTransactionsClient.Object);
            Services.AddSingleton(CardReadRepository.Object);
            Services.AddSingleton(GetTokenParams.Object);
            Services.AddSingleton(GetTokenClient.Object);
            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<CardsController>>();
            var mediator = GetInstance<IMediator>();
            var controller = new CardsController(mediator, logger.Object, CardService.Object);
            var request = new ConsultCardsByidCardRequest();

            await Assert.ThrowsAsync<UnprocessableEntityException>(() => controller.SearchTransactions(request, new CancellationToken()));
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

        private SearchTransactionsJsonAdapter GetSearchTransactions()
        {
            return ConvertJson.ReadJson<SearchTransactionsJsonAdapter>("SearchTransactions.json");
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
