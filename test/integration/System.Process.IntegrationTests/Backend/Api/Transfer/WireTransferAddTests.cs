using FluentAssertions;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Process.Api.Controllers.V1;
using System.Process.Application.Commands.WireTransfer;
using System.Process.Base.IntegrationTests;
using System.Process.CrossCutting.DependencyInjection;
using System.Process.Domain.ValueObjects;
using System.Phoenix.Common.Exceptions;
using System.Phoenix.Web.Filters;
using System.Proxy.Silverlake.TranferWire;
using System.Proxy.Silverlake.TranferWire.Messages.Request;
using System.Proxy.Silverlake.TranferWire.Messages.Response;
using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;

namespace System.Process.IntegrationTests.Backend.Api.Transfer
{
    public class WireTransferAddTests
    {
        #region Properties

        private static ServiceProvider Provider;
        private static Mock<IHttpContextAccessor> HttpContextAccessor;
        private static ServiceCollection Services;
        private CancellationToken CancellationToken { get; set; }
        private Mock<ILogger<WireTransferCommand>> Logger { get; set; }
        private Mock<ITransferWireOperation> TransferWireOperation { get; set; }
        private Mock<ILogger<TransferController>> ControllerLogger { get; set; }
        private Mock<IOptions<ProcessConfig>> ProcessConfig { get; set; }
        private IConfiguration Configuration;
        #endregion

        #region Constructor

