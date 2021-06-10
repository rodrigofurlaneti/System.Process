using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Process.Api.Controllers.V1;
using System.Process.Application.Commands.AddReceiver;
using System.Process.CrossCutting.DependencyInjection;
using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Phoenix.Common.Exceptions;
using System.Collections.Generic;
using System.Threading;
using Xunit;

namespace System.Process.IntegrationTests.Application.Commands.AddReceiver
{
    public class AddReceiverCommandTests
    {
        #region Properties
    
        private static ServiceCollection Services;
        private static ServiceProvider Provider;
        private static Mock<IHttpContextAccessor> HttpContextAccessor;
        private Mock<ILogger<AddReceiverCommand>> Logger { get; set; }

        private static Mock<IReceiverReadRepository> ReceiverReadRepository;
        private static Mock<IReceiverWriteRepository> ReceiverWriteRepository;
      
        #endregion
    
        #region Constructor
    
        public AddReceiverCommandTests()
        {
            Services = new ServiceCollection();
            Services.AddMediator();
            Services.AddScoped(typeof(ILoggerFactory), typeof(LoggerFactory));
            Services.AddScoped(typeof(ILogger<>), typeof(Logger<>));

            ReceiverReadRepository = new Mock<IReceiverReadRepository>();
            ReceiverWriteRepository = new Mock<IReceiverWriteRepository>();

            Provider = Services.BuildServiceProvider();
        }
    
        #endregion
    
        #region Tests
    
        [Trait("Integration", "Success")]
        [Fact(DisplayName = "AddReceiverCommand_Success")]
        public async void ShouldAddReceiverSuccessfully()
        {
            var receivers = new List<Receiver>();
            ReceiverReadRepository
                .Setup(r => r.FindExistent(It.IsAny<Receiver>())).Returns(receivers);

            Services.AddSingleton(ReceiverReadRepository.Object);
            Services.AddSingleton(ReceiverWriteRepository.Object);

            Provider = Services.BuildServiceProvider();
    
            var logger = new Mock<ILogger<ReceiversController>>();
            var mediator = GetInstance<IMediator>();
            var controller = new ReceiversController(mediator, logger.Object);
            var request = new AddReceiverRequest
            {
                AccountNumber = "16516",
                AccountType = "D",
                BankType = "S",
                CompanyName = "",
                CustomerId = "123",
                EmailAdress = "email@mail.com",
                FirstName = "Raul",
                LastName = "Segal",
                PhoneNumber = "+5512727222",
                ReceiverType = "D",
                RoutingNumber = "1"
            };

            var result = await controller.Add(request, new CancellationToken());
    
            Assert.IsType<CreatedResult>(result);
    
        }
    
        [Trait("Integration", "Error")]
        [Fact(DisplayName = "AddReceiverCommand_Error")]
        public async void ShouldNotAddReceiverAlreadyExists()
        {
            var request = new AddReceiverRequest
            {
                AccountNumber = "16516",
                AccountType = "D",
                BankType = "S",
                CompanyName = "",
                CustomerId = "123",
                EmailAdress = "email@mail.com",
                FirstName = "Raul",
                LastName = "Segal",
                PhoneNumber = "+5512727222",
                ReceiverType = "D",
                RoutingNumber = "1"
            };
            var receivers = new List<Receiver>
            {
                new Receiver
                {
                    AccountNumber = "65151",
                    AccountType = "D"
                }
            };
            ReceiverReadRepository
                .Setup(r => r.FindExistent(It.IsAny<Receiver>())).Returns(receivers);

            Services.AddSingleton(ReceiverReadRepository.Object);
            Services.AddSingleton(ReceiverWriteRepository.Object);
            Provider = Services.BuildServiceProvider();
    
            var logger = new Mock<ILogger<ReceiversController>>();
            var mediator = GetInstance<IMediator>();
            var controller = new ReceiversController(mediator, logger.Object);
    
            await Assert.ThrowsAsync<ConflictException>(() => controller.Add(request, new CancellationToken()));
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