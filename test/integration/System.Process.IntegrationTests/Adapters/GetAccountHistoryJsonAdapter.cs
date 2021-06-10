using System.Process.Application.Queries.GetAccountHistory;
using System.Process.Domain.Entities;
using System.Proxy.Silverlake.Inquiry.Messages.Response;
using System.Collections.Generic;

namespace System.Process.IntegrationTests.Adapters
{
    public class GetAccountHistoryJsonAdapter
    {
        public GetAccountHistoryRequest SuccessRequest { get; set; }
        public GetAccountHistoryRequest ErrorRequest { get; set; }
        public GetAccountHistoryRequest ErrorRequestStartDate { get; set; }
        public GetAccountHistoryRequest ErrorRequestEndDate { get; set; }
        public ProcessearchResponse AcctSrchSuccessResponse { get; set; }
        public ProcessearchResponse AcctSrchErrorResponse { get; set; }
        public ProcessearchResponse AcctSrchInvalidStatusResponse { get; set; }
        public AccountHistorySearchResponse AcctHistSuccessResponse { get; set; }
        public IList<Transaction> TransactionSuccessResponse { get; set; }
    }
}
