namespace System.Process.Application.Commands.CreditCardBalance
{
    public class CreditCardBalanceResponse
    {
        public int CardId { get; set; }
        public string TotalNewBalance { get; set; }
        public string AvailableCredit { get; set; }
    }
}
