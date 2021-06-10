using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Process.Api.Controllers.V1;
using System.Process.Application.Clients.Cards;
using System.Process.Application.Commands.ChangeCardStatus;
using System.Process.Base.IntegrationTests;
using System.Process.CrossCutting.DependencyInjection;
using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Process.Domain.ValueObjects;
using System.Process.IntegrationTests.Adapters;
using System.Process.IntegrationTests.Common;
using System.Phoenix.Web.Filters;
using System.Proxy.Fis.ChangeCardStatus;
using System.Proxy.Fis.ChangeCardStatus.Messages;
using System.Proxy.Fis.GetToken;
using System.Proxy.Fis.GetToken.Messages;
using System.Proxy.Fis.LockUnlock;
using System.Proxy.Fis.LockUnlock.Messages;
using System.Proxy.Fis.Messages;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace System.Process.IntegrationTests.Backend.Api.Process
{
    public class ChangeCardStatusTests
    {
        #region Properties

        private static ServiceProvider Provider;
        private static Mock<IHttpContextAccessor> HttpContextAccessor;
        private static ServiceCollection Services;

        #endregion

        #region Constructor
        static ChangeCardStatusTests()
        {
            //var appSettings = new Dictionary<string, string>
            //{
            //    {"MongoDB:Transaction:Database", "test"},
            //    {"MongoDB:Transaction:Collection", "test"},
            //    {"MongoDB:Transaction:ConnectionString", "test"}
            //};

            //var configuration = new ConfigurationBuilder()
            //    .AddInMemoryCollection(appSettings)
            //    .Build();

            Services = new ServiceCollection();
            Services.AddMvc(options => options.Filters.Add(new ValidationFilterAttribute()))
               .AddFluentValidation(options =>
               {
                   options.RegisterValidatorsFromAssemblyContaining<ChangeCardStatusValidator>();
               });
            Services.AddMediator();
            Services.AddScoped(typeof(ILoggerFactory), typeof(LoggerFactory));
            Services.AddScoped(typeof(ILogger<>), typeof(Logger<>));

            Provider = Services.BuildServiceProvider();
        }

        #endregion

        #region Tests

        [Fact(DisplayName = "Main Flow - Success")]
        public async void ShouldChangeCardStatusSuccessfully()
        {
            var adapter = GetChangeCardStatus();
            var cancellationToken = new CancellationToken();

            var operation = new Mock<ILockUnlockClient>();
            var repo = new Mock<ICardReadRepository>();
            var service = new Mock<ICardService>();
            var token = new Mock<IGetTokenClient>();
            var tokenParams = new Mock<IOptions<GetTokenParams>>();
            var ProcessConfig = new Mock<IOptions<ProcessConfig>>();

            var response = new BaseResult<ChangeCardStatusResult>
            {
                IsSuccess = true,
                Result = adapter.SuccessResult
            };

            token.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(GetBaseTokenResult()));
            operation.Setup(x => x.LockUnlockAsync(It.IsAny<LockUnlockParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetLockUnlockResult());
            repo.Setup(x => x.Find(It.IsAny<int>())).Returns(adapter.SuccessCard);
            service.Setup(x => x.HandleCardUpdate(It.IsAny<Card>(), cancellationToken));
            ProcessConfig.Setup(p => p.Value).Returns(ProcessIntegrationTestsConfiguration.ReadJson<ProcessConfig>("ProcessConfig.json"));

            Services.AddSingleton(operation.Object);
            Services.AddSingleton(repo.Object);
            Services.AddSingleton(service.Object);
            Services.AddSingleton(token.Object);
            Services.AddSingleton(tokenParams.Object);
            Services.AddSingleton(ProcessConfig.Object);

            Provider = Services.BuildServiceProvider();

            var mediator = GetInstance<IMediator>();
            var logger = new Mock<ILogger<ProcessController>>();

            var accountController = new ProcessController(mediator, logger.Object);

            var validate = new ChangeCardStatusValidator();
            validate.Validate(adapter.SuccessRequest);

            var result = await accountController.ChangeCardStatus(adapter.SuccessRequest, cancellationToken);

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region Methods


        private Task<BaseResult<LockUnlockResult>> GetLockUnlockResult()
        {
            return Task.FromResult(new BaseResult<LockUnlockResult>
            {
                IsSuccess = true,
                Result = ProcessIntegrationTestsConfiguration.ReadJson<LockUnlockResult>("LockUnlockResult.json")
            });
        }

        private ChangeCardStatusJsonAdapter GetChangeCardStatus()
        {
            return ConvertJson.ReadJson<ChangeCardStatusJsonAdapter>("ChangeCardStatus.json");
        }

        private BaseResult<GetTokenResult> GetBaseTokenResult()
        {
            return new BaseResult<GetTokenResult>
            {
                IsSuccess = true,
                Result = new GetTokenResult
                {
                    AccessToken = "D923GDM9-2943-9385-6B98-HFJC8742X901"
                }
            };
        }

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
