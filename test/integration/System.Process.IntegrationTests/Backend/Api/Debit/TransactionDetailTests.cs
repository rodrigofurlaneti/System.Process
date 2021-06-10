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
using System.Process.Application.Commands.TransactionDetail;
using System.Process.CrossCutting.DependencyInjection;
using System.Process.IntegrationTests.Adapters;
using System.Process.IntegrationTests.Common;
using System.Phoenix.Web.Filters;
using System.Proxy.Fis.GetToken;
using System.Proxy.Fis.GetToken.Messages;
using System.Proxy.Fis.Messages;
using System.Proxy.Fis.TransactionDetail;
using System.Proxy.Fis.TransactionDetail.Messages;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace System.Process.IntegrationTests.Backend.Api.Debit
{
    public class TransactionDetailTests
    {
        #region Properties

        private static ServiceProvider Provider;
        private static Mock<IHttpContextAccessor> HttpContextAccessor;
        private static ServiceCollection Services;
        private Mock<IGetTokenClient> GetTokenClient;
        private Mock<IOptions<GetTokenParams>> GetTokenParams;
        private Mock<ITransactionDetailClient> TransactionDetailClient;

        #endregion

        #region Constructor

        static TransactionDetailTests()
        {
            Services = new ServiceCollection();
            Services.AddMvc(options => options.Filters.Add(new ValidationFilterAttribute()))
               .AddFluentValidation(options =>
               {
                   options.RegisterValidatorsFromAssemblyContaining<TransactionDetailValidator>();
               });
            Services.AddMediator();
            Services.AddScoped(typeof(ILoggerFactory), typeof(LoggerFactory));
            Services.AddScoped(typeof(ILogger<>), typeof(Logger<>));

            Provider = Services.BuildServiceProvider();
        }

        #endregion

        #region Main Flow

        [Fact(DisplayName = "Main Flow - Success", Skip = "true")]
        public async void ShouldTransactionDetailSuccessfully()
        {
            var adapter = GetTransactionDetail();

            GetTokenClient = new Mock<IGetTokenClient>();
            GetTokenParams = new Mock<IOptions<GetTokenParams>>();
            TransactionDetailClient = new Mock<ITransactionDetailClient>();

            var response = new BaseResult<TransactionDetailResult>
            {
                IsSuccess = true,
                Result = adapter.SuccessResult
            };

            GetTokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(GetBaseTokenResult()));
            TransactionDetailClient.Setup(x => x.TransactionDetailAsync(It.IsAny<TransactionDetailParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                                    .Returns(Task.FromResult(response));

            Services.AddSingleton(TransactionDetailClient.Object);
            Services.AddSingleton(GetTokenClient.Object);
            Services.AddSingleton(GetTokenParams.Object);

            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<CardsController>>();
            var mediator = GetInstance<IMediator>();
            var cardService = GetInstance<CardService>();
            var controller = new CardsController(mediator, logger.Object, cardService);

            var validate = new TransactionDetailValidator();
            validate.Validate(adapter.SuccessRequest);

            //var result = await controller.TransactionDetail(adapter.SuccessRequest, new CancellationToken());

            //Assert.IsType<OkObjectResult>(result);
        }

        [Fact(DisplayName = "AF1 – Required information not provided")]
        public void ShouldThrowRequestNotCompliantError()
        {
            var adapter = GetTransactionDetail();

            var validate = new TransactionDetailValidator();

            var error = validate.Validate(adapter.ErrorRequest);

            Assert.False(error.IsValid);
        }

        [Fact(DisplayName = "EF1 – Invalid information provided", Skip = "true")]
        public async Task ShouldThrowUnprocessableEntityException()
        {
            var adapter = GetTransactionDetail();

            GetTokenClient = new Mock<IGetTokenClient>();
            GetTokenParams = new Mock<IOptions<GetTokenParams>>();
            TransactionDetailClient = new Mock<ITransactionDetailClient>();

            var response = new BaseResult<TransactionDetailResult>
            {
                IsSuccess = false
            };

            GetTokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(GetBaseTokenResult()));
            TransactionDetailClient.Setup(x => x.TransactionDetailAsync(It.IsAny<TransactionDetailParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                                    .Returns(Task.FromResult(response));

            Services.AddSingleton(TransactionDetailClient.Object);
            Services.AddSingleton(GetTokenClient.Object);
            Services.AddSingleton(GetTokenParams.Object);

            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<CardsController>>();
            var mediator = GetInstance<IMediator>();
            var cardService = GetInstance<CardService>();
            var controller = new CardsController(mediator, logger.Object, cardService);

            var validate = new TransactionDetailValidator();
            //validate.Validate(adapter.SuccessRequest);

            //var result = await Assert.ThrowsAsync<UnprocessableEntityException>(() => controller.TransactionDetail(adapter.SuccessRequest, new CancellationToken()));

            //Assert.IsType<UnprocessableEntityException>(result);
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

        private TransactionDetailJsonAdapter GetTransactionDetail()
        {
            return ConvertJson.ReadJson<TransactionDetailJsonAdapter>("TransactionDetail.json");
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
