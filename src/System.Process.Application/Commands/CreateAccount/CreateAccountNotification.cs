using MediatR;
using System.Process.Infrastructure.Messages;
using System.Phoenix.Event.Messages;

namespace System.Process.Application.Commands.CreateAccount
{
    public class CreateAccountNotification : INotification
    {
        public MessageContent<AccountMessage> MessageContent { get; set; }
    }   
}
