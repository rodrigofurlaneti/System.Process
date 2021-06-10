using System.Proxy.Fis.TransactionDetail.Messages.Result;

namespace System.Process.Application.Commands.TransactionDetail
{
    public class TransactionDetailResponse
    {
        public MetadataTransactionDetail Metadata { get; set; }
        public Transaction Transaction { get; set; }
    }
}
