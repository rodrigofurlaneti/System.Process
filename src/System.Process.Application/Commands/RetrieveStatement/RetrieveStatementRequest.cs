using MediatR;
using System;

namespace System.Process.Application.Commands.RetrieveStatement
{
    public class RetrieveStatementRequest : IRequest<RetrieveStatementResponse>
    {
        public string ApplicationId { get; set; }
        public string AccountNumber { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
