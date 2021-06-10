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
using System.Process.Application.Commands.ValidateAccount;
using System.Process.CrossCutting.DependencyInjection;
using System.Phoenix.Common.Exceptions;
using System.Phoenix.Web.Filters;
using System.Proxy.Silverlake.Inquiry;
using System.Proxy.Silverlake.Inquiry.Common;
using System.Proxy.Silverlake.Inquiry.Messages.Request;
using System.Proxy.Silverlake.Inquiry.Messages.Response;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace System.Process.IntegrationTests.Backend.Api.Process
{
    public class ValidateAccountTests
    {
        #region Properties

        private static ServiceProvider Provider;
        private static Mock<IHttpContextAccessor> HttpContextAccessor;
        private static ServiceCollection Services;

        #endregion

        #region Constructor
        static ValidateAccountTests()
        {
            Services = new ServiceCollection();
            Services.AddMvc(options => options.Filters.Add(new ValidationFilterAttribute()))
               .AddFluentValidation(options =>
               {
                   options.RegisterValidatorsFromAssemblyContaining<ValidateAccountValidatorAttribute>();
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
            var operation = new Mock<IInquiryOperation>();
            var ProcessearchResponse = await Task.FromResult(new ProcessearchResponse
            {
                ProcessearchRecInfo = new List<ProcessearchRecInfo>
                {
                    new ProcessearchRecInfo
                    {
                        AccountId = new AccountId
                        {
                            AccountNumber = "8318033",
                            AccountType = "D"
                        },
                        Amount = 123,
                        Processtatus = "1",
                        AvailableBalance = 12,
                        ProductCode = "string",
                        ProductDesc = "string",
                        ProcesstatusDesc = "Closed"
                    }
                }
            });
            operation.Setup(x => x.ProcessearchAsync(It.IsAny<ProcessearchRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(ProcessearchResponse);

            Services.AddSingleton(operation.Object);

            Provider = Services.BuildServiceProvider();
            var mediator = GetInstance<IMediator>();
            var logger = new Mock<ILogger<ProcessController>>();

            var accountController = new ProcessController(mediator, logger.Object);
            var cancellationToken = new CancellationToken();

            var result = await accountController.ValidateAccount("8318033", "D", cancellationToken);
           
            var objectResult = Assert.IsType<OkObjectResult>(result);
            var validateAccountResponse = Assert.IsAssignableFrom<ValidateAccountResponse>(objectResult.Value);
            Assert.True(validateAccountResponse.Valid);
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
            var operation = new Mock<IInquiryOperation>();
            Services.AddSingleton(operation.Object);
            Provider = Services.BuildServiceProvider();
            var mediator = GetInstance<IMediator>();
            var logger = new Mock<ILogger<ProcessController>>();
            var accountController = new ProcessController(mediator, logger.Object);
            var atrributeFilter = action.GetCustomAttribute<ValidateAccountValidatorAttribute>();
            var actionArguments = new Dictionary<string, object>
            {
                ["accountId"] = string.Empty,
                ["accountType"] = string.Empty
            };
            var filterMetadata = new List<IFilterMetadata>() { atrributeFilter };
            var actionExecutedContext = new ActionExecutingContext(actionContext, filterMetadata, actionArguments, accountController);
            var attribute = new ValidateAccountValidatorAttribute();

            attribute.OnActionExecuting(actionExecutedContext);

            actionExecutedContext.Result.Should().NotBeNull().And.BeOfType<BadRequestObjectResult>("The AccountId cannot be null or empty. Please ensure you have entered a valid AccountId ");
            actionExecutedContext.Result.Should().NotBeNull().And.BeOfType<BadRequestObjectResult>("The AccountType cannot be null or empty. Please ensure you have entered a valid AccountType ");
        }

        [Fact(DisplayName = "AF2 – Account Information not found in Core Banking - Should Returns Errors")]
        public async void ShouldIndicatesAccountInformationNotFound()
        {
            var operation = new Mock<IInquiryOperation>();
            operation.Setup(x => x.AccountInquiryAsync(It.IsAny<AccountInquiryRequest>(), It.IsAny<CancellationToken>())).ThrowsAsync(new UnprocessableEntityException("AccountId not found", ""));

            Services.AddSingleton(operation.Object);

            Provider = Services.BuildServiceProvider();
            var mediator = GetInstance<IMediator>();
            var logger = new Mock<ILogger<ProcessController>>();

            var accountController = new ProcessController(mediator, logger.Object);
            var cancellationToken = new CancellationToken();

            await Assert.ThrowsAsync<UnprocessableEntityException>(() => accountController.ValidateAccount("40600009", "D", cancellationToken));
        }

        #endregion

        #region Exception Flows

        [Fact(DisplayName = "EF1 – Failed connection with Core Banking - Should Throws Exception")]
        public async Task ShouldThrowsNotFoundException()
        {
            var operation = new Mock<IInquiryOperation>();
            operation.Setup(x => x.AccountInquiryAsync(It.IsAny<AccountInquiryRequest>(), It.IsAny<CancellationToken>())).Throws(new Exception());

            Services.AddSingleton(operation.Object);
            Provider = Services.BuildServiceProvider();

            var mediator = GetInstance<IMediator>();
            var logger = new Mock<ILogger<ProcessController>>();
            var accountController = new ProcessController(mediator, logger.Object);
            var cancellationToken = new CancellationToken();

            await Assert.ThrowsAsync<UnprocessableEntityException>(() => accountController.ValidateAccount("154154", "D", cancellationToken));
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
