using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Process.Infrastructure.Data;
using System.Phoenix.DataAccess.EntityFramework;
using System.Collections.Generic;

namespace System.Process.Infrastructure.Repositories.EntityFramework
{
    public class ReceiverReadRepository : GenericRepository<Receiver, DataContext>, IReceiverReadRepository
    {
        public ReceiverReadRepository(DataContext context) : base(context)
        {
        }

        public IList<Receiver> Find(int id)
        {
            return Find(r => r.ReceiverId == id);
        }

        public IList<Receiver> FindByCustomerId(string customerId)
        {
            return Find(r => r.CustomerId == customerId);
        }

        public IList<Receiver> FindByBankType(string customerId, string bankType)
        {
            return Find(r => r.CustomerId == customerId && r.BankType == bankType);
        }

        public IList<Receiver> FindByOwnerShip(string customerId, string ownership)
        {
            return Find(r => r.CustomerId == customerId && r.Ownership == ownership);
        }

        public IList<Receiver> FindByBankTypeAndOwnership(string customerId, string bankType, string ownership)
        {
            return Find(r => r.CustomerId == customerId && r.BankType == bankType && r.Ownership == ownership);
        }

        public IList<Receiver> FindExistent(Receiver input)
        {
            return Find(r =>
                r.CustomerId == input.CustomerId &&
                r.AccountNumber == input.AccountNumber &&
                r.RoutingNumber == input.RoutingNumber
              );
        }
    }
}
