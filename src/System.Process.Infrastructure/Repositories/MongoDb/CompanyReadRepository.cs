using System.Linq;
using MongoDB.Bson;
using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Phoenix.DataAccess.MongoDb;

namespace System.Process.Infrastructure.Repositories.MongoDb
{
    public class CompanyReadRepository : ICompanyReadRepository
    {
        #region Properties

        private MongoDbClient<Company, ObjectId> Client { get; }

        #endregion

        #region Constructor

        public CompanyReadRepository(MongoDbClient<Company, ObjectId> client)
        {
            Client = client;
        }

        #endregion

        #region ICompanyReadRepository implementation

        public Company FindBySystemId(string salesforcId)
        {
            return Client.Find(x => x.SystemId == salesforcId);
        }

        public Company FindByCompanyId(string companyId)
        {
            return Client.Find(x => x.CompanyId == companyId);
        }

        public Company FindByCifId(string cifId)
        {
            return Client.Find(x => x.CifId == cifId);
        }

        public Company GetLastCompanyId()
        {
            var companies = Client.GetAll();

            if (companies.Count == 0)
            {
                return companies.FirstOrDefault();
            }

            return companies.OrderByDescending(company => int.Parse(company.CompanyId)).First();
        }
        #endregion
    }
}
