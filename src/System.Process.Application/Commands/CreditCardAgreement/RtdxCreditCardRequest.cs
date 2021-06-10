using System.Process.Domain.Entities;
using System.Proxy.Rtdx.GetToken.Messages;
using System.Proxy.Salesforce.GetBusinessInformation.Message;
using System.Proxy.Salesforce.GetCommercialCompanyInfo.Message;
using System.Proxy.Salesforce.Messages;
using SalesforceCreditCard = System.Proxy.Salesforce.GetCreditCards.Message;

namespace System.Process.Application.Commands.CreditCardAgreement
{
    public class RtdxCreditCardRequest
    {
        public SalesforceCreditCard.CreditCard SalesforceCreditCard { get; set; }
        public CreditCardAgreementRequest CreditCardAgreementRequest { get; set; }
        public GetBusinessInformationResponse BusinessInformation { get; set; }
        public string Token { get; set; }
        public GetTokenParams TokenParams { get; set; }
        public string CifId { get; set; }
        public CommercialCompanyInfo CommercialCompanyInfo { get; set; }
        public string CompanyId { get; set; }

        public RtdxCreditCardRequest(SalesforceCreditCard.CreditCard card, CreditCardAgreementRequest creditAgreement, GetBusinessInformationResponse business, string token, GetTokenParams tokenParams, string cifId, CommercialCompanyInfo commercialCompanyInfo, string companyId)
        {
            SalesforceCreditCard = card;
            CreditCardAgreementRequest = creditAgreement;
            BusinessInformation = business;
            Token = token;
            TokenParams = tokenParams;
            CifId = cifId;
            CommercialCompanyInfo = commercialCompanyInfo;
            CompanyId = companyId;
        }
    }

}
