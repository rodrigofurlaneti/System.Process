using System.Process.Application.Commands.CreateAccount.Response;
using System.Process.Infrastructure.Configs;
using System.Process.Infrastructure.Messages;

namespace System.Process.Application.Commands.CreateAccount
{
    public class RegisterAssetParamsAdapter
    {
        public string RoutingNumber { get; set; }
        public CreateAccountResponse Product { get; set; }
        public AccountMessage AccountMessage { get; set; }
        public RecordTypesConfig RecordTypesConfig { get; set; }
    }
}
