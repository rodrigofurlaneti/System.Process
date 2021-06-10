using System.Collections.Generic;
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
using System.Process.Application.Queries.SearchCreditCard;
using System.Process.CrossCutting.DependencyInjection;
using System.Process.Infrastructure.Configs;
using System.Proxy.Salesforce.GetCreditCards;
using System.Proxy.Salesforce.GetCreditCards.Message;
using System.Proxy.Salesforce.GetToken;
using System.Proxy.Salesforce.GetToken.Messages;
using System.Proxy.Salesforce.Messages;
using Xunit;

namespace System.Process.IntegrationTests.Application.Queries.SearchCreditCard
{
    public class SearchCreditCardQueryTests
    {
        #region Properties

        private static ServiceProvider Provider;
        private static Mock<IHttpContextAccessor> HttpContextAccessor;
        private static ServiceCollection Services;

        private Mock<ILogger<SearchCreditCardQuery>> Logger { get; set; }
        private IOptions<RecordTypesConfig> RecordTypesConfig { get; set; }
        private IOptions<GetTokenParams> ConfigSalesforce { get; set; }
        private Mock<IGetTokenClient> TokenClient { get; set; }
        private Mock<IGetCreditCardsClient> GetCreditCardsClient { get; set; }
        private Mock<ICardService> CardService { get; set; }

        #endregion

        #region Constructor

        static SearchCreditCardQueryTests()
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
        public async void ShouldSearchCreditCardsSuccessfully()
        {
            Logger = new Mock<ILogger<SearchCreditCardQuery>>();
            var config = new RecordTypesConfig
            {
                AssetCreditCard = "test"
            };
            CardService = new Mock<ICardService>();
            RecordTypesConfig = Options.Create(config);
            ConfigSalesforce = Options.Create(new GetTokenParams());
            TokenClient = new Mock<IGetTokenClient>();
            GetCreditCardsClient = new Mock<IGetCreditCardsClient>();
            TokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(GetSalesforceToken());
            GetCreditCardsClient.Setup(x => x.GetCreditCards(It.IsAny<GetCreditCardsParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetCreditCards());

            Services.AddSingleton(Logger.Object);
            Services.AddSingleton(RecordTypesConfig);
            Services.AddSingleton(ConfigSalesforce);
            Services.AddSingleton(TokenClient.Object);
            Services.AddSingleton(GetCreditCardsClient.Object);

            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<CardsController>>();
            var mediator = GetInstance<IMediator>();
            var controller = new CardsController(mediator, logger.Object, CardService.Object);

            var result = await controller.SearchCreditCards("test", new CancellationToken());

            Assert.IsType<OkObjectResult>(result);
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

        private Task<BaseResult<GetTokenResult>> GetSalesforceToken()
        {
            return Task.FromResult(new BaseResult<GetTokenResult>
            {
                IsSuccess = true,
                Result = new GetTokenResult
                {
                    AccessToken = "test"
                }
            });
        }

        private Task<BaseResult<QueryResult<CreditCard>>> GetCreditCards()
        {
            return Task.FromResult(new BaseResult<QueryResult<CreditCard>>
            {
                IsSuccess = true,
                Result = new QueryResult<CreditCard>
                {
                    Records = new List<CreditCard> {
                        new CreditCard
                        {
                            AssetId = "tests",
                            CreditCardType = "tests",
                            CreditLimit = 210,
                            Status = "tests",
                            Product = "MCB",
                            Subproduct = "001"
                        }
                    }
                }
            });
        }

        #endregion
    }
}
