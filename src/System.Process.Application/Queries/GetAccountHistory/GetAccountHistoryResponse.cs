using System.Process.Application.DataTransferObjects;
using System.Collections.Generic;

namespace System.Process.Application.Queries.GetAccountHistory
{
    public class GetAccountHistoryResponse
    {
        public IList<TransactionHistoryDto> TransactionHistory { get; set; }
    }
}