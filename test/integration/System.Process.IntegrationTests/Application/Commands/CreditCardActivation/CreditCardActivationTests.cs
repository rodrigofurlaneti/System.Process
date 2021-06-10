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
using System.Process.Application.Commands.CreditCardActivation;
using System.Process.Application.Commands.CreditCardActivation.Validators;
using System.Process.Base.IntegrationTests;
using System.Process.CrossCutting.DependencyInjection;
using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Process.Domain.ValueObjects;
using System.Process.Infrastructure.Repositories.EntityFramework;
using System.Process.IntegrationTests.Common;
using System.Proxy.Rtdx.Messages;
using System.Proxy.Rtdx.CardActivation;
using System.Proxy.Rtdx.CardActivation.Messages;
using System.Proxy.Rtdx.GetToken;
using System.Proxy.Salesforce;
using System.Proxy.Salesforce.GetToken;
using System.Proxy.Salesforce.GetToken.Messages;
using System.Proxy.Salesforce.UpdateAsset;
using System.Proxy.Salesforce.UpdateAsset.Messages;
using Xunit;
using RtdxGetToken = System.Proxy.Rtdx.GetToken.Messages;

namespace System.Process.IntegrationTests.Application.Commands.CreditCardActivation
{
    public class CreditCardActivationTests
    {
        #region Properties

        private static ServiceProvider Provider;
        private static Mock<IHttpContextAccessor> HttpContextAccessor;
        private static ServiceCollection Services;

        private Mock<ILogger<CreditCardActivationCommand>> Logger { get; set; }
        private Mock<ICardReadRepository> CardReadRepository { get; set; }
        private Mock<IUpdateAssetClient> UpdateAssetClient { get; set; }
        private Mock<IGetTokenClient> TokenClient { get; set; }
        private IOptions<GetTokenParams> ConfigSalesforce { get; set; }
        private Mock<IGetTokenOperation> GetTokenOperation { get; set; }
        private IOptions<RtdxGetToken.GetTokenParams> RtdxTokenParams { get; set; }
        private Mock<ICardActivationOperation> CardActivationOperation { get; set; }
        private Mock<ICardWriteRepository> CardWriteRepository { get; set; }
        private IOptions<ProcessConfig> ProcessConfig { get; set; }

        #endregion

        #region Constructor

        static CreditCardActivationTests()
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
        public async void ShouldRequestCreditCardActivationSuccessfully()
        {
            var CardService = new Mock<ICardService>();
            Logger = new Mock<ILogger<CreditCardActivationCommand>>();
            CardReadRepository = new Mock<ICardReadRepository>();
            UpdateAssetClient = new Mock<IUpdateAssetClient>();
            TokenClient = new Mock<IGetTokenClient>();
            var param = new GetTokenParams();
            ConfigSalesforce = Options.Create(param);
            GetTokenOperation = new Mock<IGetTokenOperation>();
            CardWriteRepository = new Mock<ICardWriteRepository>();
            var configRtdx = new RtdxGetToken.GetTokenParams
            {
                CorpId = "string",
                ExternalTraceId = "string",
                Application = "",
                NewPassword = "",
                System = new List<RtdxGetToken.Params.SystemParams>
                {
                    new RtdxGetToken.Params.SystemParams
                    {
                        Password = "string",
                        SysFlag = "B",
                        UserId = "string"
                    }
                }
            };
            RtdxTokenParams = Options.Create(configRtdx);
            var ProcessConfig = ProcessIntegrationTestsConfiguration.ReadJson<ProcessConfig>("ProcessConfig.json");
            ProcessConfig = Options.Create(ProcessConfig);
            CardActivationOperation = new Mock<ICardActivationOperation>();

            var creditCard = GetCard();
            creditCard.CardStatus = "Pending Activation";
            CardReadRepository.Setup(x => x.FindByCardId(It.IsAny<int>())).Returns(creditCard);
            TokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(GetSalesforceToken());
            UpdateAssetClient.Setup(x => x.UpdateAsset(It.IsAny<UpdateAssetParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetSalesforceResult());
            GetTokenOperation.Setup(x => x.GetTokenAsync(It.IsAny<RtdxGetToken.GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(GetTokenResult());
            CardActivationOperation.Setup(x => x.CardActivationAsync(It.IsAny<CardActivationParams>(), It.IsAny<CancellationToken>())).Returns(GetCardActivationResult());
            CardWriteRepository.Setup(x => x.Add(It.IsAny<Card>(), It.IsAny<CancellationToken>()));

            Services.AddSingleton(Logger.Object);
            Services.AddSingleton(CardReadRepository.Object);
            Services.AddSingleton(UpdateAssetClient.Object);
            Services.AddSingleton(TokenClient.Object);
            Services.AddSingleton(GetTokenOperation.Object);
            Services.AddSingleton(ConfigSalesforce);
            Services.AddSingleton(RtdxTokenParams);
            Services.AddSingleton(CardActivationOperation.Object);
            Services.AddSingleton(CardWriteRepository.Object);
            Services.AddSingleton(ProcessConfig);

            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<CardsController>>();
            var mediator = GetInstance<IMediator>();
            var request = GetCreditCardActivationRequest();
            var controller = new CardsController(mediator, logger.Object, CardService.Object);

            var result = await controller.CreditCardActivation(request, new CancellationToken());

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact(DisplayName = "Required information not provided")]
        public void ShoulRequestCreditCardActivationError()
        {
            var validator = new CreditCardActivationValidator();
            var request = new CreditCardActivationRequest();

            var error = validator.Validate(request);

            Assert.False(error.IsValid);
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

        public Card GetCard()
        {
            return ConvertJson.ReadJson<Card>("Cards.json");
        }

        private Task<Proxy.Salesforce.Messages.BaseResult<GetTokenResult>> GetSalesforceToken()
        {
            return Task.FromResult(new Proxy.Salesforce.Messages.BaseResult<GetTokenResult>
            {
                IsSuccess = true,
                Result = ConvertJson.ReadJson<GetTokenResult>("GetTokenResult.json")
            });
        }

        private Task<Proxy.Salesforce.Messages.BaseResult<SalesforceResult>> GetSalesforceResult()
        {
            return Task.FromResult(new Proxy.Salesforce.Messages.BaseResult<SalesforceResult>
            {
                IsSuccess = true,
                Result = new SalesforceResult
                {
                    Success = true
                }
            });
        }

        private CreditCardActivationRequest GetCreditCardActivationRequest()
        {
            return ConvertJson.ReadJson<CreditCardActivationRequest>("CreditCardActivationRequest.json");
        }

        private Task<Proxy.Rtdx.Messages.BaseResult<RtdxGetToken.GetTokenResult>> GetTokenResult()
        {
            return Task.FromResult(new Proxy.Rtdx.Messages.BaseResult<RtdxGetToken.GetTokenResult>
            {
                IsSuccess = true,
                Result = ProcessIntegrationTestsConfiguration.ReadJson<RtdxGetToken.GetTokenResult>("RtdxGetTokenResult.json")
            });
        }
        private Task<BaseResult<CardActivationResult>> GetCardActivationResult()
        {
            return Task.FromResult(new BaseResult<CardActivationResult>
            {
                IsSuccess = true,
                Result = new CardActivationResult
                {
                    AccountNumber = "",
                    Message = "",
                    ResponseCode = "00"
                }
            });
        }
        #endregion
    }
}
