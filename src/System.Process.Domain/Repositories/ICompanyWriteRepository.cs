using System.Process.Domain.Entities;

namespace System.Process.Domain.Repositories
{
    public interface ICompanyWriteRepository
    {
        Company Save(Company company);
        bool Update(Company company);
    }
}
