using MediatR;
using System;

namespace System.Process.Application.Commands.SearchTransactions
{
    public class SearchTransactionsRequest : IRequest<SearchTransactionsResponse>
    {
        public string AccountId { get; set; }
        public string Pan { get; set; }
        public int MaxRows { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
