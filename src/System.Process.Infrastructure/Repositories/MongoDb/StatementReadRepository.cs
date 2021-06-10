using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Phoenix.DataAccess.MongoDb;
using System.Collections.Generic;

namespace System.Process.Infrastructure.Repositories.MongoDb
{
    public class StatementReadRepository : IStatementReadRepository
    {
        #region Properties

        private MongoDbClient<Statement, string> Client { get; }

        #endregion

        #region Constructor

        public StatementReadRepository(MongoDbClient<Statement, string> client)
        {
            Client = client;
        }

        #endregion

        #region IStatementReadRepository implementation

        public List<Statement> FindBy(bool active, bool holder, bool merchant)
        {
            return Client.FindAll(x => x.Active == active && (x.Holder == holder || x.Merchant == merchant));
        }

        #endregion
    }
}
