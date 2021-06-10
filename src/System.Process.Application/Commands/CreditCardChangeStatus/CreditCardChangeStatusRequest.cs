using MediatR;

namespace System.Process.Application.Commands.CreditCardChangeStatus
{
    public class CreditCardChangeStatusRequest : IRequest<CreditCardChangeStatusResponse>
    {
        public int CardId { get; set; }
        public string Action { get; set; }
    }
}
