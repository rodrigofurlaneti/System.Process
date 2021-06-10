using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Process.Api.Controllers.V1;
using System.Process.Application.Queries.GetAccountHistory;
using System.Process.CrossCutting.DependencyInjection;
using System.Process.Domain.Repositories;
using System.Process.IntegrationTests.Adapters;
using System.Process.IntegrationTests.Common;
using System.Phoenix.Common.Exceptions;
using System.Proxy.Silverlake.Inquiry;
using System.Proxy.Silverlake.Inquiry.Messages.Request;
using System.Threading;
using Xunit;

namespace System.Process.IntegrationTests.Application.Queries.GetAccountHistory
{
    public class GetAccountHistoryQueryTests
    {
        #region Properties

        private static ServiceCollection Services;
        private static ServiceProvider Provider;
        private static Mock<IHttpContextAccessor> HttpContextAccessor;

        private Mock<ILogger<GetAccountHistoryQuery>> Logger { get; }
        private Mock<IInquiryOperation> InquiryOperation { get; }
        private Mock<ITransactionReadRepository> TransactionReadRepository { get; }
        #endregion

        #region Constructor

        public GetAccountHistoryQueryTests()
        {
            Logger = new Mock<ILogger<GetAccountHistoryQuery>>();
            InquiryOperation = new Mock<IInquiryOperation>();
            TransactionReadRepository = new Mock<ITransactionReadRepository>();

            Services = new ServiceCollection();
            Services.AddMediator();
            Services.AddScoped(typeof(ILoggerFactory), typeof(LoggerFactory));
            Services.AddScoped(typeof(ILogger<>), typeof(Logger<>));

            Provider = Services.BuildServiceProvider();
        }

        #endregion

        #region Tests

        [Trait("Integration", "Success")]
        [Fact(DisplayName = "GetAccountHistoryQuery_Success")]
        public async void ShouldGetAccountHistorySuccessfully()
        {
            var adapter = ConvertJson.ReadJson<GetAccountHistoryJsonAdapter>("GetAccountHistory.json");

            InquiryOperation.Setup(x => x.ProcessearchAsync(It.IsAny<ProcessearchRequest>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(adapter.AcctSrchSuccessResponse);

            InquiryOperation.Setup(x => x.AccountHistorySearchAsync(It.IsAny<AccountHistorySearchRequest>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(adapter.AcctHistSuccessResponse);

            TransactionReadRepository.Setup(x => x.Find()).Returns(adapter.TransactionSuccessResponse);

            Services.AddSingleton(Logger.Object);
            Services.AddSingleton(InquiryOperation.Object);
            Services.AddSingleton(TransactionReadRepository.Object);
            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<ProcessController>>();
            var mediator = GetInstance<IMediator>();
            var controller = new ProcessController(mediator, logger.Object);
            var request = adapter.SuccessRequest;

            var result = await controller.GetAccountHistory(request, new CancellationToken());

            Assert.IsType<OkObjectResult>(result);

        }

        [Trait("Integration", "Error")]
        [Fact(DisplayName = "GetAccountHistoryQuery_Error - Account Id Not Found")]
        public async void ShouldNotGetAccountHistoryAccountIdNotFound()
        {
            var adapter = ConvertJson.ReadJson<GetAccountHistoryJsonAdapter>("GetAccountHistory.json");

            InquiryOperation.Setup(x => x.ProcessearchAsync(It.IsAny<ProcessearchRequest>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(adapter.AcctSrchErrorResponse);

            Services.AddSingleton(Logger.Object);
            Services.AddSingleton(InquiryOperation.Object);
            Services.AddSingleton(TransactionReadRepository.Object);
            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<ProcessController>>();
            var mediator = GetInstance<IMediator>();
            var controller = new ProcessController(mediator, logger.Object);
            var request = adapter.SuccessRequest;
            
            await Assert.ThrowsAsync<NotFoundException>(() => controller.GetAccountHistory(adapter.SuccessRequest, new CancellationToken()));
        }

        [Trait("Integration", "Error")]
        [Fact(DisplayName = "GetAccountHistoryQuery_Error - Account Status Not valid")]
        public async void ShouldNotGetAccountHistoryProcesstatusNotvalid()
        {
            var adapter = ConvertJson.ReadJson<GetAccountHistoryJsonAdapter>("GetAccountHistory.json");

            InquiryOperation.Setup(x => x.ProcessearchAsync(It.IsAny<ProcessearchRequest>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(adapter.AcctSrchInvalidStatusResponse);

            Services.AddSingleton(Logger.Object);
            Services.AddSingleton(InquiryOperation.Object);
            Services.AddSingleton(TransactionReadRepository.Object);
            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<ProcessController>>();
            var mediator = GetInstance<IMediator>();
            var controller = new ProcessController(mediator, logger.Object);
            var request = adapter.SuccessRequest;

            var ex = await Assert.ThrowsAsync<UnprocessableEntityException>(() => controller.GetAccountHistory(adapter.SuccessRequest, new CancellationToken()));

            Assert.Contains("Account Status: Escheat not valid for querying transaction history.", ex.Message);
        }

        /*
        [Trait("Integration", "Error")]
        [Fact(DisplayName = "FindReceiversQuery_Error - No receiver found")]
        public async void ShouldNotFindReceiversNotFound()
        {
            var receivers = new List<Receiver>();
            ReceiverReadRepository
               .Setup(t => t.FindByCustomerId(It.IsAny<string>())).Returns(receivers);
            ReceiverReadRepository
                .Setup(t => t.FindByBankType(It.IsAny<string>(), It.IsAny<string>())).Returns(receivers);

            Services.AddSingleton(Logger.Object);
            Services.AddSingleton(ReceiverReadRepository.Object);
            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<ReceiversController>>();
            var mediator = GetInstance<IMediator>();
            var controller = new ReceiversController(mediator, logger.Object);
            var request = new FindReceiversRequest
            {
                CustomerId = "13141",
                InquiryType = "A"
            };

            await Assert.ThrowsAsync<NotFoundException>(() => controller.Find(request.CustomerId, request.InquiryType, new CancellationToken()));
        }*/
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
    }
}
