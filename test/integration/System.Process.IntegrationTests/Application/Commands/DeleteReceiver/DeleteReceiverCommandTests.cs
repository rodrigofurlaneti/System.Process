using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Process.Api.Controllers.V1;
using System.Process.Application.Commands.DeleteReceiver;
using System.Process.CrossCutting.DependencyInjection;
using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Phoenix.Common.Exceptions;
using System.Collections.Generic;
using System.Threading;
using Xunit;

namespace System.Process.IntegrationTests.Application.Commands.DeleteReceiver
{
    public class DeleteReceiverCommandTests
    {
        #region Properties

        private static ServiceCollection Services;
        private static ServiceProvider Provider;
        private static Mock<IHttpContextAccessor> HttpContextAccessor;

        private Mock<ILogger<DeleteReceiverCommand>> Logger { get; }
        private Mock<IReceiverReadRepository> ReceiverReadRepository { get; }
        private Mock<IReceiverWriteRepository> ReceiverWriteRepository { get; }

        #endregion

        #region Constructor

        public DeleteReceiverCommandTests()
        {
            Logger = new Mock<ILogger<DeleteReceiverCommand>>();
            ReceiverReadRepository = new Mock<IReceiverReadRepository>();
            ReceiverWriteRepository = new Mock<IReceiverWriteRepository>();

            Services = new ServiceCollection();
            Services.AddMediator();
            Services.AddScoped(typeof(ILoggerFactory), typeof(LoggerFactory));
            Services.AddScoped(typeof(ILogger<>), typeof(Logger<>));

            Provider = Services.BuildServiceProvider();
        }

        #endregion

        #region Tests

        [Trait("Integration", "Success")]
        [Fact(DisplayName = "DeleteReceiverCommand_Success")]
        public async void ShouldDeleteReceiverSuccessfully()
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
                .Setup(r => r.Find(It.IsAny<int>())).Returns(receivers);
            ReceiverWriteRepository
               .Setup(r => r.Remove(It.IsAny<Receiver>(), It.IsAny<CancellationToken>())).Verifiable();

            Services.AddSingleton(Logger.Object);
            Services.AddSingleton(ReceiverReadRepository.Object);
            Services.AddSingleton(ReceiverWriteRepository.Object);
            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<ReceiversController>>();
            var mediator = GetInstance<IMediator>();
            var controller = new ReceiversController(mediator, logger.Object);
            var request = "12334";

            var result = await controller.Delete(request, new CancellationToken());

            Assert.IsType<OkObjectResult>(result);

        }

        [Trait("Integration", "Error")]
        [Fact(DisplayName = "DeleteReceiverCommand_Error")]
        public async void ShouldSendHandleNoReceiverFoundAsync()
        {
            var receivers = new List<Receiver>();

            ReceiverReadRepository
                 .Setup(r => r.Find(It.IsAny<int>())).Throws(new NotFoundException("not found"));
            ReceiverWriteRepository
               .Setup(r => r.Remove(It.IsAny<Receiver>(), It.IsAny<CancellationToken>())).Verifiable();

            Services.AddSingleton(Logger.Object);
            Services.AddSingleton(ReceiverReadRepository.Object);
            Services.AddSingleton(ReceiverWriteRepository.Object);
            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<ReceiversController>>();
            var mediator = GetInstance<IMediator>();
            var controller = new ReceiversController(mediator, logger.Object);
            var request = "817871";

            await Assert.ThrowsAsync<NotFoundException>(() => controller.Delete(request, new CancellationToken()));
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
