using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
using System.Process.Domain.ValueObjects;
using System.Process.UnitTests.Common;
using System.Phoenix.Common.Exceptions;
using System.Proxy.FourSight.StatementSearch;
using System.Proxy.FourSight.StatementSearch.Messages;
using System.Proxy.Salesforce.GetCustomerInformations;
using System.Proxy.Salesforce.GetCustomerInformations.Message;
using System.Proxy.Salesforce.GetToken;
using System.Proxy.Salesforce.GetToken.Messages;
using System.Proxy.Salesforce.Messages;
using System.Proxy.Silverlake.Inquiry;
using System.Proxy.Silverlake.Inquiry.Common;
using System.Proxy.Silverlake.Inquiry.Messages.Request;
using System.Proxy.Silverlake.Inquiry.Messages.Response;
using Xunit;
using AccountId = System.Proxy.Silverlake.Inquiry.Common.AccountId;

namespace System.Process.IntegrationTests.Application.Commands.RetrieveStatement
{
    public class RetrieveStatementCommandTests
    {
        #region Properties

        private static ServiceCollection Services;
        private static ServiceProvider Provider;
        private static Mock<IHttpContextAccessor> HttpContextAccessor;

        private Mock<ILogger<RetrieveStatementCommand>> Logger { get; }
        private Mock<IMediator> Mediator { get; }
        private Mock<IStatementSearch> StatementSearchOperation { get; }
        private Mock<ICustomerReadRepository> CustomerReadRepository { get; }
        private Mock<IGetCustomerInformationsClient> GetCustomerInformationsClient { get; }
        private Mock<IGetTokenClient> GetTokenClient { get; }
        private Mock<IOptions<GetTokenParams>> GetTokenParams { get; }
        private Mock<IInquiryOperation> InquiryOperation { get; set; }
        private Mock<IOptions<ProcessConfig>> ProcessConfig { get; }

        #endregion

        #region Constructor

