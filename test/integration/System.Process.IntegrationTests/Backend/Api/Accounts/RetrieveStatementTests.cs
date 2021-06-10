using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Process.Api.Controllers.V1;
using System.Process.Application.Commands.RetrieveStatement;
using System.Process.Application.Queries.ConsultProcessByAccountId;
using System.Process.Base.IntegrationTests;
using System.Process.CrossCutting.DependencyInjection;
using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Proxy.FourSight.StatementSearch;
using System.Proxy.FourSight.StatementSearch.Messages;
using System.Proxy.Salesforce.GetCustomerInformations;
using System.Proxy.Salesforce.GetCustomerInformations.Message;
using System.Proxy.Salesforce.GetToken;
using System.Proxy.Salesforce.GetToken.Messages;
using System.Proxy.Salesforce.Messages;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace System.Process.IntegrationTests.Backend.Api.Process
{
    public class RetrieveStatementTests
    {
        #region Properties

        private static ServiceProvider Provider;
        private static Mock<IHttpContextAccessor> HttpContextAccessor;
        private static ServiceCollection Services;
        private Mock<ILogger<RetrieveStatementCommand>> Logger;
        private Mock<IMediator> Mediator;
        private Mock<IStatementSearch> StatementSearchOperation;
        private Mock<ICustomerReadRepository> CustomerReadRepository;
        private Mock<IGetCustomerInformationsClient> GetCustomerInformationsClient;
        private Mock<IGetTokenClient> GetTokenClient;
        private Mock<IOptions<GetTokenParams>> GetTokenParams;

        #endregion

        #region Constructor

        static RetrieveStatementTests()
        {
            Services = new ServiceCollection();
            Services.AddMediator();
            Services.AddScoped(typeof(ILoggerFactory), typeof(LoggerFactory));
            Services.AddScoped(typeof(ILogger<>), typeof(Logger<>));

            Provider = Services.BuildServiceProvider();
        }

        #endregion

        #region Main Flow

        [Fact(DisplayName = "Main Flow - Success")]
        public async void ShouldRetrieveStatementSuccessfully()
        {
            Logger = new Mock<ILogger<RetrieveStatementCommand>>();
            Mediator = new Mock<IMediator>();
            StatementSearchOperation = new Mock<IStatementSearch>();
            CustomerReadRepository = new Mock<ICustomerReadRepository>();
            GetCustomerInformationsClient = new Mock<IGetCustomerInformationsClient>();
            GetTokenClient = new Mock<IGetTokenClient>();
            GetTokenParams = new Mock<IOptions<GetTokenParams>>();

            Mediator.Setup(x => x.Send(It.IsAny<ConsultProcessByAccountIdRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(GetConsultProcessByAccountIdResponse());
            StatementSearchOperation.Setup(x => x.SearchAsync(It.IsAny<StatementSearchParams>(), It.IsAny<CancellationToken>())).Returns(GetStatementSearchResult());
            CustomerReadRepository.Setup(x => x.FindBy(It.IsAny<string>())).Returns(GetCustomer());
            GetTokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(GetBaseTokenResult());
            GetCustomerInformationsClient.Setup(x => x.GetCustomer(It.IsAny<GetCustomerParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetBaseCustomerResult());

            Services.AddSingleton(Logger.Object);
            Services.AddSingleton(Mediator.Object);
            Services.AddSingleton(StatementSearchOperation.Object);
            Services.AddSingleton(CustomerReadRepository.Object);
            Services.AddSingleton(GetCustomerInformationsClient.Object);
            Services.AddSingleton(GetTokenClient.Object);
            Services.AddSingleton(GetTokenParams.Object);
            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<StatementsController>>();
            var mediator = GetInstance<IMediator>();
            var controller = new StatementsController(mediator, logger.Object);

            var result = await controller.RetrieveStatement(GetRetrieveStatementRequest(), new CancellationToken());

            Assert.IsType<OkObjectResult>(result);
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

        private RetrieveStatementRequest GetRetrieveStatementRequest()
        {
            return ProcessIntegrationTestsConfiguration.ReadJson<RetrieveStatementRequest>("RetrieveStatementRequest.json");
        }

        private ConsultProcessByAccountIdResponse GetConsultProcessByAccountIdResponse()
        {
            return ProcessIntegrationTestsConfiguration.ReadJson<ConsultProcessByAccountIdResponse>("ConsultProcessByAccountIdResponse.json");
        }

        private Task<StatementSearchResult> GetStatementSearchResult()
        {
            return Task.FromResult(ProcessIntegrationTestsConfiguration.ReadJson<StatementSearchResult>("StatementSearchResult.json"));
        }

        private Customer GetCustomer()
        {
            return ProcessIntegrationTestsConfiguration.ReadJson<Customer>("Customer.json");
        }

        private Task<BaseResult<GetTokenResult>> GetBaseTokenResult()
        {
            return Task.FromResult(new BaseResult<GetTokenResult>
            {
                IsSuccess = true,
                Result = new GetTokenResult
                {
                    AccessToken = "C454CDB8-6370-4363-9D77-EDEE1818E867"
                }
            });
        }

        private Task<BaseResult<QueryResult<GetCustomerResponse>>> GetBaseCustomerResult()
        {
            return Task.FromResult(new BaseResult<QueryResult<GetCustomerResponse>>
            {
                IsSuccess = true,
                Result = new QueryResult<GetCustomerResponse>()
            });
        }

        #endregion
    }
}
