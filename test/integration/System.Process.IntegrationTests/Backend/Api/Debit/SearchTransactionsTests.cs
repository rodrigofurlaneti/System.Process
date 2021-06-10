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

namespace System.Process.IntegrationTests.Backend.Api.Debit
{
    public class SearchTransactionsTests
    {
        #region Properties

        private static ServiceProvider Provider;
        private static Mock<IHttpContextAccessor> HttpContextAccessor;
        private static ServiceCollection Services;
        private Mock<IGetTokenClient> GetTokenClient;
        private Mock<IOptions<GetTokenParams>> GetTokenParams;
        private Mock<ISearchTransactionsClient> SearchTransactionsClient;
        private Mock<ICardService> CardService;
        private Mock<ICardReadRepository> CardReadRepository;

        #endregion

        #region Constructor

        public SearchTransactionsTests()
        {
            Services = new ServiceCollection();
            Services.AddMediator();
            Services.AddScoped(typeof(ILoggerFactory), typeof(LoggerFactory));
            Services.AddScoped(typeof(ILogger<>), typeof(Logger<>));

            Provider = Services.BuildServiceProvider();
        }

        #endregion

        #region Main Flow

        [Fact(DisplayName = "Main Flow - Success")]
        public async Task ShouldSearchTransactionsSuccessfully()
        {
            var adapter = GetSearchTransactions();

            GetTokenClient = new Mock<IGetTokenClient>();
            GetTokenParams = new Mock<IOptions<GetTokenParams>>();
            SearchTransactionsClient = new Mock<ISearchTransactionsClient>();
            CardService = new Mock<ICardService>();
            CardReadRepository = new Mock<ICardReadRepository>();

            var response = new BaseResult<SearchTransactionsResult>
            {
                IsSuccess = true,
                Result = adapter.SuccessResult
            };

            CardReadRepository.Setup(x => x.Find(It.IsAny<int>())).Returns(adapter.SuccessRepositoryResponse);
            GetTokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(GetBaseTokenResult()));
            SearchTransactionsClient.Setup(x => x.SearchTransactionsAsync(It.IsAny<SearchTransactionsParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                                    .Returns(Task.FromResult(response));

            Services.AddSingleton(SearchTransactionsClient.Object);
            Services.AddSingleton(GetTokenClient.Object);
            Services.AddSingleton(GetTokenParams.Object);
            Services.AddSingleton(CardReadRepository.Object);
            Services.AddSingleton(CardService.Object);

            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<CardsController>>();
            var mediator = GetInstance<IMediator>();
            var cardService = GetInstance<ICardService>();
            var controller = new CardsController(mediator, logger.Object, cardService);

            var result = await controller.SearchTransactions(adapter.SuccessRequest, new CancellationToken());

            Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(((SearchTransactionsResponse)((OkObjectResult)result).Value).Transactions);
        }

        [Fact(DisplayName = "Should throw Unprocessable Exception with FIS")]
        public async Task ShouldThrowUnprocessableException()
        {
            var adapter = GetSearchTransactions();

            GetTokenClient = new Mock<IGetTokenClient>();
            GetTokenParams = new Mock<IOptions<GetTokenParams>>();
            SearchTransactionsClient = new Mock<ISearchTransactionsClient>();
            CardService = new Mock<ICardService>();
            CardReadRepository = new Mock<ICardReadRepository>();

            var response = new BaseResult<SearchTransactionsResult>
            {
                IsSuccess = false,
                Message = "{\"metadata\":{\"messages\":[{\"code\":\"10\",\"text\":\"Invalid card number\"}]}}"
            };

            CardReadRepository.Setup(x => x.Find(It.IsAny<int>())).Returns(adapter.SuccessRepositoryResponse);
            GetTokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(GetBaseTokenResult()));
            SearchTransactionsClient.Setup(x => x.SearchTransactionsAsync(It.IsAny<SearchTransactionsParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                                    .Returns(Task.FromResult(response));

            Services.AddSingleton(SearchTransactionsClient.Object);
            Services.AddSingleton(GetTokenClient.Object);
            Services.AddSingleton(GetTokenParams.Object);
            Services.AddSingleton(CardReadRepository.Object);
            Services.AddSingleton(CardService.Object);

            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<CardsController>>();
            var mediator = GetInstance<IMediator>();
            var cardService = GetInstance<ICardService>();
            var controller = new CardsController(mediator, logger.Object, cardService);

            //var validate = new SearchTransactionsValidator();
            //validate.Validate(adapter.SuccessRequest);

            await Assert.ThrowsAsync<UnprocessableEntityException>(() => controller.SearchTransactions(adapter.SuccessRequest, new CancellationToken()));

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
