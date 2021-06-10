using System.Process.Application.Commands.ReissueCard;
using System.Process.Domain.Entities;
using System.Proxy.Fis.ReissueCard.Messages;
using System.Collections.Generic;

namespace System.Process.UnitTests.Adapters
{
    public class ReissueCardJsonAdapter
    {
        public ReissueCardRequest SuccessRequest { get; set; }
        public ReissueCardResult SuccessResult { get; set; }
        public IList<Card> SuccessRepositoryResponse { get; set; }
    }
}
