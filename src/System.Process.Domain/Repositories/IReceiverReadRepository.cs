using System.Process.Domain.Entities;
using System.Collections.Generic;

namespace System.Process.Domain.Repositories
{
    public interface IReceiverReadRepository
    {
        IList<Receiver> Find(int id);
        IList<Receiver> FindByCustomerId(string customerId);
        IList<Receiver> FindByBankType(string customerId, string bankType);
        IList<Receiver> FindExistent(Receiver input);
        IList<Receiver> FindByOwnerShip(string customerId, string ownership);
        IList<Receiver> FindByBankTypeAndOwnership(string customerId, string bankType, string ownership);
    }
}
