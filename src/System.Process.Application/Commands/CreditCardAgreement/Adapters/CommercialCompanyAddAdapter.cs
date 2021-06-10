using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Process.Domain.ValueObjects;
using System.Process.Infrastructure.Adapters;
using System.Proxy.Rtdx.CommercialCompanyAdd.Messages;

namespace System.Process.Application.Commands.CreditCardAgreement.Adapters
{
    public class CommercialCompanyAddAdapter : IAdapter<CommercialCompanyAddParams, RtdxCreditCardRequest>
    {
        #region Properties
        private ILogger<CreditCardAgreementCommand> Logger { get; }
        private ProcessConfig ProcessConfig { get; }

        #endregion

        #region Constructor

        public CommercialCompanyAddAdapter(ProcessConfig config,
            ILogger<CreditCardAgreementCommand> logger)
        {
            ProcessConfig = config;
            Logger = logger;
        }

        #endregion

        #region IAdapter implementation
        public CommercialCompanyAddParams Adapt(RtdxCreditCardRequest input)
        {
            Logger.LogInformation($"ProcessConfig CommercialCompanyAdd: { JsonConvert.SerializeObject(ProcessConfig) }");
            Logger.LogInformation($"Input CommercialCompanyAdd: { JsonConvert.SerializeObject(input) }");
            var asset = input.CommercialCompanyInfo.Assets.Records.FirstOrDefault();
            var contact = input.CommercialCompanyInfo.Contacts.Records.FirstOrDefault();

            var commercialCompanyAddParams = new CommercialCompanyAddParams
            {
                SecurityToken = input.Token,
                CorpId = ProcessConfig.RtdxParamsConfig.CommercialCompanyAdd.CorpId,
                CompanyId = input.CompanyId,
                Status = ProcessConfig.RtdxParamsConfig.CommercialCompanyAdd.Status,
                CompanyName = input.BusinessInformation.Account.LegalName,
                EmbossingName = ProcessConfig.RtdxParamsConfig.CommercialCompanyAdd.EmbossingName,
                RetainCreditBalance = ProcessConfig.RtdxParamsConfig.CommercialCompanyAdd.RetainCreditBalance,
                CompanyAddressOne = FormatBusinessAddress(input.BusinessInformation.Account.BusinessAddress),
                CompanyCity = input?.CreditCardAgreementRequest?.Address?.City,
                CompanyState = input?.CreditCardAgreementRequest?.Address?.State,
                PostalCode = input?.CreditCardAgreementRequest?.Address?.ZipCode,
                ContactNameOne = contact.Name,
                ContactPhoneNumberOne = string.Concat(contact.Phone.Where(char.IsDigit)),
                ContactFaxNumberOne = contact.Fax,
                BillDay = CalculateBillDay(),
                CompanyType = ProcessConfig.RtdxParamsConfig.CommercialCompanyAdd.CompanyType,
                ReportGroup = ProcessConfig.RtdxParamsConfig.CommercialCompanyAdd.ReportGroup,
                SendStatement = ProcessConfig.RtdxParamsConfig.CommercialCompanyAdd.SendStatement,
                IndustryType = ProcessConfig.RtdxParamsConfig.CommercialCompanyAdd.IndustryType,
                DistributionMethodOverride = ProcessConfig.RtdxParamsConfig.CommercialCompanyAdd.DistributionMethodOverride,
                CreditLimit = input?.SalesforceCreditCard?.CreditLimit == null ? "0" : ((int)input?.SalesforceCreditCard?.CreditLimit).ToString(),
                FiscalYear = ProcessConfig.RtdxParamsConfig.CommercialCompanyAdd.FiscalYear,
                AuthroziationLevel = ProcessConfig.RtdxParamsConfig.CommercialCompanyAdd.AuthroziationLevel,
                ProductOne = asset.CreditCardProduct,
                SubProductOne = asset.CreditCardSubproduct,
                MembershipFeeOptionOne = ProcessConfig.RtdxParamsConfig.CommercialCompanyAdd.MembershipFeeOptionOne,
                MembershipFeeFrequencyOne = ProcessConfig.RtdxParamsConfig.CommercialCompanyAdd.MembershipFeeFrequencyOne,
                MembershipFeeLevelOne = ProcessConfig.RtdxParamsConfig.CommercialCompanyAdd.MembershipFeeLevelOne,
                MembershipFeeSuppressOne = ProcessConfig.RtdxParamsConfig.CommercialCompanyAdd.MembershipFeeSuppressOne,
                PinIndicatorOne = ProcessConfig.RtdxParamsConfig.CommercialCompanyAdd.PinIndicatorOne,
                EmbossingCardholderName = ProcessConfig.RtdxParamsConfig.CommercialCompanyAdd.EmbossingCardholderName,
                RelationManager = "ARTROCA"
            };

            commercialCompanyAddParams = FillProps(commercialCompanyAddParams);

            return commercialCompanyAddParams;
        }

        #endregion

        #region Methods 

        private string FormatBusinessAddress(string businessAddress)
        {
           if (string.IsNullOrEmpty(businessAddress))
            {
                return string.Empty;
            }

            return businessAddress.Split(",").FirstOrDefault();
        }

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

        private CommercialCompanyAddParams FillProps(CommercialCompanyAddParams commercialCompanyAddParams)
        {
            PropertyInfo[] properties = commercialCompanyAddParams.GetType().GetProperties();

            foreach (var propertyInfo in properties)
            {
                if (propertyInfo.PropertyType == typeof(string) && (String)propertyInfo.GetValue(commercialCompanyAddParams, null) == null)
                {
                    propertyInfo.SetValue(commercialCompanyAddParams, String.Empty, null);
                }
            }

            return commercialCompanyAddParams;
        }
        #endregion
    }
}