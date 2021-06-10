using MediatR;

namespace System.Process.Application.Commands.CreditCardReplace
{
    public class CreditCardReplaceRequest : IRequest<CreditCardReplaceResponse>
    {
        public int CardId { get; set; }
        public string AddressType { get; set; }
        public string Reason { get; set; }
    }
}
