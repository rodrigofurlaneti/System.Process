using MediatR;

namespace System.Process.Application.Commands.WireTransfer
{
    public class WireTransferAddRequest : IRequest<WireTransferAddResponse>
    {
        public string SystemId { get; set; }
        public string CustomerId { get; set; }
        public string ReceiverId { get; set; }
        public string FromAccountId { get; set; }
        public string FromAccountType { get; set; }
        public string FromRoutingNumber { get; set; }
        public string ToAccountId { get; set; }
        public string ToAccountType { get; set; }
        public string ToRoutingNumber { get; set; }
        public string BankName { get; set; }
        public AddressWireTransferAdd BankAddress { get; set; }
        public decimal Amount { get; set; }
        public string ReceiverFirstName { get; set; }
        public string ReceiverLastName { get; set; }
        public AddressWireTransferAdd ReceiverAddress { get; set; }
        public string ReceiverEmail { get; set; }
        public string ReceiverPhone { get; set; }
        public string Message { get; set; }
        public string SessionId { get; set; }
    }
}
