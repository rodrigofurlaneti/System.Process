using MediatR;

namespace System.Process.Application.Commands.DeleteReceiver
{
    public class DeleteReceiverRequest : IRequest<DeleteReceiverResponse>
    {
        public string ReceiverId { get; set; }
    }
}
