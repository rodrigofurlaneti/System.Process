using MediatR;

namespace System.Process.Application.Commands.ChangeCardPin
{
    public class ChangeCardPinRequest : IRequest<ChangeCardPinResponse>
    {
        public int CardId { get; set; }
        public string NewPin { get; set; }
        public string CustomerId { get; set; }
    }
}
