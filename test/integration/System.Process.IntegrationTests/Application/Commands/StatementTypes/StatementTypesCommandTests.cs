using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Process.Application.Commands.StatementTypes;
using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Process.Domain.ValueObjects;
using System.Process.Infrastructure.Configs;
using System.Process.UnitTests.Common;
using System.Proxy.Salesforce.GetRecordType;
using System.Proxy.Salesforce.GetRecordType.Message;
using System.Proxy.Salesforce.GetToken;
using System.Proxy.Salesforce.GetToken.Messages;
using System.Proxy.Salesforce.Messages;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace System.Process.IntegrationTests.Application.Commands.StatementTypes
{
    public class StatementTypesCommandTests
    {
        [Fact(DisplayName = "Should Send Handle StatementTypes")]
        public async Task ShouldSendHandleStatementsTypes()
        {
            var statementReadRepository = new Mock<IStatementReadRepository>();
            var recordTypeClient = new Mock<IGetRecordTypeClient>();
            var getTokenParams = new Mock<IOptions<GetTokenParams>>();
            var tokenClient = new Mock<IGetTokenClient>();
            var logger = new Mock<ILogger<StatementTypesCommand>>();
            var config = new ProcessConfig 
            {
                SafraBankingAccountType = "test"
            };
            var ProcessConfig = Options.Create(config);
            var recordConfig = new RecordTypesConfig
            {
                AccountMerchant = "0122g000000ClgNAAS"
            };
            var recordTypesConfig = Options.Create(recordConfig);
            var types = ConvertJson.ReadJson<List<Statement>>("StatementTypes.json");

            statementReadRepository
                .Setup(r => r.FindBy(It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(types);

            tokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(GetTokenBaseResult());

            recordTypeClient
                .Setup(x => x.GetRecordType(It.IsAny<GetRecordTypeParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(GetRecordTypesResult());

            var statementsTypesCommand = new StatementTypesCommand(
                statementReadRepository.Object,
                recordTypeClient.Object,
                getTokenParams.Object,
                tokenClient.Object,
                recordTypesConfig,
                logger.Object,
                ProcessConfig);

            var request = new StatementTypesRequest();
            var cancellationToken = new CancellationToken();

            var result = await statementsTypesCommand.Handle(request, cancellationToken);

            result.Statements.Count.Should().BeGreaterThan(0);
        }

        private Task<BaseResult<GetTokenResult>> GetTokenBaseResult()
        {
            return Task.FromResult(new BaseResult<GetTokenResult>
            {
                IsSuccess = true,
                Result = new GetTokenResult
                {
                    AccessToken = "test"
                }
            });
        }

        private Task<BaseResult<QueryResult<GetRecordTypeResponse>>> GetRecordTypesResult()
        {
            return Task.FromResult(ConvertJson.ReadJson<BaseResult<QueryResult<GetRecordTypeResponse>>>("GetRecordTypeResult.json"));
        }
    }
}
