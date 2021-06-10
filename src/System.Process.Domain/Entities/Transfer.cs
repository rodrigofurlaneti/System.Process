using System.Collections.Generic;

namespace System.Process.Domain.Entities
{
    public class Transfer
    {
        public string LifeCycleId { get; set; }
        public string TransferType { get; set; }
        public string TransferDirection { get; set; }
        public string SystemId { get; set; }
        public string CustomerId { get; set; }
        public string CustomerIsEnabled { get; set; }
        public string ReceiverId { get; set; }
        public string Message { get; set; }
        public decimal Amount { get; set; }
        public string AccountFromNumber { get; set; }
        public string AccountFromType { get; set; }
        public string AccountFromRoutingNumber { get; set; }
        public string SenderName { get; set; }
        public string AccountToNumber { get; set; }
        public string AccountToType { get; set; }
        public string AccountToRoutingNumber { get; set; }
        public string ReceiverFirstName { get; set; }
        public string ReceiverLastName { get; set; }
        public string ReceiverEmail { get; set; }
        public string ReceiverPhone { get; set; }
        public string ReceiverAddressLine1 { get; set; }
        public string ReceiverAddressLine2 { get; set; }
        public string ReceiverAddressLine3 { get; set; }
        public string ReceiverAddressCity { get; set; }
        public string ReceiverAddressState { get; set; }
        public string ReceiverAddressCountry { get; set; }
        public string ReceiverAddressZipCode { get; set; }
        public string BankName { get; set; }
        public string BankAddressLine1 { get; set; }
        public string BankAddressLine2 { get; set; }
        public string BankAddressLine3 { get; set; }
        public string BankAddressCity { get; set; }
        public string BankAddressState { get; set; }
        public string BankAddressCountry { get; set; }
        public string BankAddressZipCode { get; set; }
        public string ReducedPrincipal { get; set; }
        public string NextDay { get; set; }
        public long Geolocation { get; set; }
        public string SessionId { get; set; }
        public int StopSequence { get; set; }
        public ICollection<TransferItem> TransferItems { get; set; }
    }
}
