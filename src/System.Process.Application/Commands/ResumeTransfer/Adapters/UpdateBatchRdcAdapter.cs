using System.Collections.Generic;
using System.Process.Infrastructure.Adapters;
using System.Proxy.Rda.AddItem.Messages;
using System.Proxy.Rda.Authenticate.Messages;
using System.Proxy.Rda.CreateBatch.Messages;
using System.Proxy.Rda.Messages;
using System.Proxy.Rda.UpdateBatch.Messages;

namespace System.Process.Application.Commands.ResumeTransfer.Adapters
{
    public class UpdateBatchRdcAdapter : IAdapter<UpdateBatchRequest, CreateBatchResponse>
    {
        #region Properties
        private BaseResult<AuthenticateResponse> Authenticate { get; }
        private List<BaseResult<AddItemResponse>> AddItem { get; }
        #endregion

        #region Constructor 
        public UpdateBatchRdcAdapter(
            BaseResult<AuthenticateResponse> auth,
            List<BaseResult<AddItemResponse>> addItem)
        {
            Authenticate = auth;
            AddItem = addItem;
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
                    Type = validItems ? "close" : "canceled"
                },
                Credentials = new Proxy.Rda.Common.TokenCredentials
                {
                    SecurityToken = Authenticate.Result.Credentials.SecurityToken
                }
            };
        }

        private bool ValidateItems(List<BaseResult<AddItemResponse>> addItem)
        {
            foreach (var item in addItem)
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