        public RetrieveStatementCommandTests()
        {
            Logger = new Mock<ILogger<RetrieveStatementCommand>>();
            Mediator = new Mock<IMediator>();
            StatementSearchOperation = new Mock<IStatementSearch>();
            CustomerReadRepository = new Mock<ICustomerReadRepository>();
            GetCustomerInformationsClient = new Mock<IGetCustomerInformationsClient>();
            GetTokenClient = new Mock<IGetTokenClient>();
            GetTokenParams = new Mock<IOptions<GetTokenParams>>();
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
        [Fact(DisplayName = "RetrieveStatementCommand_Success")]
        public async void ShouldRetrieveStatementSuccessfully()
        {
            Mediator.Setup(x => x.Send(It.IsAny<ConsultProcessByAccountIdRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(GetConsultProcessByAccountIdResponse());
            StatementSearchOperation.Setup(x => x.SearchAsync(It.IsAny<StatementSearchParams>(), It.IsAny<CancellationToken>())).Returns(GetStatementSearchResult());
            CustomerReadRepository.Setup(x => x.FindBy(It.IsAny<string>())).Returns(GetCustomer());
            GetTokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(GetBaseTokenResult());
            GetCustomerInformationsClient.Setup(x => x.GetCustomer(It.IsAny<GetCustomerParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetBaseCustomerResult());
            InquiryOperation.Setup(x => x.ProcessearchAsync(It.IsAny<ProcessearchRequest>(), It.IsAny<CancellationToken>())).Returns(GetProcessearchResponse());

            Services.AddSingleton(Logger.Object);
            Services.AddSingleton(StatementSearchOperation.Object);
            Services.AddSingleton(CustomerReadRepository.Object);
            Services.AddSingleton(GetCustomerInformationsClient.Object);
            Services.AddSingleton(GetTokenClient.Object);
            Services.AddSingleton(GetTokenParams.Object);
            Services.AddSingleton(InquiryOperation.Object);
            Services.AddSingleton(ProcessConfig.Object);
            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<StatementsController>>();
            var mediator = GetInstance<IMediator>();
            var controller = new StatementsController(mediator, logger.Object);
            var request = GetRetrieveStatementRequest();
            request.EndDate = DateTime.MaxValue;

            var result = await controller.RetrieveStatement(request, new CancellationToken());

            Assert.IsType<OkObjectResult>(result);
        }

        [Trait("Integration", "Error")]
        [Fact(DisplayName = "RetrieveStatementCommand_Error - Should Return Start Date must be not null and earlier than or equal to End Date")]
        public async void ShouldNotRetrieveStatementStartDateTest()
        {
            Mediator.Setup(x => x.Send(It.IsAny<ConsultProcessByAccountIdRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(GetConsultProcessByAccountIdResponse());
            StatementSearchOperation.Setup(x => x.SearchAsync(It.IsAny<StatementSearchParams>(), It.IsAny<CancellationToken>())).Returns(GetStatementSearchResult());
            CustomerReadRepository.Setup(x => x.FindBy(It.IsAny<string>())).Returns(GetCustomer());
            GetTokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(GetBaseTokenResult());
            GetCustomerInformationsClient.Setup(x => x.GetCustomer(It.IsAny<GetCustomerParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetBaseCustomerResult());
            InquiryOperation.Setup(x => x.ProcessearchAsync(It.IsAny<ProcessearchRequest>(), It.IsAny<CancellationToken>())).Returns(GetProcessearchResponse());

            Services.AddSingleton(Logger.Object);
            Services.AddSingleton(StatementSearchOperation.Object);
            Services.AddSingleton(CustomerReadRepository.Object);
            Services.AddSingleton(GetCustomerInformationsClient.Object);
            Services.AddSingleton(GetTokenClient.Object);
            Services.AddSingleton(GetTokenParams.Object);
            Services.AddSingleton(InquiryOperation.Object);
            Services.AddSingleton(ProcessConfig.Object);
            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<StatementsController>>();
            var mediator = GetInstance<IMediator>();
            var controller = new StatementsController(mediator, logger.Object);
            var request = GetRetrieveStatementRequest();
            request.StartDate = DateTime.Now;

            await Assert.ThrowsAsync<UnprocessableEntityException>(() => controller.RetrieveStatement(request, new CancellationToken()));
        }

        [Trait("Integration", "Error")]
        [Fact(DisplayName = "RetrieveStatementCommand_Error - Should Return End Date must be not null and later than or equal to today")]
        public async void ShouldNotRetrieveStatementEndDateTest()
        {
            Mediator.Setup(x => x.Send(It.IsAny<ConsultProcessByAccountIdRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(GetConsultProcessByAccountIdResponse());
            StatementSearchOperation.Setup(x => x.SearchAsync(It.IsAny<StatementSearchParams>(), It.IsAny<CancellationToken>())).Returns(GetStatementSearchResult());
            CustomerReadRepository.Setup(x => x.FindBy(It.IsAny<string>())).Returns(GetCustomer());
            GetTokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(GetBaseTokenResult());
            GetCustomerInformationsClient.Setup(x => x.GetCustomer(It.IsAny<GetCustomerParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetBaseCustomerResult());
            InquiryOperation.Setup(x => x.ProcessearchAsync(It.IsAny<ProcessearchRequest>(), It.IsAny<CancellationToken>())).Returns(GetProcessearchResponse());

            Services.AddSingleton(Logger.Object);
            Services.AddSingleton(StatementSearchOperation.Object);
            Services.AddSingleton(CustomerReadRepository.Object);
            Services.AddSingleton(GetCustomerInformationsClient.Object);
            Services.AddSingleton(GetTokenClient.Object);
            Services.AddSingleton(GetTokenParams.Object);
            Services.AddSingleton(InquiryOperation.Object);
            Services.AddSingleton(ProcessConfig.Object);
            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<StatementsController>>();
            var mediator = GetInstance<IMediator>();
            var controller = new StatementsController(mediator, logger.Object);
            var request = GetRetrieveStatementRequest();
            request.EndDate = DateTime.MinValue;

            await Assert.ThrowsAsync<UnprocessableEntityException>(() => controller.RetrieveStatement(request, new CancellationToken()));
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
            return ConvertJson.ReadJson<RetrieveStatementRequest>("RetrieveStatementRequest.json");
        }

        private ConsultProcessByAccountIdResponse GetConsultProcessByAccountIdResponse()
        {
            return ConvertJson.ReadJson<ConsultProcessByAccountIdResponse>("ConsultProcessByAccountIdResponse.json");
        }

        private Task<StatementSearchResult> GetStatementSearchResult()
        {
            return Task.FromResult(ConvertJson.ReadJson<StatementSearchResult>("StatementSearchResult.json"));
        }

        private Customer GetCustomer()
        {
            return ConvertJson.ReadJson<Customer>("Customer.json");
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
                Result = new QueryResult<GetCustomerResponse>
                {

                }
            });
        }

        private async Task<ProcessearchResponse> GetProcessearchResponse()
        {
            return await Task.FromResult(new ProcessearchResponse
            {
                ProcessearchRecInfo = new List<ProcessearchRecInfo>
                {
                    new ProcessearchRecInfo
                    {
                        AccountId = new AccountId
                        {
                            AccountNumber = "string",
                            AccountType = "string"
                        },
                        Amount = 123,
                        Processtatus = "1",
                        AvailableBalance = 12,
                        ProductCode = "string",
                        ProductDesc = "string",
                        ProcesstatusDesc = "Active"
                    }
                }
            });
        }
        #endregion
    }
}
