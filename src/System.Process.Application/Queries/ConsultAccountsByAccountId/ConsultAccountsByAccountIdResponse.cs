using System.Process.Application.DataTransferObjects;
using System.Collections.Generic;

namespace System.Process.Application.Queries.ConsultProcessByAccountId
{
    public class ConsultProcessByAccountIdResponse
    {
        public decimal? TotalProcessBalance { get; set; }
        public IList<ProcessearchRecordsDto> ProcessearchRecords { get; set; }
    }
}
