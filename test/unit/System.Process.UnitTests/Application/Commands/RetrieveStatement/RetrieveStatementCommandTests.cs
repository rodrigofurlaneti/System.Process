using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Process.Application.Commands.RetrieveStatement;
using System.Process.Application.Queries.ConsultProcessByAccountId;
using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Process.UnitTests.Common;
using System.Phoenix.Common.Exceptions;
using System.Proxy.FourSight.StatementSearch;
using System.Proxy.FourSight.StatementSearch.Messages;
using System.Proxy.Salesforce.GetCustomerInformations;
using System.Proxy.Salesforce.GetCustomerInformations.Message;
using System.Proxy.Salesforce.GetToken;
using System.Proxy.Salesforce.GetToken.Messages;
using System.Proxy.Salesforce.Messages;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace System.Process.UnitTests.Application.Commands.RetrieveStatement
{
    public class RetrieveStatementCommandTests
    {
        #region Properties

        private Mock<ILogger<RetrieveStatementCommand>> Logger { get; }
        private Mock<IMediator> Mediator { get; }
        private Mock<IStatementSearch> StatementSearchOperation { get; }
        private Mock<ICustomerReadRepository> CustomerReadRepository { get; }
        private Mock<IGetCustomerInformationsClient> GetCustomerInformationsClient { get; }
        private Mock<IGetTokenClient> GetTokenClient { get; }
        private Mock<IOptions<GetTokenParams>> GetTokenParams { get; }

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
        }

        #endregion

        #region Tests

        [Fact(DisplayName = "Should Send Handle Retrieve Statement")]
        public async Task ShouldSendHandleRetrieveStatementAsync()
        {
            Mediator.Setup(x => x.Send(It.IsAny<ConsultProcessByAccountIdRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(GetConsultProcessByAccountIdResponse());
            StatementSearchOperation.Setup(x => x.SearchAsync(It.IsAny<StatementSearchParams>(), It.IsAny<CancellationToken>())).Returns(GetStatementSearchResult());
            CustomerReadRepository.Setup(x => x.FindBy(It.IsAny<string>())).Returns(GetCustomer());
            GetTokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(GetBaseTokenResult());
            GetCustomerInformationsClient.Setup(x => x.GetCustomer(It.IsAny<GetCustomerParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetBaseCustomerResult());

            var retrieveStatementCommand = new RetrieveStatementCommand(
                Logger.Object,
                Mediator.Object,
                StatementSearchOperation.Object,
                CustomerReadRepository.Object,
                GetCustomerInformationsClient.Object,
                GetTokenClient.Object,
                GetTokenParams.Object
              );

            var cancellationToken = new CancellationToken(false);

            var request = GetRetrieveStatementRequest();
            request.EndDate = DateTime.MaxValue;

            var result = await retrieveStatementCommand.Handle(request, cancellationToken);

            Assert.IsType<RetrieveStatementResponse>(result);
            Assert.NotNull(result);
        }

        [Fact(DisplayName = "Should Return Start Date must be not null and earlier than or equal to End Date")]
        public async Task ShouldSendUnprocessableEntityException()
        {
            Mediator.Setup(x => x.Send(It.IsAny<ConsultProcessByAccountIdRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(GetConsultProcessByAccountIdResponse());
            StatementSearchOperation.Setup(x => x.SearchAsync(It.IsAny<StatementSearchParams>(), It.IsAny<CancellationToken>())).Returns(GetStatementSearchResult());

            var retrieveStatementCommand = new RetrieveStatementCommand(
                Logger.Object,
                Mediator.Object,
                StatementSearchOperation.Object,
                CustomerReadRepository.Object,
                GetCustomerInformationsClient.Object,
                GetTokenClient.Object,
                GetTokenParams.Object
              );

            var cancellationToken = new CancellationToken(false);

            var request = GetRetrieveStatementRequest();
            request.StartDate = DateTime.Now;

            await Assert.ThrowsAsync<UnprocessableEntityException>(() => retrieveStatementCommand.Handle(request, cancellationToken));
        }

        [Fact(DisplayName = "Should Return End Date must be not null and later than or equal to today")]
        public async Task ShouldSendException()
        {
            Mediator.Setup(x => x.Send(It.IsAny<ConsultProcessByAccountIdRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(GetConsultProcessByAccountIdResponse());
            StatementSearchOperation.Setup(x => x.SearchAsync(It.IsAny<StatementSearchParams>(), It.IsAny<CancellationToken>())).Returns(GetStatementSearchResult());

            var retrieveStatementCommand = new RetrieveStatementCommand(
                Logger.Object,
                Mediator.Object,
                StatementSearchOperation.Object,
                CustomerReadRepository.Object,
                GetCustomerInformationsClient.Object,
                GetTokenClient.Object,
                GetTokenParams.Object
              );

            var cancellationToken = new CancellationToken(false);

            var request = GetRetrieveStatementRequest();
            request.EndDate = DateTime.MinValue;

            await Assert.ThrowsAsync<UnprocessableEntityException>(() => retrieveStatementCommand.Handle(request, cancellationToken));
        }

        #endregion

        #region Methods

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

        #endregion
    }
}
