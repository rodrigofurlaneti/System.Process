using MediatR;
using System.Process.Infrastructure.Messages;
using System.Phoenix.Event.Messages;

namespace System.Process.Application.Commands.CreateCustomerId
{
    public class CreateCustomerIdNotification : INotification
    {
        public MessageContent<AccountMessage> MessageContent { get; set; }
    }
}
