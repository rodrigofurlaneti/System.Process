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
using System.Process.Application.Commands.CreditCard;
using System.Process.Base.IntegrationTests;
using System.Process.CrossCutting.DependencyInjection;
using System.Process.Infrastructure.Configs;
using System.Process.IntegrationTests.Common;
using System.Proxy.Salesforce;
using System.Proxy.Salesforce.GetBusinessInformation;
using System.Proxy.Salesforce.GetBusinessInformation.Message;
using System.Proxy.Salesforce.GetCreditCards;
using System.Proxy.Salesforce.GetCreditCards.Message;
using System.Proxy.Salesforce.GetTerms;
using System.Proxy.Salesforce.GetTerms.Messages;
using System.Proxy.Salesforce.GetToken;
using System.Proxy.Salesforce.GetToken.Messages;
using System.Proxy.Salesforce.Messages;
using System.Proxy.Salesforce.RegisterAsset;
using System.Proxy.Salesforce.RegisterAsset.Messages;
using Xunit;

namespace System.Process.IntegrationTests.Application.Commands.CreditCard
{
    public class CreditCardCommandTests
    {
        #region Properties

        private static ServiceProvider Provider;
        private static Mock<IHttpContextAccessor> HttpContextAccessor;
        private static ServiceCollection Services;

        private Mock<ILogger<CreditCardCommand>> Logger { get; set; }
        private IOptions<RecordTypesConfig> RecordTypesConfig { get; set; }
        private IOptions<GetTokenParams> ConfigSalesforce { get; set; }
        private Mock<IGetTokenClient> TokenClient { get; set; }
        private Mock<IRegisterAssetClient> RegisterAssetClient { get; set; }
        private Mock<ICardService> CardService { get; set; }
        private Mock<IGetCreditCardsClient> GetCreditCardsClient { get; set; }
        private Mock<IGetTermsClient> GetTermsClient { get; set; }
        private Mock<IGetBusinessInformationClient> GetBusinessInformationClient { get; set; }

        #endregion

        #region Constructor

        static CreditCardCommandTests()
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
            Logger = new Mock<ILogger<CreditCardCommand>>();
            var config = new RecordTypesConfig
            {
                AssetCreditCard = "test"
            };
            CardService = new Mock<ICardService>();
            RecordTypesConfig = Options.Create(config);
            ConfigSalesforce = Options.Create(new GetTokenParams());
            TokenClient = new Mock<IGetTokenClient>();
            RegisterAssetClient = new Mock<IRegisterAssetClient>();
            GetCreditCardsClient = new Mock<IGetCreditCardsClient>();
            GetTermsClient = new Mock<IGetTermsClient>();
            GetBusinessInformationClient = new Mock<IGetBusinessInformationClient>();

            TokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(GetSalesforceToken());
            RegisterAssetClient.Setup(x => x.RegisterAsset(It.IsAny<RegisterAssetParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetSalesforceResult());
            GetCreditCardsClient.Setup(x => x.GetCreditCards(It.IsAny<GetCreditCardsParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetCreditCards());
            GetTermsClient.Setup(x => x.GetTerms(It.IsAny<GetTermsParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetTermsResult());
            GetBusinessInformationClient.Setup(x => x.GetBusinessInformation(It.IsAny<GetBusinessInformationParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetBusinessInformationResult());

            Services.AddSingleton(Logger.Object);
            Services.AddSingleton(RecordTypesConfig);
            Services.AddSingleton(ConfigSalesforce);
            Services.AddSingleton(TokenClient.Object);
            Services.AddSingleton(RegisterAssetClient.Object);
            Services.AddSingleton(GetCreditCardsClient.Object);
            Services.AddSingleton(GetTermsClient.Object);
            Services.AddSingleton(GetBusinessInformationClient.Object);

            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<CardsController>>();
            var mediator = GetInstance<IMediator>();
            var request = GetCreditCardRequest();
            var controller = new CardsController(mediator, logger.Object, CardService.Object);

            var result = await controller.CreditCardRequest(request, new CancellationToken());

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact(DisplayName = "Required information not provided")]
        public void ShoulRequestCreditCardError()
        {
            var validator = new CreditCardValidator();
            var request = new CreditCardRequest();

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
                Result = new GetTokenResult
                {
                    AccessToken = "test"
                }
            });
        }

        private Task<BaseResult<SalesforceResult>> GetSalesforceResult()
        {
            return Task.FromResult(new BaseResult<SalesforceResult>
            {
                IsSuccess = true,
                Result = ProcessIntegrationTestsConfiguration.ReadJson<SalesforceResult>("SalesforceResult.json")
            });
        }

        private CreditCardRequest GetCreditCardRequest()
        {
            return ProcessIntegrationTestsConfiguration.ReadJson<CreditCardRequest>("CreditCardRequest.json");
        }

        private Task<BaseResult<QueryResult<Proxy.Salesforce.GetCreditCards.Message.CreditCard>>> GetCreditCards()
        {
            return Task.FromResult(new BaseResult<QueryResult<Proxy.Salesforce.GetCreditCards.Message.CreditCard>>
            {
                IsSuccess = true,
                Result = new QueryResult<Proxy.Salesforce.GetCreditCards.Message.CreditCard>
                {
                    Records = new List<Proxy.Salesforce.GetCreditCards.Message.CreditCard>
                    {

                    }
                }
            });
        }
        private Task<BaseResult<QueryResult<Terms>>> GetTermsResult()
        {
            return Task.FromResult(new BaseResult<QueryResult<Terms>>
            {
                IsSuccess = true,
                Result = new QueryResult<Terms>
                {
                    Records = new List<Terms>
                    {
                         new Terms
                            {
                                Version = 0,
                                Type = "Prohibited_Activities",
                                Name = "Card Scheme prohibited"
                            }
                    }
                }
            });
        }

        private Task<BaseResult<QueryResult<GetBusinessInformationResponse>>> GetBusinessInformationResult()
        {
            return Task.FromResult(new BaseResult<QueryResult<GetBusinessInformationResponse>>
            {
                IsSuccess = true,
                Result = new QueryResult<GetBusinessInformationResponse>
                {
                    Records = new List<GetBusinessInformationResponse>
                    {
                        ConvertJson.ReadJson<GetBusinessInformationResponse>("GetBusinessInformationResponse.json")
                    }
                }
            });
        }
        #endregion
    }
}
