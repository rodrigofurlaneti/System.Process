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
using System.Process.Application.Commands.CreditCardMakePayment;
using System.Process.Base.IntegrationTests;
using System.Process.CrossCutting.DependencyInjection;
using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Process.Domain.ValueObjects;
using System.Process.Infrastructure.Repositories.EntityFramework;
using System.Proxy.Rtdx.GetToken;
using System.Proxy.Rtdx.GetToken.Messages;
using System.Proxy.Rtdx.Messages;
using System.Proxy.Rtdx.PaymentInquiry;
using System.Proxy.Rtdx.PaymentInquiry.Messages;
using Xunit;

namespace System.Process.IntegrationTests.Application.Commands.CreditCardMakePayment
{
    public class CreditCardMakePaymentCommandTests
    {
        #region Properties

        private static ServiceProvider Provider;
        private static Mock<IHttpContextAccessor> HttpContextAccessor;
        private static ServiceCollection Services;

        private Mock<ILogger<CreditCardMakePaymentCommand>> Logger { get; set; }
        private Mock<ICardReadRepository> CardReadRepository { get; set; }
        private Mock<IGetTokenOperation> TokenOperation { get; set; }
        private IOptions<GetTokenParams> GetTokenParams { get; set; }
        private Mock<IPaymentInquiryOperation> PaymentInquiryOperation { get; set; }
        private IOptions<ProcessConfig> ProcessConfig { get; set; }
        private Mock<ICardService> CardService { get; set; }

        #endregion

        #region Constructor

        static CreditCardMakePaymentCommandTests()
        {
            Services = new ServiceCollection();
            Services.AddMediator();
            Services.AddScoped(typeof(ILoggerFactory), typeof(LoggerFactory));
            Services.AddScoped(typeof(ILogger<>), typeof(Logger<>));
            Services.AddScoped<ICardReadRepository, CardReadRepository>();


            Provider = Services.BuildServiceProvider();
        }

        #endregion

        #region Main Flow

        [Fact(DisplayName = "Main Flow - Success")]
        public async void ShouldCreditCardMakePaymentSuccessfully()
        {
            CardService = new Mock<ICardService>();
            Logger = new Mock<ILogger<CreditCardMakePaymentCommand>>();
            CardReadRepository = new Mock<ICardReadRepository>();
            PaymentInquiryOperation = new Mock<IPaymentInquiryOperation>();
            TokenOperation = new Mock<IGetTokenOperation>();
            var param = new GetTokenParams();
            GetTokenParams = Options.Create(param);
            var ProcessConfig = ProcessIntegrationTestsConfiguration.ReadJson<ProcessConfig>("ProcessConfig.json");
            ProcessConfig = Options.Create(ProcessConfig);

            var creditCard = GetCard();
            creditCard.CardStatus = "Active";
            CardReadRepository.Setup(x => x.FindByCardId(It.IsAny<int>())).Returns(creditCard);
            PaymentInquiryOperation.Setup(x => x.PaymentInquiryAsync(It.IsAny<PaymentInquiryParams>(), It.IsAny<CancellationToken>())).Returns(GetPaymentInquiryResult());
            TokenOperation.Setup(x => x.GetTokenAsync(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(GetTokenResult());

            Services.AddSingleton(Logger.Object);
            Services.AddSingleton(CardReadRepository.Object);
            Services.AddSingleton(PaymentInquiryOperation.Object);
            Services.AddSingleton(TokenOperation.Object);
            Services.AddSingleton(GetTokenParams);
            Services.AddSingleton(ProcessConfig);

            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<CardsController>>();
            var mediator = GetInstance<IMediator>();
            var controller = new CardsController(mediator, logger.Object, CardService.Object);
            var request = new CreditCardMakePaymentRequest
            {
                CardId = 10
            };
            var result = await controller.CreditCardMakePayment(request, new CancellationToken());

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

        private Task<BaseResult<PaymentInquiryResult>> GetPaymentInquiryResult()
        {
            return Task.FromResult(new BaseResult<PaymentInquiryResult>
            {
                IsSuccess = true,
                Result = ProcessIntegrationTestsConfiguration.ReadJson<PaymentInquiryResult>("PaymentInquiryResult.json")
            });
        }

        private Task<BaseResult<GetTokenResult>> GetTokenResult()
        {
            var status = new Proxy.Rtdx.GetToken.Messages.Results.Status() { System = new Proxy.Rtdx.GetToken.Messages.Results.System() { Code = "00", DaysLeft = "214", Message = "", Name = "B" } };
            return Task.FromResult(new BaseResult<GetTokenResult>
            {
                IsSuccess = true,
                Result = new GetTokenResult { ExternalTraceId = "335", Response = "Connected", SecurityToken = "[B:Ivj8M0//Vo8FzGSRopJAYA==]", TraceId = "U01K3210925450661QX", Status = status }
            });
        }

        public Card GetCard()
        {
            return ProcessIntegrationTestsConfiguration.ReadJson<Card>("Cards.json");
        }

        #endregion
    }
}
