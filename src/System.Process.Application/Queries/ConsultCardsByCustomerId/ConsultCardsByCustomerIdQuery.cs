using MediatR;
using Microsoft.Extensions.Logging;
using System.Process.Application.Clients.Cards;
using System.Process.Application.DataTransferObjects;
using System.Process.Domain.Entities;
using System.Phoenix.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace System.Process.Application.Queries.ConsultCardsByCustomerId
{
    public class ConsultCardsByCustomerIdQuery : IRequestHandler<ConsultCardsByCustomerIdRequest, ConsultCardsByCustomerIdResponse>
    {
        #region Properties
        private ILogger<ConsultCardsByCustomerIdQuery> Logger { get; }
        private ICardService CardService { get; }
        #endregion

        #region Constructor

        public ConsultCardsByCustomerIdQuery(
            ILogger<ConsultCardsByCustomerIdQuery> logger,
            ICardService cardClient
        )
        {
            Logger = logger;
            CardService = cardClient;
        }

        #endregion

        #region IRequestHandler implementation

        public async Task<ConsultCardsByCustomerIdResponse> Handle(ConsultCardsByCustomerIdRequest request, CancellationToken cancellationToken)
        {
            try
            {
                // obtain card detail
                IList<Card> cards = await CardService.FindByCardType(request.CustomerId, request.CardType, cancellationToken);

                // error if no card
                if (cards.Count == 0)
                {
                    Logger.LogError("No card was found for this client");
                    throw new Exception("No card was found for this client");
                }

                // return debit card
                var cardRecords = HandleCardsDTO(cards);

                var adapter = new ConsultCardsByCustomerIdAdapter();
                var response = adapter.Adapt(cardRecords);

                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                throw new UnprocessableEntityException(ex.Message);
            }
        }

        private IList<CustomerCardDto> HandleCardsDTO(IList<Card> cards)
        {
            var cardResults = new List<CustomerCardDto> { };

            for (int i = 0; i < cards.Count; i++)
            {
                cardResults.Add(
                    new CustomerCardDto
                    {
                        AssetId = cards[i].AssetId,
                        CardId = cards[i].CardId,
                        Pan = cards[i].LastFour,
                        BusinessName = cards[i].BusinessName,
                        AccountBalance = cards[i].AccountBalance,
                        CardHolder = cards[i].CardHolder,
                        CardStatus = cards[i].CardStatus,
                        CardType = cards[i].CardType,
                        Bin = cards[i].Bin,
                        ExpirationDate = cards[i].ExpirationDate,
                        Locked = cards[i].Locked == 1
                    }
                );
            }

            return cardResults;
        }

        #endregion
    }
}