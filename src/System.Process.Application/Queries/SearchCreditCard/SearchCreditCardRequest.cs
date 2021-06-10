using MediatR;

namespace System.Process.Application.Queries.SearchCreditCard
{
    public class SearchCreditCardRequest : IRequest<SearchCreditCardResponse>
    {
        public string SystemId { get; set; }
    }
}
