using System.Process.Application.DataTransferObjects;
using System.Process.Domain.Constants;
using System.Proxy.FourSight.StatementSearch.Messages;
using System.Collections.Generic;

namespace System.Process.Application.Commands.RetrieveStatement.Adapters
{
    public class RetrieveStatementAdapter
    {
        public StatementSearchParams AdaptParams(ProcessearchRecordsDto accountInfoInput,
                                                RetrieveStatementRequest retrieveStatementInput)
        {
            return new StatementSearchParams
            {
                AccountId = new AccountId
                {
                    AccountNumber = accountInfoInput?.AccountNumber,
                    AccountType = accountInfoInput?.AccountType
                },
                CustomerId = accountInfoInput?.CustomerId,
                EndDate = retrieveStatementInput.EndDate,
                StartDate = retrieveStatementInput.StartDate,
                MaximumRecords = Constants.MaximumRecords
            };
        }

        public List<Response.Statement> AdaptResult(StatementSearchResult input)
        {
            var statementsList = new List<Response.Statement>();
            foreach (var retriviedStatement in input.Statements)
            {
                var statement = new Response.Statement
                {
                    StatementDate = retriviedStatement?.StatementDate,
                    StatementId = retriviedStatement?.StatementId
                };
                statementsList.Add(statement);
            }

            return statementsList;
        }

    }
}
