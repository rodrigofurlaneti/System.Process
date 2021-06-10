using FluentAssertions;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Process.Api.Controllers.V1;
using System.Process.Application.Commands.DeleteReceiver;
using System.Process.CrossCutting.DependencyInjection;
using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Phoenix.Common.Exceptions;
using System.Phoenix.Web.Filters;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Xunit;

namespace System.Process.IntegrationTests.Backend.Api.Receivers
{
    public class DeleteReceiverTests
    {
        #region Properties

        private static ServiceProvider Provider;
        private static Mock<IHttpContextAccessor> HttpContextAccessor;
        private static ServiceCollection Services;

        #endregion

        #region Constructor
        static DeleteReceiverTests()
        {
            Services = new ServiceCollection();
            Services.AddMvc(options => options.Filters.Add(new ValidationFilterAttribute()))
               .AddFluentValidation(options =>
               {
                   options.RegisterValidatorsFromAssemblyContaining<DeleteReceiverValidatorAttribute>();
               });
            Services.AddMediator();
            Services.AddScoped(typeof(ILoggerFactory), typeof(LoggerFactory));
            Services.AddScoped(typeof(ILogger<>), typeof(Logger<>));

            Provider = Services.BuildServiceProvider();
        }

        #endregion

        #region Main Flow

        [Fact(DisplayName = "Main Flow - Success")]
        public async void ShouldValidateProcessuccessfully()
        {
            var receiverReadRepository = new Mock<IReceiverReadRepository>();
            var receiverWriteRepository = new Mock<IReceiverWriteRepository>();

            var receivers = new List<Receiver>
            {
                new Receiver
                {
                    AccountNumber = "8318033",
                    AccountType = "D"
                }
            };
            receiverReadRepository
                .Setup(r => r.Find(It.IsAny<int>())).Returns(receivers);

            receiverWriteRepository
               .Setup(r => r.Remove(It.IsAny<Receiver>(), It.IsAny<CancellationToken>())).Verifiable();

            Services.AddSingleton(receiverReadRepository.Object);
            Services.AddSingleton(receiverWriteRepository.Object);

            Provider = Services.BuildServiceProvider();
            var mediator = GetInstance<IMediator>();
            var logger = new Mock<ILogger<ReceiversController>>();

            var receiversController = new ReceiversController(mediator, logger.Object);
            var cancellationToken = new CancellationToken();

            var result = await receiversController.Delete("8318033", cancellationToken);

            var objectResult = Assert.IsType<OkObjectResult>(result);
            var deleteReceiverResponse = Assert.IsAssignableFrom<DeleteReceiverResponse>(objectResult.Value);
            Assert.NotNull(deleteReceiverResponse.Code);
        }

        #endregion

        #region Alternative Flows

        [Fact(DisplayName = "AF1 – Required information not provided - Should Returns Errors")]
        public void ShouldIndicatesInformationIsNotProvided()
        {
            var httpContext = new DefaultHttpContext();
            var routeData = new RouteData();
            var action = typeof(ProcessController).GetMethod(nameof(ProcessController.ValidateAccount));
            var actionDescriptor = new ControllerActionDescriptor()
            {
                ActionName = action.Name,
                ControllerName = typeof(ProcessController).Name,
                DisplayName = action.Name,
                MethodInfo = action,
            };
            var actionContext = new ActionContext(httpContext, routeData, actionDescriptor);
            var operationRead = new Mock<IReceiverReadRepository>();
            var operationWrite = new Mock<IReceiverWriteRepository>();
            Services.AddSingleton(operationRead.Object);
            Services.AddSingleton(operationWrite.Object);
            Provider = Services.BuildServiceProvider();
            var mediator = GetInstance<IMediator>();
            var logger = new Mock<ILogger<ReceiversController>>();
            var receiversController = new ReceiversController(mediator, logger.Object);
            var atrributeFilter = action.GetCustomAttribute<DeleteReceiverValidatorAttribute>();
            var actionArguments = new Dictionary<string, object>
            {
                ["receiverId"] = string.Empty
            };
            var filterMetadata = new List<IFilterMetadata>() { atrributeFilter };
            var actionExecutedContext = new ActionExecutingContext(actionContext, filterMetadata, actionArguments, receiversController);
            var attribute = new DeleteReceiverValidatorAttribute();

            attribute.OnActionExecuting(actionExecutedContext);

            actionExecutedContext.Result.Should().NotBeNull().And.BeOfType<BadRequestObjectResult>("The ReceiverId cannot be null or empty. Please ensure you have entered a valid ReceiverId ");
        }

        [Fact(DisplayName = "AF2 – Receiver not found - Should Returns Errors")]
        public async void ShouldIndicatesAccountInformationNotFound()
        {
            var receiverReadRepository = new Mock<IReceiverReadRepository>();
            var receiverWriteRepository = new Mock<IReceiverWriteRepository>();
            var operationWrite = new Mock<IReceiverWriteRepository>();

            receiverReadRepository
                .Setup(r => r.Find(It.IsAny<int>())).Throws(new NotFoundException("not found"));

            receiverWriteRepository
              .Setup(r => r.Remove(It.IsAny<Receiver>(), It.IsAny<CancellationToken>())).Verifiable();

            Services.AddSingleton(receiverReadRepository.Object);
            Services.AddSingleton(receiverWriteRepository.Object);

            Provider = Services.BuildServiceProvider();
            var mediator = GetInstance<IMediator>();
            var logger = new Mock<ILogger<ReceiversController>>();

            var receiversController = new ReceiversController(mediator, logger.Object);
            var cancellationToken = new CancellationToken();

            await Assert.ThrowsAsync<NotFoundException>(() => receiversController.Delete("8318033", cancellationToken));
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
