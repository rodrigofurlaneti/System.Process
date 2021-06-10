using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Process.Api.Controllers.V1;
using System.Process.Application.Commands.ValidateAccount;
using System.Process.Base.IntegrationTests;
using System.Process.CrossCutting.DependencyInjection;
using System.Process.Domain.ValueObjects;
using System.Phoenix.Common.Exceptions;
using System.Proxy.Silverlake.Inquiry;
using System.Proxy.Silverlake.Inquiry.Common;
using System.Proxy.Silverlake.Inquiry.Messages.Request;
using System.Proxy.Silverlake.Inquiry.Messages.Response;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace System.Process.IntegrationTests.Application.Commands.ValidateAccount
{
    public class ValidateAccountCommandTests
    {
        #region Properties

        private static ServiceCollection Services;
        private static ServiceProvider Provider;
        private static Mock<IHttpContextAccessor> HttpContextAccessor;

        private Mock<ILogger<ValidateAccountCommand>> Logger { get; }
        private Mock<IInquiryOperation> InquiryOperation { get; }
        private Mock<IOptions<ProcessConfig>> ProcessConfig { get; }

        #endregion

        #region Constructor

        public ValidateAccountCommandTests()
        {
            Logger = new Mock<ILogger<ValidateAccountCommand>>();
            InquiryOperation = new Mock<IInquiryOperation>();
            ProcessConfig = new Mock<IOptions<ProcessConfig>>();
            ProcessConfig.Setup(p => p.Value).Returns(ProcessIntegrationTestsConfiguration.ReadJson<ProcessConfig>("ProcessConfig.json"));

            Services = new ServiceCollection();
            Services.AddMediator();
            Services.AddScoped(typeof(ILoggerFactory), typeof(LoggerFactory));
            Services.AddScoped(typeof(ILogger<>), typeof(Logger<>));

            Provider = Services.BuildServiceProvider();
        }

        #endregion

        #region Tests

        [Trait("Integration", "Success")]
        [Fact(DisplayName = "ValidateAccountCommand_Success")]
        public async void ShouldValidateProcessuccessfully()
        {
            var ProcessearchResponse = await Task.FromResult(new ProcessearchResponse
            {
                ProcessearchRecInfo = new List<ProcessearchRecInfo>
                {
                    new ProcessearchRecInfo
                    {
                        AccountId = new AccountId
                        {
                            AccountNumber = "16561",
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

            InquiryOperation.Setup(x => x.ProcessearchAsync(It.IsAny<ProcessearchRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(ProcessearchResponse);

            Services.AddSingleton(Logger.Object);
            Services.AddSingleton(InquiryOperation.Object);
            Services.AddSingleton(ProcessConfig.Object);
            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<ProcessController>>();
            var mediator = GetInstance<IMediator>();
            var controller = new ProcessController(mediator, logger.Object);
            var request = new ValidateAccountRequest
            {
                AccountId = "16561",
                AccountType = "D"
            };

            var result = await controller.ValidateAccount(request.AccountId, request.AccountType, new CancellationToken());

            Assert.IsType<OkObjectResult>(result);
        }

        [Trait("Integration", "Error")]
        [Fact(DisplayName = "ValidateAccountCommand_Error")]
        public async void ShouldNotValidateAccount()
        {
            InquiryOperation
                .Setup(t => t.AccountInquiryAsync(It.IsAny<AccountInquiryRequest>(), It.IsAny<CancellationToken>())).Returns(Task<AccountInquiryResponse>.FromResult(
                    new AccountInquiryResponse
                    {
                        StatusDep = "1"
                    }));

            Services.AddSingleton(Logger.Object);
            Services.AddSingleton(InquiryOperation.Object);
            Services.AddSingleton(ProcessConfig.Object);
            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<ProcessController>>();
            var mediator = GetInstance<IMediator>();
            var controller = new ProcessController(mediator, logger.Object);
            var request = new ValidateAccountRequest();

            await Assert.ThrowsAsync<UnprocessableEntityException>(() => controller.ValidateAccount(request.AccountId, request.AccountType, new CancellationToken()));
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
