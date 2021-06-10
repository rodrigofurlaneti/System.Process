namespace System.Process.Domain.ValueObjects
{
    public class BankAccount
    {
        public string BankCode { get; set; }
        public string RoutingNumber { get; set; }
        public string AccountNumber { get; set; }
        public bool SettlementSafra { get; set; }
        public string NameOfAccountHolder { get; set; }
        public string NameOfBank { get; set; }
        public Term DebitCardTerm { get; set; }
        public Term SafraDigitalTerm { get; set; }
        public Term FeeScheduleTerm { get; set; }
    }
}