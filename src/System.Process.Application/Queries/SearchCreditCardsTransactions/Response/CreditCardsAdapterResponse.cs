using System.Collections.Generic;
using System.Proxy.Rtdx.PendingActivityDetails.Messages.Results;
using System.Proxy.Rtdx.TransactionDetails.Messages.Result;

namespace System.Process.Application.Queries.SearchCreditCardsTransactions.Response
{
    public class CreditCardsAdapterResponse
    {
        public int CardId { get; set; }
        public IList<StatementLines> StatementLines { get; set; }
        public IList<AuthorizationItems> AuthorizationItems { get; set; }

        public CreditCardsAdapterResponse(IList<StatementLines> statementLines, IList<AuthorizationItems> authorizationItems, int cardId)
        {
            StatementLines = statementLines;
            AuthorizationItems = authorizationItems;
            CardId = cardId;
        }
    }


}
