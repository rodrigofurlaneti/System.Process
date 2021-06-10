using MediatR;

namespace System.Process.Application.Commands.CreditCardActivation
{
    public class CreditCardActivationRequest : IRequest<CreditCardActivationResponse>
    {
        public int CardId { get; set; }
        public string SystemId { get; set; }
        public string LastFour { get; set; }
        public string ExpireDate { get; set; }
    }
}
