using MediatR;

namespace System.Process.Application.Commands.TransactionDetail
{
    public class TransactionDetailRequest : IRequest<TransactionDetailResponse>
    {
        public string Pan { get; set; }
        public string PrimaryKey { get; set; }
    }
}
