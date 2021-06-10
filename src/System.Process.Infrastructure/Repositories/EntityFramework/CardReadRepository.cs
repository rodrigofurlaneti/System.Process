using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Process.Infrastructure.Data;
using System.Phoenix.DataAccess.EntityFramework;
using System.Collections.Generic;
using System.Linq;

namespace System.Process.Infrastructure.Repositories.EntityFramework
{
    public class CardReadRepository : GenericRepository<Card, DataCardContext>, ICardReadRepository
    {
        public CardReadRepository(DataCardContext context) : base(context)
        {
        }

        public IList<Card> Find(int idCard)
        {
            return Find(r => r.CardId == idCard);
        }

        public IList<Card> FindByCustomerId(string customerId)
        {
            return Find(r => r.CustomerId == customerId);
        }

        public Card FindByAssetId(string assetId)
        {
            return Find(r => r.AssetId == assetId).FirstOrDefault();
        }

        public IList<Card> FindByLastFour(string customerId, string lastFour)
        {
            return Find(r => r.CustomerId == customerId && r.LastFour == lastFour);
        }

        public IList<Card> FindExistent(Card input)
        {
            return Find(r =>
                r.CustomerId == input.CustomerId &&
                r.Pan == input.Pan &&
                r.CardStatus == input.CardStatus
              );
        }

        public Card FindByAssetIdCustomerId(string assetId, string customerId)
        {
            return Find(r => r.AssetId == assetId && r.CustomerId == customerId).OrderByDescending(r => r.CustomerId).FirstOrDefault();
        }

        public Card FindByCardId(int id)
        {
            return Find(r => r.CardId == id).FirstOrDefault();
        }
    }
}
