using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Process.Application.Queries.ConsultProcessByCustomerId;
using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Process.Domain.ValueObjects;
using System.Phoenix.DataAccess.Redis;
using System.Proxy.Silverlake.Inquiry;
using System.Proxy.Silverlake.Inquiry.Messages.Response;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace System.Process.Application.Clients.Cards
{
    public class CardService : ICardService
    {
        private IConfiguration Configuration { get; }
        private IRedisService RedisService { get; }
        private ICardReadRepository CardReadRepository { get; }
        private ICardWriteRepository CardWriteRepository { get; }
        private ILogger<CardService> Logger { get; }
        private IInquiryOperation InquiryOperation { get; }
        private ICustomerReadRepository CustomerReadRepository { get; }
        private ProcessConfig ProcessConfig { get; set; }

        public CardService(
            IConfiguration configuration,
            IRedisService redisService,
            ICardReadRepository cardReadRepository,
            ICardWriteRepository cardWriteRepository,
            ILogger<CardService> logger,
            IInquiryOperation inquiryOperation,
            ICustomerReadRepository customerReadRepository,
            IOptions<ProcessConfig> ProcessConfig
        )
        {
            Configuration = configuration;
            RedisService = redisService;
            CardReadRepository = cardReadRepository;
            CardWriteRepository = cardWriteRepository;
            Logger = logger;
            InquiryOperation = inquiryOperation;
            CustomerReadRepository = customerReadRepository;
            ProcessConfig = ProcessConfig.Value;
        }

        public async Task<IList<Card>> FindByCardType(string customerId, string cardType, CancellationToken cancellationToken)
        {
            return await FindCard(customerId, (card) => String.Compare(card.CardType, cardType, true) == 0, cancellationToken);
        }

        public async Task<IList<Card>> FindByLastFour(string customerId, string lastFour, CancellationToken cancellationToken)
        {
            return await FindCard(customerId, (card) => String.Compare(card.LastFour, lastFour, true) == 0, cancellationToken); ;
        }

        public async Task<IList<Card>> FindByCardId(string customerId, int cardId, CancellationToken cancellationToken)
        {
            return await FindCard(customerId, (card) => card.CardId == cardId, cancellationToken);
        }

        private async Task<IList<Card>> FindCard(string customerId, Func<Card, bool> condition, CancellationToken cancellationToken)
        {
            Logger.LogInformation($"Start FindCard");
            var cards = CardReadRepository.FindByCustomerId(customerId);
            var updatedCards = await UpdateAccountBalanceFromDebitCard(cards, customerId, cancellationToken);

            var cardRecords = FilterCardsByCallback(updatedCards, condition);

            return cardRecords;
        }

        private async Task<IList<Card>> UpdateAccountBalanceFromDebitCard(IList<Card> cards, string customerId, CancellationToken cancellationToken)
        {

            Logger.LogInformation($"Start UpdateAccountBalanceFromDebitCard");
            if (HasDebitCardOnList(cards))
            {
                ProcessearchResponse accountInformationResponse;
                ConsultProcessByCustomerIdResponse accountInformation;
                Customer customer = new Customer();
                var consultProcessAdapter = new ConsultProcessByCustomerIdAdapter(ProcessConfig);
                var cardResults = new List<Card> { };

                try
                {

                    Logger.LogInformation($"Start get customer by customerId");
                    // fetch BusinessCif on mongo
                    customer = CustomerReadRepository.FindByCustomerId(customerId);
                    // fetch account ballance
                    // TODO REMOVE MOCK WHEN WE HAVE A MASS
                    Logger.LogInformation($"Start Adapt consultProcessOptions");
                    var consultProcessOptions = consultProcessAdapter.Adapt(customer.BusinessCif);

                    Logger.LogInformation($"Start ProcessearchAsync");
                    accountInformationResponse = await InquiryOperation.ProcessearchAsync(consultProcessOptions, cancellationToken);
                    Logger.LogInformation($"Start Adapt accountInformation");
                    accountInformation = consultProcessAdapter.Adapt(accountInformationResponse);
                }
                catch (Exception ex)
                {
                    // error if doesn't have account ballance
                    Logger.LogError(ex, ex.Message);
                    throw new Exception("Could not get account balance", ex);
                }


                Logger.LogInformation($"Start UpdateCardOnOracle");
                for (int i = 0; i < cards.Count; i++)
                {
                    cardResults.Add(cards[i]);
                    if (cardResults[i].CardType == "DR")
                    {
                        cardResults[i].AccountBalance = accountInformation.TotalProcessBalance.ToString();
                        UpdateCardOnOracle(cardResults[i], cancellationToken);
                    }
                }

                UpdateCardsOnRedis(cardResults, customerId);

                return cardResults;
            }

            return cards;
        }

        public async Task<Card> HandleCardUpdate(Card card, CancellationToken cancellationToken)
        {
            Logger.LogInformation($"Start HandleCardUpdate");
            var cards = CardReadRepository.FindByCustomerId(card.CustomerId);
            var updatedResultOnCards = FindCardByCallbackAndReplace(cards, card, (c) => card.CardId == c.CardId);

            UpdateCardOnOracle(card, cancellationToken);
            UpdateCardsOnRedis(updatedResultOnCards, card.CustomerId);

            return card;
        }

        private void UpdateCardOnOracle(Card card, CancellationToken cancellationToken)
        {
            Logger.LogInformation($"Start UpdateCardOnOracle");
            CardWriteRepository.Update(card, cancellationToken);
        }

        private void UpdateCardsOnRedis(IList<Card> cards, string customerId)
        {
            Logger.LogInformation($"Start UpdateCardsOnRedis");
            var redisKey = string.Concat(customerId, "-cards");
            var timespan = TimeSpan.FromSeconds(Int64.Parse(Configuration.GetSection("Redis:expirationTime").Value));
            RedisService.SetCache(redisKey, cards, timespan);
        }

        private IList<Card> FilterCardsByCallback(IList<Card> cards, Func<Card, bool> condition)
        {
            Logger.LogInformation($"Start FilterCardsByCallback");
            var cardResults = new List<Card> { };

            for (int i = 0; i < cards.Count; i++)
            {
                if (condition(cards[i]))
                {
                    cardResults.Add(cards[i]);
                }
            }

            return cardResults;
        }

        private IList<Card> FindCardByCallbackAndReplace(IList<Card> cards, Card cardToReplace, Func<Card, bool> condition)
        {

            Logger.LogInformation($"Start FindCardByCallbackAndReplace");
            var cardResults = new List<Card> { };

            for (int i = 0; i < cards.Count; i++)
            {
                Card card = condition(cards[i]) ? cardToReplace : cards[i];
                cardResults.Add(card);
            }

            return cardResults;
        }

        private bool HasDebitCardOnList(IList<Card> cards)
        {
            Logger.LogInformation($"Start HasDebitCardOnList");
            for (int i = 0; i < cards.Count; i++)
            {
                if (cards[i].CardType == "DR")
                {
                    return true;
                }
            }

            return false;
        }
    }
}
