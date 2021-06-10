using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Process.Infrastructure.Data;
using System.Phoenix.DataAccess.EntityFramework;
using System.Threading;
using System.Threading.Tasks;

namespace System.Process.Infrastructure.Repositories.EntityFramework
{
    public class CardWriteRepository : GenericRepository<Card, DataCardContext>, ICardWriteRepository
    {
        public CardWriteRepository(DataCardContext context) : base(context)
        {
        }

        public new async Task Add(Card card, CancellationToken cancelToken)
        {
            await AddAsync(card, cancelToken);
            Commit();
        }

        public void Update(Card card, CancellationToken cancelToken)
        {
            Update(card);
            Commit();
        }

        public void Remove(Card card, CancellationToken cancelToken)
        {
            Remove(card);
            Commit();
        }
    }
}
