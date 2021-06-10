using System.Process.Application.DataTransferObjects;
using System.Process.Domain.Entities;
using System.Process.Infrastructure.Adapters;
using System.Collections.Generic;

namespace System.Process.Application.Queries.FindReceivers
{
    public class FindReceiversAdapter :
        IAdapter<IList<ReceiverDto>, List<Receiver>>
    {
        public IList<ReceiverDto> Adapt(List<Receiver> input)
        {
            var receiverList = new List<ReceiverDto>();

            if (input == null)
            {
                return null;
            }

            foreach (var item in input)
            {
                receiverList.Add(
                    new ReceiverDto
                    {
                        ReceiverId = item.ReceiverId.ToString(),
                        FirstName = item.FirstName,
                        LastName = item.LastName,
                        NickName = item.NickName,
                        CompanyName = item.CompanyName,
                        AccountNumber = item.AccountNumber,
                        AccountType = item.AccountType,
                        RoutingNumber = item.RoutingNumber,
                        PhoneNumber = item.Phone,
                        EmailAddress = item.Email,
                        BankType = item.BankType,
                        ReceiverType = item.ReceiverType,
                        Ownership = item.Ownership
                    }
                );
            }

            return receiverList;
        }
    }
}
