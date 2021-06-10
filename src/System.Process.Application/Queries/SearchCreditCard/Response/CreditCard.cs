namespace System.Process.Application.Queries.SearchCreditCard.Response
{
    public class CreditCard
    {
        public string AssetId { get; set; }
        public string Status { get; set; }
        public string CreditCardType { get; set; }
        public string CreatedDate { get; set; }
        public decimal CreditLimit { get; set; }
        public string Description { get; set; }
    }
}
