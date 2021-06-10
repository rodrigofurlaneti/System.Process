using MediatR;

namespace System.Process.Application.Queries.ConsultProcessByAccountId
{
    public class ConsultProcessByAccountIdRequest : IRequest<ConsultProcessByAccountIdResponse>
    {
        public string AccountId { get; set; }
        public string AccountType { get; set; }

        public ConsultProcessByAccountIdRequest(string accountId)
        {
            AccountId = accountId;
        }
    }
}
