using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Process.Api.Controllers.V1;
using System.Process.Application.Queries.GetAccountHistory;
using System.Process.CrossCutting.DependencyInjection;
using System.Process.Domain.Repositories;
using System.Process.UnitTests.Adapters;
using System.Process.UnitTests.Common;
using System.Phoenix.Common.Exceptions;
using System.Phoenix.Web.Filters;
using System.Proxy.Silverlake.Inquiry;
using System.Proxy.Silverlake.Inquiry.Messages.Request;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace System.Process.UnitTests.Backend.Api.Controllers.V1
{
    public class AccountControllerTests
    {
        #region Properties

        private static ServiceProvider Provider;
        private static Mock<IHttpContextAccessor> HttpContextAccessor;
        private static ServiceCollection Services;

        #endregion

        #region Constructor
        static AccountControllerTests()
        {
            var appSettings = new Dictionary<string, string>
            {
                {"MongoDB:Transaction:Database", "test"},
                {"MongoDB:Transaction:Collection", "test"},
                {"MongoDB:Transaction:ConnectionString", "test"}
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(appSettings)
                .Build();

            Services = new ServiceCollection();
            Services.AddMvc(options => options.Filters.Add(new ValidationFilterAttribute()))
               .AddFluentValidation(options =>
               {
                   options.RegisterValidatorsFromAssemblyContaining<GetAccountHistoryValidator>();
               });
            Services.AddMediator();
            Services.AddScoped(typeof(ILoggerFactory), typeof(LoggerFactory));
            Services.AddScoped(typeof(ILogger<>), typeof(Logger<>));

            Provider = Services.BuildServiceProvider();
        }

        #endregion

        #region Tests

        [Fact(DisplayName = "Should Get Account History Successfully")]
        public async Task ShouldGetAccountHistorySuccessfully()
        {
            var adapter = ConvertJson.ReadJson<GetAccountHistoryJsonAdapter>("GetAccountHistory.json");

            var operation = new Mock<IInquiryOperation>();
            var repo = new Mock<ITransactionReadRepository>();

            operation.Setup(x => x.ProcessearchAsync(It.IsAny<ProcessearchRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(adapter.AcctSrchSuccessResponse);
            operation.Setup(x => x.AccountHistorySearchAsync(It.IsAny<AccountHistorySearchRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(adapter.AcctHistSuccessResponse);
            repo.Setup(x => x.Find()).Returns(adapter.TransactionSuccessResponse);

            Services.AddSingleton(operation.Object);
            Services.AddSingleton(repo.Object);

            Provider = Services.BuildServiceProvider();

            var mediator = GetInstance<IMediator>();
            var logger = new Mock<ILogger<ProcessController>>();

            var accountController = new ProcessController(mediator, logger.Object);

            var cancellationToken = new CancellationToken();

            var validate = new GetAccountHistoryValidator();
            validate.Validate(adapter.SuccessRequest);

            var result = await accountController.GetAccountHistory(adapter.SuccessRequest, cancellationToken);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact(DisplayName = "Should Throw Not Found Exception")]
        public async Task ShouldThrowNotFoundException()
        {
            var adapter = ConvertJson.ReadJson<GetAccountHistoryJsonAdapter>("GetAccountHistory.json");

            var operation = new Mock<IInquiryOperation>();
            var repo = new Mock<ITransactionReadRepository>();

            operation.Setup(x => x.ProcessearchAsync(It.IsAny<ProcessearchRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(adapter.AcctSrchErrorResponse);

            Services.AddSingleton(operation.Object);
            Services.AddSingleton(repo.Object);

            Provider = Services.BuildServiceProvider();

            var mediator = GetInstance<IMediator>();
            var logger = new Mock<ILogger<ProcessController>>();

            var accountController = new ProcessController(mediator, logger.Object);

            var cancellationToken = new CancellationToken();

            var result = await Assert.ThrowsAsync<NotFoundException>(() => accountController.GetAccountHistory(adapter.SuccessRequest, cancellationToken));

            Assert.IsType<NotFoundException>(result);
        }

        [Fact(DisplayName = "Should Throw Unprocessable Entity Exception")]
        public async Task ShouldThrowUnprocessableEntityException()
        {
            var adapter = ConvertJson.ReadJson<GetAccountHistoryJsonAdapter>("GetAccountHistory.json");

            var operation = new Mock<IInquiryOperation>();
            var repo = new Mock<ITransactionReadRepository>();

            operation.Setup(x => x.ProcessearchAsync(It.IsAny<ProcessearchRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(adapter.AcctSrchInvalidStatusResponse);

            Services.AddSingleton(operation.Object);
            Services.AddSingleton(repo.Object);

            Provider = Services.BuildServiceProvider();

            var mediator = GetInstance<IMediator>();
            var logger = new Mock<ILogger<ProcessController>>();

            var accountController = new ProcessController(mediator, logger.Object);

            var cancellationToken = new CancellationToken();

            var result = await Assert.ThrowsAsync<UnprocessableEntityException>(() => accountController.GetAccountHistory(adapter.SuccessRequest, cancellationToken));

            Assert.IsType<UnprocessableEntityException>(result);
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
