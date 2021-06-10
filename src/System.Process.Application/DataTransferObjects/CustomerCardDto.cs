namespace System.Process.Application.DataTransferObjects
{
    public class CustomerCardDto
    {
        public int CardId { get; set; }
        public string AssetId { get; set; }
        public string CardType { get; set; }
        public string Bin { get; set; }
        public string Pan { get; set; }
        public string AccountBalance { get; set; }
        public string BusinessName { get; set; }
        public string CardHolder { get; set; }
        public string CardStatus { get; set; }
        public string ExpirationDate { get; set; }
        public bool Locked { get; set; }
    }
}
