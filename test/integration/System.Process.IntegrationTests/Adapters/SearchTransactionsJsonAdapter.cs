using System.Process.Application.Commands.SearchTransactions;
using System.Process.Domain.Entities;
using System.Proxy.Fis.SearchTransactions.Messages;
using System.Collections.Generic;

namespace System.Process.IntegrationTests.Adapters
{
    public class SearchTransactionsJsonAdapter
    {
        public ConsultCardsByidCardRequest SuccessRequest { get; set; }
        public SearchTransactionsResult SuccessResult { get; set; }
        public IList<Card> SuccessRepositoryResponse { get; set; }
    }
}
