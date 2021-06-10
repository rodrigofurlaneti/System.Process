using MediatR;

namespace System.Process.Application.Commands.TransactionDetail
{
    public class ConsultCardsByKeyTransactionRequest : IRequest<TransactionDetailResponse>
    {
        public int CardId { get; set; }
        public string PrimaryKey { get; set; }
    }
}
