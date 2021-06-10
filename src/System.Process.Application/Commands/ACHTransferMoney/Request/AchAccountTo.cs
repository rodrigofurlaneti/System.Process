namespace System.Process.Application.Commands.ACHTransferMoney.Request
{
    public class AchAccountTo
    {
        public string Name { get; set; }
        public string RoutingNumber { get; set; }
        public string AccountId { get; set; }
        public string AccountType { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }
}
