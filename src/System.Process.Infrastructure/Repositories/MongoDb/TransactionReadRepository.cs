using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Phoenix.DataAccess.MongoDb;
using System.Collections.Generic;

namespace System.Process.Infrastructure.Repositories.MongoDb
{
    public class TransactionReadRepository : ITransactionReadRepository
    {
        #region Properties

        private MongoDbClient<Transaction, string> Client { get; }

        #endregion

        #region Constructor

        public TransactionReadRepository(MongoDbClient<Transaction, string> client)
        {
            Client = client;
        }

        #endregion

        #region ITransactionReadRepository implementation

        public Transaction FindBy(string groupName)
        {
            return Client.Find(x => x.Label == groupName);
        }

        public IList<Transaction> Find()
        {
            return Client.GetAll();
        }

        #endregion
    }
}
