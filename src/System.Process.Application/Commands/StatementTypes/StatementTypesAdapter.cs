using System.Process.Application.Commands.StatementTypes.Response;
using System.Process.Domain.Entities;
using System.Process.Infrastructure.Adapters;
using System.Collections.Generic;

namespace System.Process.Application.Commands.StatementTypes
{
    public class StatementTypesAdapter : IAdapter<StatementTypesResponse, List<Statement>>
    {
        public StatementTypesResponse Adapt(List<Statement> input)
        {
            if (input == null || input.Count == 0)
            {
                return null;
            }

            var statementsReponse = new StatementTypesResponse();
            statementsReponse.Statements = new List<StatementType>();

            foreach (var statement in input)
            {
                statementsReponse.Statements.Add(new StatementType
                {
                    Description = statement.Description,
                    Id = statement.Id,
                    Source = statement.Source
                });
            }

            return statementsReponse;
        }
    }
}
