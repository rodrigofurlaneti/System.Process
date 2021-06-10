using System.Process.Application.DataTransferObjects;
using System.Collections.Generic;

namespace System.Process.Application.Commands.ValidateAccount
{
    public class ValidateAccountResponse
    {
        public bool Valid { get; set; }
        public decimal? TotalProcessBalance { get; set; }
        public IList<ProcessearchRecordsDto> ProcessearchRecords { get; set; }

    }
}