using MediatR;

namespace System.Process.Application.Commands.DownloadStatements
{
    public class DownloadStatementsRequest : IRequest<DownloadStatementsResponse>
    {
        public string StatementId { get; set; }
    }
}
