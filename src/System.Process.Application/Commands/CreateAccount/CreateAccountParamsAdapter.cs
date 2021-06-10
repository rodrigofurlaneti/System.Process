using System.Process.Infrastructure.Messages;
using System.Process.Worker.Clients.Product;

namespace System.Process.Application.Commands.CreateAccount
{
    public class CreateAccountParamsAdapter
    {
        public ProductMessage Request { get; set; }
        public AccountMessage Message { get; set; }
    }
}
