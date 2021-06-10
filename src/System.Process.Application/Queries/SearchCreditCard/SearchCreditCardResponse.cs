using System.Collections.Generic;
using System.Process.Application.Queries.SearchCreditCard.Response;

namespace System.Process.Application.Queries.SearchCreditCard
{
    public class SearchCreditCardResponse
    {
        public IList<CreditCard> Cards { get; set; }
        public bool HasCard { get; set; }
    }
}
