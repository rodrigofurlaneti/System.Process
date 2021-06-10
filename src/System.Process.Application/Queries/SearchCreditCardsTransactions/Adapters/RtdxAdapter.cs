using System.Proxy.Rtdx.PendingActivityDetails.Messages;

namespace System.Process.Application.Queries.SearchCreditCardsTransactions.Adapters
{
    public class RtdxAdapter
    {
        public PendingActivityDetailsParams Adapt(string accountNumber, string token)
        {
            return new PendingActivityDetailsParams
            {
                AccountNumber = accountNumber,
                SecurityToken = token,
            };
        }
    }
}
