using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Process.Api.Controllers.V1;
using System.Process.Application.Queries.FindReceivers;
using System.Process.CrossCutting.DependencyInjection;
using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Phoenix.Common.Exceptions;
using System.Collections.Generic;
using System.Threading;
using Xunit;

namespace System.Process.IntegrationTests.Application.Queries.FindReceivers
{
    public class FindReceiversQueryTests
    {
        #region Properties

        private static ServiceCollection Services;
        private static ServiceProvider Provider;
        private static Mock<IHttpContextAccessor> HttpContextAccessor;

        private Mock<ILogger<FindReceiversQuery>> Logger { get; }
        private Mock<IReceiverReadRepository> ReceiverReadRepository { get; }
        #endregion

        #region Constructor

        public FindReceiversQueryTests()
        {
            Logger = new Mock<ILogger<FindReceiversQuery>>();
            ReceiverReadRepository = new Mock<IReceiverReadRepository>();

            Services = new ServiceCollection();
            Services.AddMediator();
            Services.AddScoped(typeof(ILoggerFactory), typeof(LoggerFactory));
            Services.AddScoped(typeof(ILogger<>), typeof(Logger<>));

            Provider = Services.BuildServiceProvider();
        }

        #endregion

        #region Tests

        [Trait("Integration", "Success")]
        [Fact(DisplayName = "FindReceiversQuery_Success")]
        public async void ShouldFindReceiversSuccessfully()
        {
            var receivers = new List<Receiver>
            {
                new Receiver
                {
                   AccountNumber = "13141",
                   AccountType = "D"
                }
            };
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
                InquiryType = "A",
                Ownership = "A"
            };

            var result = await controller.Find(request.CustomerId, request.InquiryType, request.Ownership, new CancellationToken());

            Assert.IsType<OkObjectResult>(result);

        }

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
                InquiryType = "A",
                Ownership = "A"
            };

            await Assert.ThrowsAsync<NotFoundException>(() => controller.Find(request.CustomerId, request.InquiryType, request.Ownership, new CancellationToken()));
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
    }
}
