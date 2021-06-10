using System;
using System.Collections.Generic;

namespace System.Process.Domain.ValueObjects
{
    public class BusinessInformation
    {
        public int BusinessOwnersGroups { get; set; }
        public string LegalName { get; set; }
        public string DbaName { get; set; }
        public TaxId TaxId { get; set; }
        public string IndustryCode { get; set; }
        public string IndustryCategory { get; set; }
        public string Industry { get; set; }
        public string Website { get; set; }
        public string EntityType { get; set; }
        public string EntitySubType { get; set; }
        public decimal EstimatedSalesVolume { get; set; }
        public string FormationCountry { get; set; }
        public string FormationState { get; set; }
        public DateTime? FormationDate { get; set; }
        public int? NumberOfEmployees { get; set; }
        public IList<Address> Addresses { get; set; }
        public IList<Contact> Contacts { get; set; }
        public Transaction Transaction { get; set; }
    }
}