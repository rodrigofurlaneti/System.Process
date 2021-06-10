using MediatR;

namespace System.Process.Application.Commands.ValidateAccount
{
    public class ValidateAccountRequest : IRequest<ValidateAccountResponse>
    {
        public string AccountId { get; set; }
        public string AccountType { get; set; }
    }
}