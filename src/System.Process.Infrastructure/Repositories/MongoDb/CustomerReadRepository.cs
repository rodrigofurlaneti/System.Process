using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Phoenix.DataAccess.MongoDb;

namespace System.Process.Infrastructure.Repositories.MongoDb
{
    public class CustomerReadRepository : ICustomerReadRepository
    {
        #region Properties

        private MongoDbClient<Customer, string> Client { get; }

        #endregion

        #region Constructor

        public CustomerReadRepository(MongoDbClient<Customer, string> client)
        {
            Client = client;
        }

        #endregion

        #region ICustomerReadRepository implementation

        public Customer FindBy(string applicationId)
        {
            return Client.Find(x => x.ApplicationId == applicationId);
        }

        public Customer FindByCustomerId(string customerId)
        {
            return Client.Find(x => x.SalesforceId == customerId);
        }

        #endregion
    }
}
