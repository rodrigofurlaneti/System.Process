using MediatR;

namespace System.Process.Application.Commands.CreditCardMakePayment
{
    public class CreditCardMakePaymentRequest : IRequest<CreditCardMakePaymentResponse>
    {
        public int CardId { get; set; }
    }
}
