using System.Collections.Generic;
using System.Process.Domain.Constants;
using System.Process.Infrastructure.Adapters;
using System.Process.Infrastructure.Configs;
using System.Proxy.Salesforce.GetCreditCards.Message;
using System.Proxy.Salesforce.Messages;

namespace System.Process.Application.Queries.SearchCreditCard
{
    class SearchCreditCardAdapter :
        IAdapter<GetCreditCardsParams, SearchCreditCardRequest>,
        IAdapter<SearchCreditCardResponse, QueryResult<CreditCard>>
    {
        #region Properties

        private RecordTypesConfig RecordTypesConfig { get; }

        #endregion

        #region Constructor

        public SearchCreditCardAdapter(RecordTypesConfig recordTypesConfig)
        {
            RecordTypesConfig = recordTypesConfig;
        }

        #endregion

        #region IAdapter implementation

        public GetCreditCardsParams Adapt(SearchCreditCardRequest input)
        {
            return new GetCreditCardsParams
            {
                SystemId = input.SystemId,
                RecordTypeId = RecordTypesConfig.AssetCreditCard
            };
        }

        public SearchCreditCardResponse Adapt(QueryResult<CreditCard> input)
        {
            if (input?.Records == null)
            {
                return new SearchCreditCardResponse();
            }

            var cards = new List<CreditCard>();
            foreach (var card in input?.Records)
            {
                if (card.Status != Constants.DeclinedByCustomerStatus && card.Status != Constants.DeclinedByCreditStatus)
                {
                    cards.Add(card);
                }
            }

            return new SearchCreditCardResponse
            {
                HasCard = cards.Count > 0,
                Cards = GetCards(cards)
            };
        }

        #endregion

        #region Methods

        private IList<Response.CreditCard> GetCards(IList<CreditCard> creditCards)
        {
            var creditCardsResponse = new List<Response.CreditCard>();

            if (creditCards is null || creditCards.Count.Equals(0))
            {
                return creditCardsResponse;
            }

            foreach (var card in creditCards)
            {
                creditCardsResponse.Add(new Response.CreditCard
                {
                    AssetId = card?.AssetId,
                    CreditCardType = card?.CreditCardType,
                    CreatedDate = card?.CreatedDate,
                    Status = card?.Status,
                    CreditLimit = (card == null || card.CreditLimit == null) ? 0 : card.CreditLimit.GetValueOrDefault(),
                    Description = FormatDescription(card.Product, card.Subproduct)
                });
            }

            return creditCardsResponse;
        }

        private string FormatDescription(string product, string subproduct)
        {
            return ($"{product} {subproduct}") switch
            {
                "MCB 000" => "Business Credit Card",
                "MCB VIR" => "Business Credit Card",
                "MCB 001" => "Business Credit Card",
                "MWB 000" => "World Elite Business Card",
                "MWB VIR" => "World Elite Business Card",
                _ => "",
            };
        }

        #endregion
    }
}
