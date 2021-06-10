namespace System.Process.Application.Queries.SearchCreditCardsTransactions.Response
{
    public class TransactionItem
    {
        public string PostedDate { get; set; }
        public string TransactionDateTime { get; set; }
        public decimal TransactionAmount { get; set; }
        public string MerchantName { get; set; }
        public string PrimaryKey { get; set; }
        public string TransactionCategory { get; set; }
        public string TransactionType { get; set; }

    }
}
