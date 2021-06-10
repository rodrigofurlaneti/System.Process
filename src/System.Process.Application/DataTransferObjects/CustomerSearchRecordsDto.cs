using System;

namespace System.Process.Application.DataTransferObjects
{
    public class CustomerSearchRecordsDto
    {
        public string Name { get; set; }
        public string Bank { get; set; }
        public string AccountType { get; set; }
        public string RoutingNumber { get; set; }
        public string AccountNumber { get; set; }
        public string Processtatus { get; set; }
        public string AccountName { get; set; }
        public DateTime OpeningDate { get; set; }
        public decimal? CurrentBalance { get; set; }
        public string CurrentBalanceCurrency { get; set; }
        public decimal? AvailableBalance { get; set; }
        public string AvailableBalanceCurrency { get; set; }
    }
}
