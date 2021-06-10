using System.Process.Domain.Entities;

namespace System.Process.Domain.Repositories
{
    public interface ICompanyReadRepository
    {
        Company FindBySystemId(string salesforcId);
        Company FindByCompanyId(string companyId);
        Company FindByCifId(string cifId);
        Company GetLastCompanyId();
    }
}
