using System.Process.Domain.ValueObjects;
using System.Process.Infrastructure.Adapters;
using System.Proxy.Silverlake.Customer.Messages.Request;

namespace System.Process.Application.Commands.CreateAccount
{
    public class AccountIdGeneratorAdapter : IAdapter<AccountIdGeneratorRequest, CreateAccountParamsAdapter>
    {
        public ProcessConfig ProcessConfig { get; set; }

        public AccountIdGeneratorAdapter(ProcessConfig ProcessConfig)
        {
            ProcessConfig = ProcessConfig;
        }

        public AccountIdGeneratorRequest Adapt(CreateAccountParamsAdapter paramsAdapter)
        {
            return new AccountIdGeneratorRequest
            {
                AccountType = ProcessConfig.AccountType,
                ProductCode = ProcessConfig.AdapterProductCode,
                QuantityOfNumberProcess = int.Parse(ProcessConfig.QuantityOfNumberProcess),
                BusinessDetail = new Proxy.Silverlake.Customer.Messages.Request.BusinessDetail
                {
                    OfficerCode = string.Empty,
                    BranchCode = ProcessConfig.AdapterBranchCode
                }
            };
        }
    }
}
