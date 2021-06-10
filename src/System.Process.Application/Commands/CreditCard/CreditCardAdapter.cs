using System.Collections.Generic;
using System.Process.Domain.Constants;
using System.Process.Infrastructure.Adapters;
using System.Process.Infrastructure.Configs;
using System.Proxy.Salesforce;
using System.Proxy.Salesforce.GetCreditCards.Message;
using System.Proxy.Salesforce.Messages;
using System.Proxy.Salesforce.RegisterAsset.Messages;

namespace System.Process.Application.Commands.CreditCard
{
    public class CreditCardAdapter :
        IAdapter<RegisterAssetParams, CreditCardRequest>,
        IAdapter<CreditCardResponse, BaseResult<SalesforceResult>>,
        IAdapter<GetCreditCardsParams, string>
    {
        #region Properties

        private RecordTypesConfig RecordTypesConfig { get; set; }

        #endregion

        #region Constructor

        public CreditCardAdapter(RecordTypesConfig recordTypesConfig)
        {
            RecordTypesConfig = recordTypesConfig;
        }

        #endregion

        #region IAdapter implementation

        public RegisterAssetParams Adapt(CreditCardRequest input)
        {
            return new RegisterAssetParams
            {
                Account = new Account
                {
                    SystemId = input.SystemId
                },
                Name = Constants.CreditCardName,
                RecordTypeId = RecordTypesConfig.AssetCreditCard,
                Status = Constants.PendingAnalysisStatus
            };
        }

        public CreditCardResponse Adapt(BaseResult<SalesforceResult> input)
        {
            return new CreditCardResponse
            {
                AssetId = input?.Result?.Id,
                Success = (bool)(input?.IsSuccess),
                ErrorMessages = GetErrors(input?.Result?.Errors)
            };
        }

        public GetCreditCardsParams Adapt(string input)
        {
            return new GetCreditCardsParams
            {
                SystemId = input,
                RecordTypeId = RecordTypesConfig.AssetCreditCard
            };
        }

        #endregion

        #region Private Methods
        private IList<ErrorMessage> GetErrors(List<Proxy.Salesforce.Messages.ErrorMessage> errorMessages)
        {
            var messages = new List<ErrorMessage>();

            if (errorMessages is null || errorMessages.Count.Equals(0))
            {
                return messages;
            }

            foreach (var error in errorMessages)
            {
                messages.Add(new ErrorMessage
                {
                    ErrorCode = error.ErrorCode,
                    Message = error.Message
                });
            }

            return messages;
        }

        #endregion
    }
}
