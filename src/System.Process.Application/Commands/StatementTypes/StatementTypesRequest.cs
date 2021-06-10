using MediatR;

namespace System.Process.Application.Commands.StatementTypes
{
    public class StatementTypesRequest : IRequest<StatementTypesResponse>
    {
        public string SalesforceId { get; set; }
    }
}
