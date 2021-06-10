using System.Linq;
using System.Process.Application.Commands.RemoteDepositCapture.Message;
using System.Process.Infrastructure.Adapters;
using System.Proxy.Rda.AddItem.Messages;
using System.Proxy.Rda.Authenticate.Messages;
using System.Proxy.Rda.CreateBatch.Messages;
using System.Proxy.Rda.Messages;
using System.Proxy.RdaAdmin.GetProcessCriteriaReference.Messages;
using System.Proxy.RdaAdmin.Messages;

namespace System.Process.Application.Commands.RemoteDepositCapture.Adapters
{
    public class AddItemAdapter : IAdapter<AddItemRequest, BaseResult<CreateBatchResponse>>
    {
        #region Properties
        private BaseResult<AuthenticateResponse> Authenticate { get; }
        private AdminBaseResult<GetProcessCriteriaReferenceResponse> Account { get; set; }
        private RequestItem Item { get; set; }

        #endregion

        #region Constructor 
        public AddItemAdapter(
            BaseResult<AuthenticateResponse> auth,
            AdminBaseResult<GetProcessCriteriaReferenceResponse> account,
            RequestItem item)
        {
            Authenticate = auth;
            Account = account;
            Item = item;
        }
        #endregion

        public AddItemRequest Adapt(BaseResult<CreateBatchResponse> input)
        {
            return new AddItemRequest
            {
                BatchReference = input.Result.Batch.BatchReference,
                AccountReference = Account.Result.Process.First().ReferenceId,
                Amount = Item.Amount,
                FrontImage = Item.FrontImage,
                RearImage = Item.BackImage,
                Credentials = new Proxy.Rda.Common.TokenCredentials
                {
                    SecurityToken = Authenticate.Result.Credentials.SecurityToken,
                    Type = Authenticate.Result.Credentials.Type
                }
            };
        }
    }
}
