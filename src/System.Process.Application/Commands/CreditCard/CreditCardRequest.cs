using MediatR;

namespace System.Process.Application.Commands.CreditCard
{
    public class CreditCardRequest : IRequest<CreditCardResponse>
    {
        public string SystemId { get; set; }
    }
}
