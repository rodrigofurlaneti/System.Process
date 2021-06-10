using MediatR;

namespace System.Process.Application.Commands.CreditCardBalance
{
    public class CreditCardBalanceRequest : IRequest<CreditCardBalanceResponse>
    {
        public int CardId { get; set; }
    }
}
