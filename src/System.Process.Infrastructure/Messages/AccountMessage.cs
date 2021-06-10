using System;
using System.Process.Domain.Enums;
using System.Process.Domain.ValueObjects;
using System.Collections.Generic;

namespace System.Process.Infrastructure.Messages
{
    public class AccountMessage
    {
        public string ApplicationId { get; set; }
        public string MerchantId { get; set; }
        public string SalesforceId { get; set; }
        public string BusinessCif { get; set; }
        public string MpaId { get; set; }
        public DateTime? MpaSubmitDate { get; set; }
        public string CustomerReturnPolicy { get; set; }
        public string CustomerReturnPolicyText { get; set; }
        public ProcessStep? ProcessStep { get; set; }
        public UnderwritingProcess UnderwritingProcess { get; set; }
        public OnboardingStatus OnboardingStatus { get; set; }
        public bool? PaperApplication { get; set; }
        public bool? BusinessRepresentative { get; set; }
        public bool? OpenCheckingAccount { get; set; }
        public bool? LinkedExternalAccount { get; set; }
        public BusinessInformation BusinessInformation { get; set; }
        public Order Order { get; set; }
        public BankAccount BankAccount { get; set; }
        public Iso Iso { get; set; }
        public Pricing Pricing { get; set; }
        public IList<Principal> Principals { get; set; }
        public IList<Document> Documents { get; set; }
        public IList<AccountInfo> Process { get; set; }
        public OriginChannel OriginChannel { get; set; }
        public bool ManualDecision { get; set; }
        public Term ProhibitedActivitiesTerm { get; set; }
        public Term UserLicenseTerm { get; set; }
        public Term PrivatePolicyTerm { get; set; }
        public Term MerchantAgreementTerm { get; set; }
        public Term EquipmentTerm { get; set; }
        public Term TerminalTerm { get; set; }
        public IList<Term> Terms { get; set; }
    }
}
