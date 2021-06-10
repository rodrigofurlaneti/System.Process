using System.Process.Application.DataTransferObjects;
using System.Collections.Generic;

namespace System.Process.Application.Queries.FindReceivers
{
    public class FindReceiversResponse
    {
        public IList<ReceiverDto> ReceiverList { get; set; }
    }
}
