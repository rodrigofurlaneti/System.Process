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
using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Process.IntegrationTests.Adapters;
using System.Process.IntegrationTests.Common;
using System.Phoenix.Common.Exceptions;
using System.Proxy.Fis.GetToken;
using System.Proxy.Fis.GetToken.Messages;
using System.Proxy.Fis.Messages;
using System.Proxy.Fis.TransactionDetail;
using System.Proxy.Fis.TransactionDetail.Messages;
using System.Proxy.Silverlake.Base.Exceptions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace System.Process.IntegrationTests.Application.Commands.TransactionDetail
{
    public class TransactionDetailCommandTests
    {
        #region Properties

        private static ServiceCollection Services;
        private static ServiceProvider Provider;
        private static Mock<IHttpContextAccessor> HttpContextAccessor;

        private Mock<ILogger<TransactionDetailCommand>> Logger { get; }
        private Mock<IGetTokenClient> GetTokenClient { get; }
        private Mock<IOptions<GetTokenParams>> GetTokenParams { get; }
        private Mock<ITransactionDetailClient> TransactionDetailClient { get; }
        private Mock<ICardReadRepository> CardReadRepository { get; }
        private Mock<ICardService> CardService { get; }

        #endregion

        #region Constructor

        public TransactionDetailCommandTests()
        {
            Logger = new Mock<ILogger<TransactionDetailCommand>>();
            GetTokenClient = new Mock<IGetTokenClient>();
            CardService = new Mock<ICardService>();
            GetTokenParams = new Mock<IOptions<GetTokenParams>>();
            TransactionDetailClient = new Mock<ITransactionDetailClient>();
            CardReadRepository = new Mock<ICardReadRepository>();

            Services = new ServiceCollection();
            Services.AddMediator();
            Services.AddScoped(typeof(ILoggerFactory), typeof(LoggerFactory));
            Services.AddScoped(typeof(ILogger<>), typeof(Logger<>));

            Provider = Services.BuildServiceProvider();
        }

        #endregion

        #region Tests

        [Trait("Integration", "Success")]
        [Fact(DisplayName = "TransactionDetailCommand_Success")]
        public async void ShouldSendHandleTransactionDetailSuccessfully()
        {
            var adapter = GetTransactionDetail();
            var response = new BaseResult<TransactionDetailResult>
            {
                IsSuccess = true,
                Result = adapter.SuccessResult
            };

            GetTokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(GetBaseTokenResult()));
            TransactionDetailClient.Setup(x => x.TransactionDetailAsync(It.IsAny<TransactionDetailParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                                    .Returns(Task.FromResult(response));
            CardReadRepository.Setup(x => x.Find(It.IsAny<int>())).Returns(GetCardResult());

            Services.AddSingleton(Logger.Object);
            Services.AddSingleton(TransactionDetailClient.Object);
            Services.AddSingleton(GetTokenClient.Object);
            Services.AddSingleton(GetTokenParams.Object);
            Services.AddSingleton(CardReadRepository.Object);

            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<CardsController>>();
            var mediator = GetInstance<IMediator>();
            var controller = new CardsController(mediator, logger.Object, CardService.Object);
            var request = new ConsultCardsByKeyTransactionRequest();

            var result = await controller.TransactionDetail(request, new CancellationToken());

            Assert.IsType<OkObjectResult>(result);

        }
        
        [Trait("Integration", "Error")]
        [Fact(DisplayName = "Required information not provided")]
        public void ShouldTranserMoneyWireError()
        {
            var validator = new TransactionDetailValidator();
            var request = new TransactionDetailRequest();

            var error = validator.Validate(request);

            Assert.False(error.IsValid);
        }
        
        [Trait("Integration", "Error")]
        [Fact(DisplayName = "TransactionDetailCommand_Error")]
        public async void ShouldSendUnprocessableEntityException()
        {
            var adapter = GetTransactionDetail();
            var response = new BaseResult<TransactionDetailResult>
            {
                IsSuccess = true,
                Result = adapter.SuccessResult
            };

            GetTokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(GetBaseTokenResult()));
            TransactionDetailClient.Setup(x => x.TransactionDetailAsync(It.IsAny<TransactionDetailParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());
            CardReadRepository.Setup(x => x.Find(It.IsAny<int>())).Returns(GetCardResult());

            Services.AddSingleton(Logger.Object);
            Services.AddSingleton(TransactionDetailClient.Object);
            Services.AddSingleton(GetTokenClient.Object);
            Services.AddSingleton(GetTokenParams.Object);
            Services.AddSingleton(CardReadRepository.Object);

            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<CardsController>>();
            var mediator = GetInstance<IMediator>();
            var controller = new CardsController(mediator, logger.Object, CardService.Object);
            var request = new ConsultCardsByKeyTransactionRequest();

            await Assert.ThrowsAsync<UnprocessableEntityException>(() => controller.TransactionDetail(request, new CancellationToken()));
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

        #endregion

        #region Private Methods

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

        private IList<Card> GetCardResult()
        {
            return new List<Card>
            {
                new Card
                {
                    Pan = "435938594389584395"
                }
            };
        }

        #endregion  
    }
}