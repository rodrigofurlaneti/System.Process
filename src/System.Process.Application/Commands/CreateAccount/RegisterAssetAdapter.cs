using System;
using System.Process.Application.Commands.CreateAccount.Card;
using System.Process.Domain.Constants;
using System.Process.Infrastructure.Adapters;
using System.Process.Infrastructure.Configs;
using System.Proxy.Salesforce.RegisterAsset.Messages;

namespace System.Process.Application.Commands.CreateAccount
{
    public class RegisterAssetAdapter : IAdapter<RegisterAssetParams, RegisterAssetParamsAdapter>
    {
        #region Properties

        private RecordTypesConfig RecordTypesConfig { get; set; }

        #endregion

        #region Constructor

        public RegisterAssetAdapter(RecordTypesConfig recordTypesConfig)
        {
            RecordTypesConfig = recordTypesConfig;
        }

        #endregion
        public RegisterAssetParams Adapt(RegisterAssetParamsAdapter input)
        {
            var accountNumber = input.Product.AccountIdList[0].AccountNumber;
            return new RegisterAssetParams
            {
                Account = new Account
                {
                    SystemId = input?.AccountMessage?.SalesforceId
                },
                BankAccountType = Proxy.Salesforce.Enums.BankAccountType.System,
                AccountNumber = accountNumber,
                JackHenryId = input?.AccountMessage?.BusinessCif,
                Name = Constants.SafraBankAccount,
                RoutingNumber = Convert.ToInt32(input?.RoutingNumber),
                Status = Constants.ActivedStatus,
                RecordTypeId = input?.RecordTypesConfig?.AssetBanking,
                NameofAccountHolder = input?.AccountMessage?.BusinessInformation?.LegalName,
                ExternalBankName = string.Empty,
                IsSettlement = input?.AccountMessage?.BankAccount?.SettlementSafra ?? false,
                DigitalBankAccountType = input.Product.AccountIdList[0].ProductCode
            };
        }

        public RegisterAssetParams Adapt(AddCardRequest input)
        {
            return new RegisterAssetParams
            {
                Account = new Account
                {
                    SystemId = input.CustomerId
                },
                Name = Constants.DebitCardName,
                RecordTypeId = RecordTypesConfig.AssetDebitCard,
                Pan = input.LastFour,
                CardType = input.CardType,
                DateLastIssued = DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffK"),
                ExpirationDate = new DateTime(Convert.ToInt32("20" + input.ExpirationDate.Substring(0, 2)), Convert.ToInt32(input.ExpirationDate.Substring(2, 2)), 1).ToString("yyyy-MM-dd"),
                Status = Constants.ActiveStatus
            };
        }
    }
}
