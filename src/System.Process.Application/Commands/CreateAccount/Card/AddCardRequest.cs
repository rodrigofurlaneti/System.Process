namespace System.Process.Application.Commands.CreateAccount.Card
{
    public class AddCardRequest
    {
        public string CustomerId { get; set; }
        public string Pan { get; set; }
        public string BusinessName { get; set; }
        public string Bin { get; set; }
        public string CardHolder { get; set; }
        public string CardStatus { get; set; }
        public string CardType { get; set; }
        public string ExpirationDate { get; set; }
        public string LastFour { get; set; }
    }
}
