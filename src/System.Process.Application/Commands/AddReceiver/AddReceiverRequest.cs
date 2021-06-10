using MediatR;

namespace System.Process.Application.Commands.AddReceiver
{
    public class AddReceiverRequest : IRequest<AddReceiverResponse>
    {
        public string CustomerId { get; set; }
        public string RoutingNumber { get; set; }
        public string CompanyName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NickName { get; set; }
        public string AccountNumber { get; set; }
        public string AccountType { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAdress { get; set; }
        public string ReceiverType  { get; set; }
        public string BankType  { get; set; }
        public string Ownership  { get; set; }
    }
}
