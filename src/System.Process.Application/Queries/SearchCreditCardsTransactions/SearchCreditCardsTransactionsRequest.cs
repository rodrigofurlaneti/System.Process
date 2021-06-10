using MediatR;

namespace System.Process.Application.Queries.SearchCreditCardsTransactions
{
    public class SearchCreditCardsTransactionsRequest : IRequest<SearchCreditCardsTransactionsResponse>
    {
        public string CardId { get; set; }
    }
}
