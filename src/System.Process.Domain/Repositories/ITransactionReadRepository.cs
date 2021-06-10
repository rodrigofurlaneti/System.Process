using System.Process.Domain.Entities;
using System.Collections.Generic;

namespace System.Process.Domain.Repositories
{
    public interface ITransactionReadRepository
    {
        Transaction FindBy(string groupName);
        IList<Transaction> Find();
    }
}
