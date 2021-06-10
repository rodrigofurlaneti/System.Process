using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Process.Api.Controllers.V1;
using System.Process.Application.Commands.DownloadStatements;
using System.Process.CrossCutting.DependencyInjection;
using System.Process.Infrastructure.Configs;
using System.Proxy.FourSight.StatementGenerate;
using System.Proxy.FourSight.StatementGenerate.Messages;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace System.Process.IntegrationTests.Application.Commands.DownloadStatements
{
    public class DownloadStatementsCommandTests
    {
        #region Properties

        private static ServiceCollection Services;
        private static ServiceProvider Provider;
        private static Mock<IHttpContextAccessor> HttpContextAccessor;

        private Mock<ILogger<DownloadStatementsCommand>> Logger { get; }
        private Mock<IStatementGenerate> StatementGenerate { get; }
        private Mock<IOptions<GenerateStatementsConfig>> Config { get; }

        #endregion

        #region Constructor

        public DownloadStatementsCommandTests()
        {
            Logger = new Mock<ILogger<DownloadStatementsCommand>>();
            StatementGenerate = new Mock<IStatementGenerate>();
            Config = new Mock<IOptions<GenerateStatementsConfig>>();

            Services = new ServiceCollection();
            Services.AddMediator();
            Services.AddScoped(typeof(ILoggerFactory), typeof(LoggerFactory));
            Services.AddScoped(typeof(ILogger<>), typeof(Logger<>));

            Provider = Services.BuildServiceProvider();
        }

        #endregion

        #region Tests

        [Trait("Integration", "Success")]
        [Fact(DisplayName = "DownloadStatementsCommand_Success")]
        public async Task ShouldDownloadStatementsSuccessfully()
        {
            StatementGenerate
                .Setup(t => t.Generate(It.IsAny<StatementGenerateParams>(), It.IsAny<CancellationToken>())).Returns(GetStatementGenerateResult());
            Config
                .Setup(x => x.Value)
                .Returns(new GenerateStatementsConfig
                {
                    MaxBytes = 20000
                });

            Services.AddSingleton(Logger.Object);
            Services.AddSingleton(StatementGenerate.Object);
            Services.AddSingleton(Config.Object);
            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<StatementsController>>();
            var mediator = GetInstance<IMediator>();
            var controller = new StatementsController(mediator, logger.Object);
            var request = "15332163";

            var result = await controller.DownloadStatements(request, new CancellationToken());

            Assert.IsType<CreatedResult>(result);

        }

        [Trait("Integration", "Error")]
        [Fact(DisplayName = "DownloadStatementsCommand_Error")]
        public async Task ShouldNotDownloadStatements()
        {
            StatementGenerate
                 .Setup(t => t.Generate(It.IsAny<StatementGenerateParams>(), It.IsAny<CancellationToken>())).Returns(GetStatementGenerateResultError());
            Config
                .Setup(x => x.Value).Returns(new GenerateStatementsConfig
                {
                    MaxBytes = 20000
                });
            Services.AddSingleton(Logger.Object);
            Services.AddSingleton(StatementGenerate.Object);
            Services.AddSingleton(Config.Object);
            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<StatementsController>>();
            var mediator = GetInstance<IMediator>();
            var controller = new StatementsController(mediator, logger.Object);
            var request = string.Empty;

            await Assert.ThrowsAsync<ArgumentException>(() => controller.DownloadStatements(request, new CancellationToken()));
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

        private Task<StatementGenerateResult> GetStatementGenerateResult()
        {
            return Task.FromResult(new StatementGenerateResult
            {
                StatementId = "124314",
                DocImage = new List<byte>().ToArray(),
            });
        }

        private Task<StatementGenerateResult> GetStatementGenerateResultError()
        {
            return Task.FromResult(new StatementGenerateResult());
        }

        #endregion
    }
}
