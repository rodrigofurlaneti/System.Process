using System.Process.Application.DataTransferObjects;
using System.Collections.Generic;

namespace System.Process.Application.Queries.ConsultProcessByCustomerId
{
    public class ConsultProcessByCustomerIdResponse
    {
        public decimal? TotalProcessBalance { get; set; }
        public IList<CustomerSearchRecordsDto> ProcessearchRecords { get; set; }
    }
}
