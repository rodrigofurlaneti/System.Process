using System.Process.Domain.Entities;
using System.Collections.Generic;

namespace System.Process.Domain.Repositories
{
    public interface IStatementReadRepository
    {
        public List<Statement> FindBy(bool active, bool holder, bool merchant);
    }
}
