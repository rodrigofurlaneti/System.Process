using System.Process.Domain.Constants;
using System.Process.Infrastructure.Adapters;
using System.Proxy.Salesforce.UpdateAsset.Messages;

namespace System.Process.Application.Commands.CreditCardDeclinedByCredit
{
    public class CreditCardDeclinedByCreditAdapter :
        IAdapter<UpdateAssetParams, string>
    {

        public UpdateAssetParams Adapt(string input)
        {
            return new UpdateAssetParams
            {
                Id = input,
                UpdateAssetBody = new UpdateAssetBody
                {
                    Status = Constants.DeclinedByCreditStatus
                }
            };
        }
    }
}
