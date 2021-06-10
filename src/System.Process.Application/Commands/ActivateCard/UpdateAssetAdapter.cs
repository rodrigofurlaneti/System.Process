using System;
using System.Process.Infrastructure.Adapters;
using System.Proxy.Salesforce.UpdateAsset.Messages;

namespace System.Process.Application.Commands.ActivateCard
{
    public class UpdateAssetAdapter : IAdapter<UpdateAssetParams, string>
    {
        public UpdateAssetParams Adapt(string assetId)
        {
            return new UpdateAssetParams
            {
                Id = assetId,
                UpdateAssetBody = new UpdateAssetBody
                {
                    Status = "Activated",
                    ActivationTime = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss")
                }
            };
        }
    }
}
