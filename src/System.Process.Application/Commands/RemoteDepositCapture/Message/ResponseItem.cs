namespace System.Process.Application.Commands.RemoteDepositCapture.Message
{
    public class ResponseItem
    {
        public decimal Amount { get; set; }
        public string ItemReference { get; set; }
        public string Status { get; set; }
        public string StatusDescription { get; set; }
        public string TransactionReferenceNumber { get; set; }
        public string BatchReference { get; set; }
    }
}
