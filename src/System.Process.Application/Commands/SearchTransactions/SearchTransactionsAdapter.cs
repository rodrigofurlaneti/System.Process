using System.Process.Infrastructure.Adapters;
using System.Proxy.Fis.SearchTransactions.Messages;
using System.Proxy.Fis.ValueObjects;
using System;

namespace System.Process.Application.Commands.SearchTransactions
{
    public class SearchTransactionsAdapter : IAdapter<SearchTransactionsParams, SearchTransactionsRequest>
    {
        public SearchTransactionsParams Adapt(SearchTransactionsRequest input)
        {
            return new SearchTransactionsParams
            {
                Pan = new Pan
                {
                    Alias = "",
                    PlainText = input.Pan,
                    CipherText = ""
                },
                MaxRows = input.MaxRows,
                StartDate = DateTime.Now.AddDays(-30).ToString(),
                EndDate = DateTime.Now.ToString()
            };
        }
    }
}