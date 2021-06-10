using System.Collections.Generic;
using System.Process.Application.Queries.SearchCreditCardsTransactions.Response;

namespace System.Process.Application.Queries.SearchCreditCardsTransactions
{
    public class SearchCreditCardsTransactionsResponse
    {
        public int CardId { get; set; }
        public IList<TransactionItem> PendingTransactions { get; set; }
        public IList<TransactionItem> PostedTransactions { get; set; }
    }
}
