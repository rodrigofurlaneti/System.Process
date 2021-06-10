using System.Process.Domain.Entities;
using System.Process.Infrastructure.Adapters;

namespace System.Process.Application.Commands.AddReceiver
{
    public class AddReceiverAdapter : IAdapter<Receiver, AddReceiverRequest>
    {
        public Receiver Adapt(AddReceiverRequest input)
        {
            return new Receiver
            {
                CustomerId = input.CustomerId,
                RoutingNumber = input.RoutingNumber,
                FirstName = input.FirstName,
                LastName = input.LastName,
                NickName = input.NickName,
                CompanyName = input.CompanyName,
                Phone = input.PhoneNumber,
                Email = input.EmailAdress,
                BankType = input.BankType,
                AccountNumber = input.AccountNumber,
                AccountType = input.AccountType,
                ReceiverType = input.ReceiverType,
                Ownership = input.Ownership
            };
        }
    }
}
