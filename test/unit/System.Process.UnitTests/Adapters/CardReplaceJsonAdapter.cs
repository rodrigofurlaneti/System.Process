using System.Process.Application.Commands.CardReplace;
using System.Process.Domain.Entities;
using System.Proxy.Fis.CardReplace.Messages;
using System.Collections.Generic;

namespace System.Process.UnitTests.Adapters
{
    public class CardReplaceJsonAdapter
    {
        public CardReplaceRequest SuccessRequest { get; set; }
        public CardReplaceResult SuccessResult { get; set; }
        public IList<Card> SuccessRepositoryResponse { get; set; }
    }
}
