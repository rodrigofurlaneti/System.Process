using System.Process.Infrastructure.Messages;
using System.Proxy.Salesforce.UpdateAsset.Messages;

namespace System.Process.Application.Commands.CreateCustomerId.Adapters
{
    public class UpdateAssetAdapter
    {
        public UpdateAssetParams Adapt(AccountMessage input)
        {
            return new UpdateAssetParams
            {
                Id = GetAssetId(input),
                UpdateAssetBody = new UpdateAssetBody
                {                  
                    AccountNumber = input.BankAccount.AccountNumber,
                    IsActive = true
                }
            };
        }

        public string GetAssetId(AccountMessage input)
        {
            var assetId = string.Empty;
            var accountNumber = input.BankAccount.AccountNumber;

            foreach(var account in input.Process)
            {
                if (account.Number == accountNumber)
                {
                    assetId = account.AssetId;

                    break;
                }
            }

            return assetId;
        }
    }
}
