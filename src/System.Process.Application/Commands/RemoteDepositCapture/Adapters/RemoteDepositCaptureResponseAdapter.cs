using System.Collections.Generic;
using System.Process.Application.Commands.RemoteDepositCapture.Message;
using System.Process.Infrastructure.Adapters;
using System.Proxy.Rda.AddItem.Messages;
using System.Proxy.Rda.Messages;
using System.Proxy.Rda.UpdateBatch.Messages;

namespace System.Process.Application.Commands.RemoteDepositCapture.Adapters
{
    public class RemoteDepositCaptureResponseAdapter : IAdapter<RemoteDepositCaptureResponse, List<BaseResult<AddItemResponse>>>
    {
        RemoteDepositCaptureRequest Request { get; set; }
        BaseResult<UpdateBatchResponse> UpdateBatch { get; set; }
        public RemoteDepositCaptureResponseAdapter(
            RemoteDepositCaptureRequest request, 
            BaseResult<UpdateBatchResponse> updateBatch)
        {
            Request = request;
            UpdateBatch = updateBatch;
        }
        public RemoteDepositCaptureResponse Adapt(List<BaseResult<AddItemResponse>> input)
        {
            return new RemoteDepositCaptureResponse
            {
                Success = UpdateBatch.Result.Batch.StatusDescription == "Submitted" ? true: false,
                Errors = UpdateBatch.Result.Batch.StatusDescription == "Submitted" ?  null: GetErrors(input),
                Count = Request.Count,
                GeoLocation = Request.GeoLocation,
                Item = GetItemList(input),
                ToAccount = Request.ToAccount,
                ToRountingNumber = Request.ToRoutingNumber,
                TotalAmount = Request.TotalAmount
            };
        }

        private List<string> GetErrors(List<BaseResult<AddItemResponse>> input)
        {
            var returnList = new List<string>();

            foreach (var response in input)
            {
                var item = response.Result.Item;
                if (item != null)
                {
                    returnList.Add(item.StatusDescription);
                }
                else
                {
                    returnList.Add(response.Result.ValidationResults[0].Message);
                }
            }

            return returnList;
        }

        private List<ResponseItem> GetItemList(List<BaseResult<AddItemResponse>> input)
        {
            var returnList = new List<ResponseItem>();

            foreach (var response in input)
            {
                var item = response.Result.Item;

                if (item != null)
                {
                    var newItem = new ResponseItem
                    {
                        Amount = item.Amount,
                        BatchReference = item.BatchReference,
                        ItemReference = item.ItemReference,
                        Status = item.Status,
                        StatusDescription = item.StatusDescription,
                        TransactionReferenceNumber = item.TransactionReferenceNumber
                    };

                    returnList.Add(newItem);
                }
                else
                {
                    var newItem = new ResponseItem
                    {
                        StatusDescription = response.Result.ValidationResults[0].Message,
                    };

                    returnList.Add(newItem);
                }
            }

            return returnList;
        }
    }
}
