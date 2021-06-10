using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Process.Application.Commands.RetrieveStatement.Adapters;
using System.Process.Application.Commands.RetrieveStatement.Response;
using System.Process.Application.Queries.ConsultProcessByAccountId;
using System.Process.Domain.Repositories;
using System.Phoenix.Common.Exceptions;
using System.Proxy.FourSight.StatementSearch;
using System.Proxy.Salesforce.GetCustomerInformations;
using System.Proxy.Salesforce.GetToken;
using System.Proxy.Salesforce.GetToken.Messages;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace System.Process.Application.Commands.RetrieveStatement
{
    public class RetrieveStatementCommand : IRequestHandler<RetrieveStatementRequest, RetrieveStatementResponse>
    {
        #region Properties
        private ILogger<RetrieveStatementCommand> Logger { get; }
        private IMediator Mediator { get; }
        private IStatementSearch StatementSearchOperation { get; }
        private ICustomerReadRepository CustomerReadRepository { get; }
        private IGetCustomerInformationsClient GetCustomerInformationsClient { get; }
        private IGetTokenClient GetTokenClient { get; }
        private GetTokenParams GetTokenParams { get; }

        #endregion 

        #region Constructor

        public RetrieveStatementCommand(
            ILogger<RetrieveStatementCommand> logger,
            IMediator mediator,
            IStatementSearch statementSearchOperation,
            ICustomerReadRepository customerReadRepository,
            IGetCustomerInformationsClient getCustomerInformationsClient,
            IGetTokenClient getTokenClient,
            IOptions<GetTokenParams> getTokenParams)
        {
            Logger = logger;
            Mediator = mediator;
            StatementSearchOperation = statementSearchOperation;
            CustomerReadRepository = customerReadRepository;
            GetCustomerInformationsClient = getCustomerInformationsClient;
            GetTokenClient = getTokenClient;
            GetTokenParams = getTokenParams.Value;
        }

        #endregion

        #region IRequestHandler implementation

        public async Task<RetrieveStatementResponse> Handle(RetrieveStatementRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var today = DateTime.UtcNow;

                if (request.StartDate == null || request.StartDate > request.EndDate)
                {
                    Logger.LogError("Start Date must be not null and earlier than or equal to End Date");
                    throw new UnprocessableEntityException("Start Date must be not null and earlier than or equal to End Date");
                }

                if (request.EndDate == null || request.EndDate.Date < today.Date)
                {
                    Logger.LogError("End Date must be not null and later than or equal to today");
                    throw new UnprocessableEntityException("End Date must be not null and later than or equal to today");
                }

                long accountId;

                if (request.AccountNumber == null || request.AccountNumber == string.Empty || !Int64.TryParse(request.AccountNumber, out accountId))
                {
                    Logger.LogError("Account Number must be not null and only numeric");
                    throw new UnprocessableEntityException("Account Number must be not null and only numeric");
                }

                var adapter = new RetrieveStatementAdapter();

                var consultProcessResponse = await Mediator.Send(new ConsultProcessByAccountIdRequest(request.AccountNumber), cancellationToken);

                if (consultProcessResponse.ProcessearchRecords == null)
                {
                    Logger.LogError($"Account not Found for request: {request}");
                    throw new UnprocessableEntityException($"Account not Found for request: {request}");
                }

                var response = new RetrieveStatementResponse();
                response.Statements = new List<Statement>();

                foreach (var accountInfo in consultProcessResponse.ProcessearchRecords)
                {
                    var paramsStatementSearch = adapter.AdaptParams(accountInfo, request);

                    var foursightResult = await StatementSearchOperation.SearchAsync(paramsStatementSearch, cancellationToken);
                    response.Statements.AddRange(adapter.AdaptResult(foursightResult));
                }

                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error on Retrieve statements");
                throw new UnprocessableEntityException("Error on Retrieve statements", ex);
            }
        }

        #endregion
    }
}
