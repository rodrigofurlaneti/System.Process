using System.Process.Application.DataTransferObjects;
using System.Collections.Generic;

namespace System.Process.Application.Queries.ConsultCardsByCustomerId
{
    public class ConsultCardsByCustomerIdResponse
    {
        public IList<CustomerCardDto> CardRecords { get; set; }
    }
}
