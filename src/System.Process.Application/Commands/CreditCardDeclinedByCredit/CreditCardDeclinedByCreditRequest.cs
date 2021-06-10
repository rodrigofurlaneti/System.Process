using MediatR;

namespace System.Process.Application.Commands.CreditCardDeclinedByCredit
{
    public class CreditCardDeclinedByCreditRequest : IRequest<CreditCardDeclinedByCreditResponse>
    {
        public string AssetId { get; set; }
    }
}
