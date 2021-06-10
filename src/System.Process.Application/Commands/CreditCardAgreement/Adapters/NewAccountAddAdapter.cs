using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Process.Domain.ValueObjects;
using System.Process.Infrastructure.Adapters;
using System.Proxy.Rtdx.NewAccountAdd.Messages;

namespace System.Process.Application.Commands.CreditCardAgreement.Adapters
{
    public class NewAccountAddAdapter : IAdapter<NewAccountAddParams, RtdxCreditCardRequest>
    {
        #region Properties
        private ILogger<CreditCardAgreementCommand> Logger { get; }

        private ProcessConfig ProcessConfig { get; }

        #endregion

        #region Constructor

        public NewAccountAddAdapter(ProcessConfig config, ILogger<CreditCardAgreementCommand> logger)
        {
            ProcessConfig = config;
            Logger = logger;
        }

        #endregion

        #region IAdapter implementation
        public NewAccountAddParams Adapt(RtdxCreditCardRequest input)
        {
            Logger.LogInformation($"ProcessConfig NewAccountAddParams: { JsonConvert.SerializeObject(ProcessConfig) }");
            Logger.LogInformation($"Input NewAccountAddParams: { JsonConvert.SerializeObject(input) }");
            var asset = input.CommercialCompanyInfo.Assets.Records.FirstOrDefault();
            var contact = input?.CommercialCompanyInfo?.Contacts?.Records.FirstOrDefault();

            var newAccountAddParams = new NewAccountAddParams
            {
                CorpId = input.TokenParams.CorpId,
                SecurityToken = input.Token,
                Corp = input.TokenParams.CorpId,
                AccountBin = asset?.Bin,
                CommercialCardIndicator = ProcessConfig.RtdxParamsConfig.NewAccountAdd.CommercialCardIndicator,
                Product = asset?.CreditCardProduct,
                Subproduct = asset?.CreditCardSubproduct,
                LastNameTwo = input?.BusinessInformation?.LastName,
                FirstNameTwo = input?.BusinessInformation?.FirstName,
                MiddleNameTwo = input?.BusinessInformation?.MiddleName,
                CorpName = input?.BusinessInformation?.Account?.LegalName,
                CompanyId = input.CompanyId,
                SublevelNumber = ProcessConfig.RtdxParamsConfig.NewAccountAdd.SublevelNumber,
                SublevelId = ProcessConfig.RtdxParamsConfig.NewAccountAdd.SublevelId,
                AddressLineOne = input?.CreditCardAgreementRequest?.Address?.Line1,
                AddressLineTwo = input?.CreditCardAgreementRequest?.Address?.Line2,
                AddressLineThree = input?.CreditCardAgreementRequest?.Address?.Line3,
                BusinessPhone = string.Concat(contact?.Phone.Where(char.IsDigit)),
                MobilePhone = string.Concat(contact?.MobilePhone.Where(char.IsDigit)),
                FinalScore = input?.BusinessInformation?.Account == null ? string.Empty : input?.BusinessInformation?.Account?.RiskRating.ToString(),
                SourceCode = ProcessConfig.RtdxParamsConfig.NewAccountAdd.SourceCode,
                City = input?.CreditCardAgreementRequest?.Address?.City,
                State = input?.CreditCardAgreementRequest?.Address?.State,
                Zip = input?.CreditCardAgreementRequest?.Address?.ZipCode,
                BillDay = CalculateBillDay(),
                CreditLimit = input?.SalesforceCreditCard?.CreditLimit == null ? "0" : String.Format("{0:0}", input?.SalesforceCreditCard?.CreditLimit),
                StatementGroupCode = ProcessConfig.RtdxParamsConfig.NewAccountAdd.StatementGroupCode,
                FinanceCharge = ProcessConfig.RtdxParamsConfig.NewAccountAdd.FinanceCharge,
                CardTwoType = ProcessConfig.RtdxParamsConfig.NewAccountAdd.CardTwoType,
                NumberCardTwoIssueNameTwo = ProcessConfig.RtdxParamsConfig.NewAccountAdd.NumberCardTwoIssueNameTwo,
                CreditAssociationTwo = ProcessConfig.RtdxParamsConfig.NewAccountAdd.CreditAssociationTwo,
                BirthDateTwo = DateTime.ParseExact(contact?.Birthdate, "yyyy-MM-dd", null).ToString("yyyyMMdd"),
                SsnTwo = input?.BusinessInformation?.SocialSecurity,
                CardAnnualFeeIndicator = ProcessConfig.RtdxParamsConfig.NewAccountAdd.CardAnnualFeeIndicator,
                CreatePlastics = ProcessConfig.RtdxParamsConfig.NewAccountAdd.CreatePlastics,
                CurrencyCode = ProcessConfig.RtdxParamsConfig.NewAccountAdd.CurrencyCode,
                FiscalYearEndMonth = ProcessConfig.RtdxParamsConfig.NewAccountAdd.FiscalYearEndMonth,
                LoyaltyCode = ProcessConfig.RtdxParamsConfig.NewAccountAdd.LoyaltyCode,
                CompanyName = input?.BusinessInformation?.Account.LegalName,
                EmbossNameTwo = string.Concat(input?.BusinessInformation?.FirstName, input?.BusinessInformation?.MiddleName, input?.BusinessInformation?.LastName),
                PrimaryCardDeliveryCode = ProcessConfig.RtdxParamsConfig.NewAccountAdd.PrimaryCardDeliveryCode,
                InstitutionId = ProcessConfig.RtdxParamsConfig.NewAccountAdd.InstitutionId,
                EmbossNameIndicator = ProcessConfig.RtdxParamsConfig.NewAccountAdd.EmbossNameIndicator,
                OverLimitOptionIndicator = ProcessConfig.RtdxParamsConfig.NewAccountAdd.OverLimitOptionIndicator,
                ProcessingType = ProcessConfig.RtdxParamsConfig.NewAccountAdd.ProcessingType,
                CardholderVerificationMethodIndTwo = ProcessConfig.RtdxParamsConfig.NewAccountAdd.CardholderVerificationMethodIndTwo,
                CardActivationStatus = ProcessConfig.RtdxParamsConfig.NewAccountAdd.CardActivationStatus,
                MilitaryLendingActIndP1C = ProcessConfig.RtdxParamsConfig.NewAccountAdd.MilitaryLendingActIndP1C,
                SMSTextConsentInd = ProcessConfig.RtdxParamsConfig.NewAccountAdd.SMSTextConsentInd,
                CashLimit = "000"
            };

            newAccountAddParams = FillProps(newAccountAddParams);
            return newAccountAddParams;
        }

        #endregion

        #region Methods 
        private static string CalculateBillDay()
        {
            int dayNow = DateTime.Now.Day;
            if (dayNow >= 27 && dayNow <= 6)
            {
                return "1";
            }
            else if (dayNow >= 7 && dayNow <= 15)
            {
                return "11";
            }
            return "20";
        }

        private NewAccountAddParams FillProps(NewAccountAddParams newAccountAddParams)
        {
            PropertyInfo[] properties = newAccountAddParams.GetType().GetProperties();

            foreach (var propertyInfo in properties)
            {
                if (propertyInfo.PropertyType == typeof(string) && (String)propertyInfo.GetValue(newAccountAddParams, null) == null)
                {
                    propertyInfo.SetValue(newAccountAddParams, String.Empty, null);
                }
            }

            return newAccountAddParams;
        }
        #endregion
    }
}
