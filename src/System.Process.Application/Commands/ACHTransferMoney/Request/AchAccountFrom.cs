namespace System.Process.Application.Commands.ACHTransferMoney.Request
{
    public class AchAccountFrom
    {
        public string Name { get; set; }
        public string RoutingNumber { get; set; }
        public string AccountId { get; set; }
        public string AccountType { get; set; }
    }
}