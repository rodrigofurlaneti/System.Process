using System.Process.Domain.Entities;

namespace System.Process.Domain.Repositories
{
    public interface ICustomerReadRepository
    {
        Customer FindBy(string applicationId);
        Customer FindByCustomerId(string customerId);
    }
}
