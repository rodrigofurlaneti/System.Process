using MongoDB.Bson;
using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Phoenix.DataAccess.MongoDb;

namespace System.Process.Infrastructure.Repositories.MongoDb
{
    public class CompanyWriteRepository : ICompanyWriteRepository
    {
        #region Properties

        private MongoDbClient<Company, ObjectId> Client { get; }

        #endregion

        #region Constructor

        public CompanyWriteRepository(MongoDbClient<Company, ObjectId> client)
        {
            Client = client;
        }

        #endregion

        #region ICompanyWriteRepository implementation

        public Company Save(Company company)
        {
            return Client.Insert(company);
        }

        public bool Update(Company company)
        {
            return Client.Update(company.Id, company);
        }

        #endregion
    }
}
