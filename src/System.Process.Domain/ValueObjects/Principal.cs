using System;
using System.Collections.Generic;

namespace System.Process.Domain.ValueObjects
{
    public class Principal
    {
        public string MerchantId { get; set; }
        public string Cif { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string NameSuffix { get; set; }
        public string SalesforceId { get; set; }
        public TaxId TaxId { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public decimal? Principalship { get; set; }
        public bool MainIndicator { get; set; }
        public string Title { get; set; }
        public Address Address { get; set; }
        public IList<Contact> Contacts { get; set; }
        public bool Bankruptcy { get; set; }
        public string BankruptcyText { get; set; }
        public bool Owner { get; set; }
        public bool HasControl { get; set; }
    }
}