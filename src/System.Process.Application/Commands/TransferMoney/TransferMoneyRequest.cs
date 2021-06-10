using MediatR;
using System.Process.Application.Commands.TransferMoney.Request;

namespace System.Process.Application.Commands.TransferMoney
{
    public class TransferMoneyRequest : IRequest<TransferMoneyResponse>
    {
        public string SystemId { get; set; }
        public string CustomerId { get; set; }
        public string ReceiverId { get; set; }
        public AccountFrom AccountFrom { get; set; }
        public AccountTo AccountTo { get; set; }
        public decimal Amount { get; set; }
        public string ReducedPrincipal { get; set; }
        public string Message { get; set; }
        public string SessionId { get; set; }
    }
}