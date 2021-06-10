using System.Process.Domain.Entities;
using System.Process.Infrastructure.Adapters;
using System.Proxy.Rda.AddItem.Messages;
using System.Proxy.Rda.Authenticate.Messages;
using System.Proxy.Rda.CreateBatch.Messages;
using System.Proxy.Rda.Messages;

namespace System.Process.Application.Commands.ResumeTransfer.Adapters
{
    public class AddItemRdcAdapter : IAdapter<AddItemRequest, BaseResult<CreateBatchResponse>>
    {
        #region Properties
        private BaseResult<AuthenticateResponse> Authenticate { get; }
        private TransferItem Item { get; set; }

        #endregion

        #region Constructor 
        public AddItemRdcAdapter(
            BaseResult<AuthenticateResponse> auth,
            TransferItem item)
        {
            Authenticate = auth;
            Item = item;
        }
        #endregion

        public AddItemRequest Adapt(BaseResult<CreateBatchResponse> input)
        {
            return new AddItemRequest
            {
                BatchReference = input.Result.Batch.BatchReference,
                AccountReference = Item.ReferenceId,
                Amount = Item.Amount,
                FrontImage = Item.FrontImage,
                RearImage = Item.RearImage,
                Credentials = new Proxy.Rda.Common.TokenCredentials
                {
                    SecurityToken = Authenticate.Result.Credentials.SecurityToken,
                    Type = Authenticate.Result.Credentials.Type
                }
            };
        }
    }
}
