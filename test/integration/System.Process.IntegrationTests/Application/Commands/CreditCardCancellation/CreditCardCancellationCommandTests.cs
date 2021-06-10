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
using System.Process.Application.Commands.CreditCardCancellation;
using System.Process.Base.IntegrationTests;
using System.Process.CrossCutting.DependencyInjection;
using System.Process.Infrastructure.Configs;
using System.Proxy.Salesforce;
using System.Proxy.Salesforce.GetCreditCards;
using System.Proxy.Salesforce.GetCreditCards.Message;
using System.Proxy.Salesforce.GetToken;
using System.Proxy.Salesforce.GetToken.Messages;
using System.Proxy.Salesforce.Messages;
using System.Proxy.Salesforce.UpdateAsset;
using System.Proxy.Salesforce.UpdateAsset.Messages;
using Xunit;

namespace System.Process.IntegrationTests.Application.Commands.CreditCardCancellation
{
    public class CreditCardCancellationCommandTests
    {
        #region Properties

        private static ServiceProvider Provider;
        private static Mock<IHttpContextAccessor> HttpContextAccessor;
        private static ServiceCollection Services;


        private Mock<ICardService> CardService { get; set; }
        private Mock<ILogger<CreditCardCancellationCommand>> Logger { get; set; }
        private IOptions<RecordTypesConfig> RecordTypesConfig { get; set; }
        private IOptions<GetTokenParams> ConfigSalesforce { get; set; }
        private Mock<IGetTokenClient> TokenClient { get; set; }
        private Mock<IGetCreditCardsClient> GetCreditCardsClient { get; set; }
        private Mock<IUpdateAssetClient> UpdateAssetClient { get; set; }
        private CancellationToken CancellationToken { get; set; }

        #endregion

        #region Constructor

        static CreditCardCancellationCommandTests()
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
        public async void ShouldRequestCreditCardSuccessfully()
        {
            Logger = new Mock<ILogger<CreditCardCancellationCommand>>();
            var config = ProcessIntegrationTestsConfiguration.ReadJson<RecordTypesConfig>("RecordTypesConfig.json");
            RecordTypesConfig = Options.Create(config);
            var configSalesforce = new GetTokenParams();
            ConfigSalesforce = Options.Create(configSalesforce);
            TokenClient = new Mock<IGetTokenClient>();
            GetCreditCardsClient = new Mock<IGetCreditCardsClient>();
            UpdateAssetClient = new Mock<IUpdateAssetClient>();
            TokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(GetSalesforceToken());
            GetCreditCardsClient.Setup(x => x.GetCreditCards(It.IsAny<GetCreditCardsParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetCreditCard());
            UpdateAssetClient.Setup(x => x.UpdateAsset(It.IsAny<UpdateAssetParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetSalesforceResult());

            Services.AddSingleton(Logger.Object);
            Services.AddSingleton(RecordTypesConfig);
            Services.AddSingleton(ConfigSalesforce);
            Services.AddSingleton(TokenClient.Object);
            Services.AddSingleton(GetCreditCardsClient.Object);
            Services.AddSingleton(UpdateAssetClient.Object);

            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<CardsController>>();
            var mediator = GetInstance<IMediator>();
            var request = new CreditCardCancellationRequest
            {
                AssetId = "string",
                SystemId = "string"
            };
            CardService = new Mock<ICardService>();
            var controller = new CardsController(mediator, logger.Object, CardService.Object);

            var result = await controller.CancelCreditCardRequest(request, new CancellationToken());

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact(DisplayName = "Required information not provided")]
        public void ShoulRequestCreditCardError()
        {
            var validator = new CreditCardCancellationValidation();
            var request = new CreditCardCancellationRequest();

            var error = validator.Validate(request);

            Assert.False(error.IsValid);
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
                Result = ProcessIntegrationTestsConfiguration.ReadJson<GetTokenResult>("GetTokenResult.json")
            });
        }

        private Task<BaseResult<QueryResult<Proxy.Salesforce.GetCreditCards.Message.CreditCard>>> GetCreditCard()
        {
            return Task.FromResult(new BaseResult<QueryResult<Proxy.Salesforce.GetCreditCards.Message.CreditCard>>
            {
                IsSuccess = true,
                Result = new QueryResult<Proxy.Salesforce.GetCreditCards.Message.CreditCard>
                {
                    Records = new List<Proxy.Salesforce.GetCreditCards.Message.CreditCard>
                    {
                        new Proxy.Salesforce.GetCreditCards.Message.CreditCard
                        {
                              AssetId = "string",
                              CreditLimit =  1,
                              Status = "Approved Pending Acceptance",
                              CreditCardType = "string"
                        }
                    }
                }
            });
        }

        private Task<BaseResult<SalesforceResult>> GetSalesforceResult()
        {
            return Task.FromResult(new BaseResult<SalesforceResult>
            {
                IsSuccess = true,
                Result = new SalesforceResult
                {
                    Success = true
                }
            });
        }

        #endregion
    }
}
