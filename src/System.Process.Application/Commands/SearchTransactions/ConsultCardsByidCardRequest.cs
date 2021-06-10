using MediatR;

namespace System.Process.Application.Commands.SearchTransactions
{
    public class ConsultCardsByidCardRequest : IRequest<SearchTransactionsResponse>
    {
        public int CardId { get; set; }
    }
}
