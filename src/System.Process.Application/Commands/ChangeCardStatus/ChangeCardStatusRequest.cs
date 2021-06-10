using MediatR;

namespace System.Process.Application.Commands.ChangeCardStatus
{
    public class ChangeCardStatusRequest : IRequest<ChangeCardStatusResponse>
    {
        public int CardId { get; set; }
        public string Action { get; set; }
    }
}
