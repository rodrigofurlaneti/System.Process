using System.Process.Domain.Constants;
using System.Process.Domain.Enums;
using System.Process.Domain.ValueObjects;
using System.Process.Infrastructure.Adapters;
using System.Proxy.Salesforce.UpdateAsset.Messages;
using System.Collections.Generic;
using System.Linq;

namespace System.Process.Application.Commands.CreateAccount
{
    public class UpdateAssetAdapter : IAdapter<UpdateAssetParams, IList<AccountInfo>>
    {
        public UpdateAssetParams Adapt(IList<AccountInfo> input)
        {
            return new UpdateAssetParams
            {
                Id = input.Single(x => x.Origin == OriginAccount.E).AssetId,
                UpdateAssetBody = new UpdateAssetBody
                {
                    Status = Constants.ActivedStatus
                }
            };
        }
    }
}
