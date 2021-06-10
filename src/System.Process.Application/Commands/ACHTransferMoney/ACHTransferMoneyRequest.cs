using MediatR;
using System.Process.Application.Commands.ACHTransferMoney.Request;

namespace System.Process.Application.Commands.AchTransferMoney
{
    public class AchTransferMoneyRequest : IRequest<AchTransferMoneyResponse>
    {
        public string SystemId { get; set; }
        public string CustomerId { get; set; }
        public string ReceiverId { get; set; }
        public string NextDay { get; set; }
        public decimal Amount { get; set; }
        public string ReducedPrincipal { get; set; }
        public string Memo { get; set; }
        public AchAccountFrom AccountFrom { get; set; }
        public AchAccountTo AccountTo { get; set; }
        public string SessionId { get; set; }
    }
}