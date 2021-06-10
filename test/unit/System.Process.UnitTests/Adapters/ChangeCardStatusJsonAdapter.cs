using System.Process.Application.Commands.ChangeCardStatus;
using System.Process.Domain.Entities;
using System.Proxy.Fis.ChangeCardStatus.Messages;
using System.Collections.Generic;

namespace System.Process.UnitTests.Adapters
{
    public class ChangeCardStatusJsonAdapter
    {
        public ChangeCardStatusRequest SuccessRequest { get; set; }
        public ChangeCardStatusResult SuccessResult { get; set; }
        public IList<Card> SuccessCard { get; set; }
    }
}
