using System.Process.Application.Commands.TransactionDetail;
using System.Proxy.Fis.TransactionDetail.Messages;

namespace System.Process.IntegrationTests.Adapters
{
    public class TransactionDetailJsonAdapter
    {
        public TransactionDetailRequest SuccessRequest { get; set; }
        public TransactionDetailResult SuccessResult { get; set; }
        public TransactionDetailRequest ErrorRequest { get; set; }
    }
}
