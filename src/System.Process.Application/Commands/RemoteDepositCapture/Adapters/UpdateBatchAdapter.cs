using System.Process.Infrastructure.Adapters;
using System.Process.Infrastructure.Configs;
using System.Proxy.Rda.AddItem.Messages;
using System.Proxy.Rda.Authenticate.Messages;
using System.Proxy.Rda.CreateBatch.Messages;
using System.Proxy.Rda.Messages;
using System.Proxy.Rda.UpdateBatch.Messages;
using System;
using System.Collections.Generic;

namespace System.Process.Application.Commands.RemoteDepositCapture.Adapters
{
    public class UpdateBatchAdapter : IAdapter<UpdateBatchRequest, CreateBatchResponse>
    {
        #region Properties
        private BaseResult<AuthenticateResponse> Authenticate { get; }
        private List<BaseResult<AddItemResponse>> AddItem { get; }
        private RdaCredentialsConfig RdaConfig { get; set; }
        #endregion

        #region Constructor 
        public UpdateBatchAdapter(
            BaseResult<AuthenticateResponse> auth,
            List<BaseResult<AddItemResponse>> addItem,
            RdaCredentialsConfig rdaConfig)
        {
            Authenticate = auth;
            AddItem = addItem;
            RdaConfig = rdaConfig;
        }
        #endregion

        public UpdateBatchRequest Adapt(CreateBatchResponse input)
        {
            bool validItems = ValidateItems(AddItem);
            return new UpdateBatchRequest
            {
                Criteria = new Criteria
                {
                    BatchReference = input.Batch.BatchReference,
                    Type = validItems ? RdaConfig.TypeUpdateClose : RdaConfig.TypeUpdateDelete
                },
                Credentials = new Proxy.Rda.Common.TokenCredentials
                {
                    SecurityToken = Authenticate.Result.Credentials.SecurityToken
                }
            };
        }

        private bool ValidateItems(List<BaseResult<AddItemResponse>> addItem)
        {
            foreach(var item in addItem)
            {
                if (item.Result.ValidationResults.Count > 0)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
