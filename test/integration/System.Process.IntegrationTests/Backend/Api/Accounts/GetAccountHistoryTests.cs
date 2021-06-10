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
using System.Process.IntegrationTests.Adapters;
using System.Process.IntegrationTests.Common;
using System.Phoenix.Common.Exceptions;
using System.Phoenix.Web.Filters;
using System.Proxy.Silverlake.Base.Exceptions;
using System.Proxy.Silverlake.Inquiry;
using System.Proxy.Silverlake.Inquiry.Messages.Request;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace System.Process.IntegrationTests.Backend.Api.Process
{
    public class GetAccountHistoryTests
    {
        #region Properties

        private static ServiceProvider Provider;
        private static Mock<IHttpContextAccessor> HttpContextAccessor;
        private static ServiceCollection Services;

        #endregion

        #region Constructor
        static GetAccountHistoryTests()
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

        [Fact(DisplayName = "Main Flow - Success")]
        public async void ShouldGetAccountHistorySuccessfully()
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

        [Fact(DisplayName = "AF1 – Required information not provided")]
        public void ShouldThrowRequestNotCompliantError()
        {
            var adapter = ConvertJson.ReadJson<GetAccountHistoryJsonAdapter>("GetAccountHistory.json");

            var validate = new GetAccountHistoryValidator();

            var error = validate.Validate(adapter.ErrorRequest);
            var errorStartDate = validate.Validate(adapter.ErrorRequestStartDate);
            var errorEndDate = validate.Validate(adapter.ErrorRequestEndDate);

            Assert.False(error.IsValid);
            Assert.False(errorStartDate.IsValid);
            Assert.False(errorEndDate.IsValid);
        }

        [Fact(DisplayName = "AF2 – Account specified is an external account")]
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

        [Fact(DisplayName = "AF3 – Account status not valid")]
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

        [Fact(DisplayName = "EF1 – Failed connection with Jack Henry")]
        public async Task ShouldThrowJackHenryProxyException()
        {
            var adapter = ConvertJson.ReadJson<GetAccountHistoryJsonAdapter>("GetAccountHistory.json");

            var operation = new Mock<IInquiryOperation>();
            var repo = new Mock<ITransactionReadRepository>();

            operation.Setup(x => x.ProcessearchAsync(It.IsAny<ProcessearchRequest>(), It.IsAny<CancellationToken>()))
                .Callback(() =>
                {
                    throw new SilverlakeException("Service Exception Error");
                });

            Services.AddSingleton(operation.Object);
            Services.AddSingleton(repo.Object);

            Provider = Services.BuildServiceProvider();

            var mediator = GetInstance<IMediator>();
            var logger = new Mock<ILogger<ProcessController>>();

            var accountController = new ProcessController(mediator, logger.Object);

            var cancellationToken = new CancellationToken();

            var result = await Assert.ThrowsAsync<SilverlakeException>(() => accountController.GetAccountHistory(adapter.SuccessRequest, cancellationToken));

            Assert.IsType<SilverlakeException>(result);
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
