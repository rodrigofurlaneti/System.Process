using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Process.Domain.Repositories;
using System.Process.Domain.ValueObjects;
using System.Process.Infrastructure.Configs;
using System.Phoenix.Common.Exceptions;
using System.Proxy.Salesforce.GetRecordType;
using System.Proxy.Salesforce.GetRecordType.Message;
using System.Proxy.Salesforce.GetToken;
using System.Proxy.Salesforce.GetToken.Messages;
using System.Proxy.Salesforce.Messages;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace System.Process.Application.Commands.StatementTypes
{
    public class StatementTypesCommand : IRequestHandler<StatementTypesRequest, StatementTypesResponse>
    {
        #region Properties

        private IStatementReadRepository StatementReadRepository { get; }
        private IGetRecordTypeClient RecordTypeClient { get; }
        private GetTokenParams Config { get; }
        private IGetTokenClient TokenClient { get; }
        private RecordTypesConfig RecordTypesConfig { get; }
        private ILogger<StatementTypesCommand> Logger { get; }
        private ProcessConfig ProcessConfig { get; }

        #endregion

        #region Constructor

        public StatementTypesCommand(
            IStatementReadRepository statementReadRepository,
            IGetRecordTypeClient recordTypeClient,
            IOptions<GetTokenParams> config,
            IGetTokenClient tokenClient,
            IOptions<RecordTypesConfig> recordTypesConfig,
            ILogger<StatementTypesCommand> logger,
            IOptions<ProcessConfig> ProcessConfig)
        {
            StatementReadRepository = statementReadRepository;
            RecordTypeClient = recordTypeClient;
            Config = config.Value;
            TokenClient = tokenClient;
            RecordTypesConfig = recordTypesConfig.Value;
            Logger = logger;
            ProcessConfig = ProcessConfig.Value;
        }

        #endregion

        #region INotificationHandler implementation

        public async Task<StatementTypesResponse> Handle(StatementTypesRequest request, CancellationToken cancellationToken)
        {
            try
            {
                bool[] parameters = await GetParameters(request.SalesforceId, cancellationToken);

                var active = true;

                var search = StatementReadRepository.FindBy(active, parameters[0], parameters[1]);

                var adapter = new StatementTypesAdapter();

                var result = adapter.Adapt(search);

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                throw new UnprocessableEntityException(ex.Message);
            }
        }

        #endregion

        #region Methods

        private async Task<bool[]> GetParameters(string salesforceId, CancellationToken cancellationToken)
        {
            try
            {
                bool[] result = new bool[2] { false, false };
                var records = await GetRecordsType(salesforceId, cancellationToken);
                if (records == null)
                {
                    throw new UnprocessableEntityException("Cannot get data from Salesforce", "Incorrect Salesforce Id");
                }

                if (records.Result.Records.FirstOrDefault().Assets.Records.Where(x => x.BankAccountType == ProcessConfig.SafraBankingAccountType).Count() > 0)
                {
                    result[0] = true;
                }

                var recordType = records.Result.Records.FirstOrDefault().RecordTypeId;
                if (recordType == RecordTypesConfig.AccountMerchant)
                {
                    result[0] = true;
                }

                return result;
            }
            catch
            {
                throw;
            }
        }

        private async Task<BaseResult<QueryResult<GetRecordTypeResponse>>> GetRecordsType(string id, CancellationToken cancellationToken)
        {
            try
            {
                var param = new GetRecordTypeParams { SystemId = id };
                var authToken = await TokenClient.GetToken(Config, cancellationToken);
                var result = await RecordTypeClient.GetRecordType(param, authToken.Result.AccessToken, cancellationToken);

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                throw new UnprocessableEntityException(ex.Message);
            }
        }

        #endregion
    }
}
