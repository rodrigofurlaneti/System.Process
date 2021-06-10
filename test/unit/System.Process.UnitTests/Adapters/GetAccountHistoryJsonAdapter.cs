using System.Process.Application.Queries.GetAccountHistory;
using System.Process.Domain.Entities;
using System.Proxy.Silverlake.Inquiry.Messages.Response;
using System.Collections.Generic;

namespace System.Process.UnitTests.Adapters
{
    public class GetAccountHistoryJsonAdapter
    {
        public GetAccountHistoryRequest SuccessRequest { get; set; }
        public ProcessearchResponse AcctSrchSuccessResponse { get; set; }
        public ProcessearchResponse AcctSrchErrorResponse { get; set; }
        public ProcessearchResponse AcctSrchInvalidStatusResponse { get; set; }
        public AccountHistorySearchResponse AcctHistSuccessResponse { get; set; }
        public AccountHistorySearchResponse AcctHistErrorResponse { get; set; }
        public IList<Transaction> TransactionSuccessResponse { get; set; }
        public GetAccountHistoryResponse SuccessFinalResponse { get; set; }
    }
}
