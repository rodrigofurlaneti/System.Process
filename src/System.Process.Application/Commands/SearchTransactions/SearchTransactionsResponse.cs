using System.Proxy.Fis.SearchTransactions.Messages.Result;
using System.Collections.Generic;

namespace System.Process.Application.Commands.SearchTransactions
{
    public class SearchTransactionsResponse
    {
        public IList<Transactions> Transactions { get; set; }
        public MetadataSearchTransactions Metadata { get; set; }

    }
}