        static WireTransferAddTests()
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
                   options.RegisterValidatorsFromAssemblyContaining<WireTransferValidator>();
               });
            Services.AddMediator();
            Services.AddScoped(typeof(ILoggerFactory), typeof(LoggerFactory));
            Services.AddScoped(typeof(ILogger<>), typeof(Logger<>));

            Provider = Services.BuildServiceProvider();
        }
        #endregion

        #region Main Flow

        [Fact(DisplayName = "Main Flow - Success", Skip = "true")]
        public async void ShouldWireTransferAddSuccessfully()
        {
            InitializeProperties();
            ProcessConfig = new Mock<IOptions<ProcessConfig>>();
            ProcessConfig.Setup(p => p.Value).Returns(ProcessIntegrationTestsConfiguration.ReadJson<ProcessConfig>("ProcessConfig.json"));
            var transferWireResponse = GetTransferWireAddResponse();

            TransferWireOperation.Setup(x => x.TransferWireAddAsync(It.IsAny<TransferWireAddRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(transferWireResponse);
            Services.AddSingleton(TransferWireOperation.Object);
            Services.AddSingleton(ProcessConfig.Object);
            var appSettings = new Dictionary<string, string> { { "Redis:expirationTime", "60" } };

            Configuration = new ConfigurationBuilder().AddInMemoryCollection(appSettings).Build();
            Services.AddSingleton(Configuration);

            Provider = Services.BuildServiceProvider();

            var mediator = GetInstance<IMediator>();
            var transferController = new TransferController(mediator, ControllerLogger.Object);
            var request = GetWireTransferAddRequest();

            var wireTransferValidator = new WireTransferValidator();
            var responseValidator = wireTransferValidator.Validate(request);
            responseValidator.IsValid.Should().BeTrue();

            var result = await transferController.WireTransferAdd(request, CancellationToken);

            var objectResult = Assert.IsType<OkObjectResult>(result);
            var TransferMoneyResponse = Assert.IsAssignableFrom<WireTransferAddResponse>(objectResult.Value);
            Assert.NotNull(TransferMoneyResponse.TransactionId);
        }

        #endregion

        #region Alternative Flows

        [Fact(DisplayName = "AF1 – Required information not provided")]
        public void ReturnsErrorCodeForRequiredInformation()
        {
            var wireTransferValidator = new WireTransferValidator();
            var responseValidator = wireTransferValidator.Validate(new WireTransferAddRequest());
            responseValidator.IsValid.Should().BeFalse();
            responseValidator.Errors.Should().NotBeNull();
        }

        [Fact(DisplayName = "AF2 – Error in Wire transfer process", Skip = "true")]
        public async void ShouldReturnsErrorInProcess()
        {
            InitializeProperties();

            var transferWireResponse = GetTransferWireAddResponse();
            transferWireResponse.ResponseStatus = "Fail";

            TransferWireOperation.Setup(x => x.TransferWireAddAsync(It.IsAny<TransferWireAddRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(transferWireResponse);

            Services.AddSingleton(TransferWireOperation.Object);
            Provider = Services.BuildServiceProvider();

            var mediator = GetInstance<IMediator>();
            var transferController = new TransferController(mediator, ControllerLogger.Object);
            var request = GetWireTransferAddRequest();

            await Assert.ThrowsAsync<UnprocessableEntityException>(() => transferController.WireTransferAdd(request, CancellationToken));
        }

        #endregion

        #region Exception Flows

        [Fact(DisplayName = "EF1 – Failed connection with Core Banking ", Skip = "true")]
        public async void ShouldReturnsFailedConnectionError()
        {
            InitializeProperties();
            TransferWireOperation.Setup(x => x.TransferWireAddAsync(It.IsAny<TransferWireAddRequest>(), It.IsAny<CancellationToken>())).ThrowsAsync(new NotFoundException("test"));
            Services.AddSingleton(TransferWireOperation.Object);
            Provider = Services.BuildServiceProvider();
            var mediator = GetInstance<IMediator>();
            var transferController = new TransferController(mediator, ControllerLogger.Object);
            var request = GetWireTransferAddRequest();

            await Assert.ThrowsAsync<UnprocessableEntityException>(() => transferController.WireTransferAdd(request, CancellationToken));
        }

        [Fact(DisplayName = "EF2 – No connection with Core Banking", Skip = "true")]
        public async void ShouldReturnsConnectionError()
        {
            InitializeProperties();
            TransferWireOperation.Setup(x => x.TransferWireAddAsync(It.IsAny<TransferWireAddRequest>(), It.IsAny<CancellationToken>())).ThrowsAsync(new TimeoutException());
            Services.AddSingleton(TransferWireOperation.Object);
            Provider = Services.BuildServiceProvider();
            var mediator = GetInstance<IMediator>();
            var transferController = new TransferController(mediator, ControllerLogger.Object);
            var request = GetWireTransferAddRequest();

            await Assert.ThrowsAsync<UnprocessableEntityException>(() => transferController.WireTransferAdd(request, CancellationToken));
        }

        [Fact(DisplayName = "EF3 – Unexpected error", Skip = "true")]
        public async void ShouldReturnsUnexpectedError()
        {
            InitializeProperties();
            TransferWireOperation.Setup(x => x.TransferWireAddAsync(It.IsAny<TransferWireAddRequest>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());
            Services.AddSingleton(TransferWireOperation.Object);
            Provider = Services.BuildServiceProvider();
            var mediator = GetInstance<IMediator>();
            var transferController = new TransferController(mediator, ControllerLogger.Object);
            var request = GetWireTransferAddRequest();

            await Assert.ThrowsAsync<UnprocessableEntityException>(() => transferController.WireTransferAdd(request, CancellationToken));
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

        private void InitializeProperties()
        {
            CancellationToken = new CancellationToken();
            Logger = new Mock<ILogger<WireTransferCommand>>();
            TransferWireOperation = new Mock<ITransferWireOperation>();
            ControllerLogger = new Mock<ILogger<TransferController>>();
        }

        private TransferWireAddResponse GetTransferWireAddResponse()
        {
            return ProcessIntegrationTestsConfiguration.ReadJson<TransferWireAddResponse>("TransferWireAddResponse.json");
        }

        private WireTransferAddRequest GetWireTransferAddRequest()
        {
            return ProcessIntegrationTestsConfiguration.ReadJson<WireTransferAddRequest>("WireTransferAddRequest.json");
        }

        #endregion
    }
}
