using System.Process.Domain.Entities;
using System.Collections.Generic;

namespace System.Process.Domain.Repositories
{
    public interface ICardReadRepository
    {
        IList<Card> Find(int id);
        IList<Card> FindByCustomerId(string customerId);
        IList<Card> FindByLastFour(string customerId, string lastFour);
        IList<Card> FindExistent(Card input);
        Card FindByAssetId(string assetId);
        Card FindByAssetIdCustomerId(string assetId, string customerId);
        Card FindByCardId(int id);
    }
}
