using System.Process.Domain.Entities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace System.Process.Application.Clients.Cards
{
    public interface ICardService
    {
        Task<IList<Card>> FindByCardType(string customerId, string cardType, CancellationToken cancellationToken);
        Task<IList<Card>> FindByLastFour(string customerId, string lastFour, CancellationToken cancellationToken);
        Task<IList<Card>> FindByCardId(string customerId, int cardId, CancellationToken cancellationToken);
        Task<Card> HandleCardUpdate(Card card, CancellationToken cancellationToken);
    }
}
