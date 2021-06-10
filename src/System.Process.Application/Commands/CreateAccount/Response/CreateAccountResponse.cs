using System.Collections.Generic;

namespace System.Process.Application.Commands.CreateAccount.Response
{
    public class CreateAccountResponse
    {
        public string ProcessId { get; set; }
        public IList<AccountInformation> AccountIdList { get; set; }
    }
}
