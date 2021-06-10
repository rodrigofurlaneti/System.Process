using MediatR;

namespace System.Process.Application.Commands.CreditCardCancellation
{
    public class CreditCardCancellationRequest : IRequest<CreditCardCancellationResponse>
    {
        public string SystemId { get; set; }
        public string AssetId { get; set; }
    }
}
