using System.Process.Domain.ValueObjects;
using System.Phoenix.Domain;
using System.Collections.Generic;

namespace System.Process.Domain.Entities
{
    public class Customer : BaseEntity<string>
    {
        public string BusinessCif { get; set; }
        public string ApplicationId { get; set; }
        public string SalesforceId { get; set; }
        public string MerchantId { get; set; }
        public string LegalName { get; set; }
        public string TinType { get; set; }
        public string DbaName { get; set; }
        public string Tin { get; set; }
        public string IndustryType { get; set; }
        public Address Address { get; set; }
        public BusinessDetail BusinessDetail { get; set; }
        public IList<AccountInfo> Process { get; set; }
        public IList<Shareholder> Shareholders { get; set; }
        public OriginChannel OriginChannel { get; set; }
        public bool OpenCheckingAccount { get; set; }
    }
}
