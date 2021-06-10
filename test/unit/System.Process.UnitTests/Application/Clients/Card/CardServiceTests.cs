using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Process.Application.Clients.Cards;
using System.Process.Base.IntegrationTests;
using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Process.Domain.ValueObjects;
using System.Phoenix.DataAccess.Redis;
using System.Proxy.Silverlake.Inquiry;
using System.Proxy.Silverlake.Inquiry.Common;
using System.Proxy.Silverlake.Inquiry.Messages.Request;
using System.Proxy.Silverlake.Inquiry.Messages.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace System.Process.UnitTests.Application.Clients.Cards
{
    public class CardServiceTests
    {
        #region Properties

        private IConfiguration Configuration { get; }
        private Mock<IRedisService> RedisService { get; }
        private Mock<ICardReadRepository> CardReadRepository { get; }
        private Mock<ICardWriteRepository> CardWriteRepository { get; }
        private Mock<ILogger<CardService>> Logger { get; }
        private Mock<IInquiryOperation> InquiryOperation { get; }
        private Mock<ICustomerReadRepository> CustomerReadRepository { get; }
        private Mock<IOptions<ProcessConfig>> ProcessConfig { get; }

        #endregion

        #region Constructor

        public CardServiceTests()
        {
            RedisService = new Mock<IRedisService>();
            CardReadRepository = new Mock<ICardReadRepository>();
            CardWriteRepository = new Mock<ICardWriteRepository>();
            Logger = new Mock<ILogger<CardService>>();
            InquiryOperation = new Mock<IInquiryOperation>();
            CustomerReadRepository = new Mock<ICustomerReadRepository>();
            ProcessConfig = new Mock<IOptions<ProcessConfig>>();

            var appSettings = new Dictionary<string, string>
            {
                {"Redis:expirationTime", "60"}
            };

            Configuration = new ConfigurationBuilder()
                  .AddInMemoryCollection(appSettings)
                  .Build();
        }

        #endregion

        #region Tests

        [Fact(DisplayName = "Should Find By Card Type Successfully")]
        public async Task ShouldFindByCardTypeSuccessfully()
        {
            var customerId = "SP-000";
            var cardType = "DR";

            var cancellationToken = new CancellationToken(false);

            RedisService.Setup(x => x.GetCache<IList<Card>>(It.IsAny<string>())).Returns(GetCards());
            CustomerReadRepository.Setup(x => x.FindByCustomerId(It.IsAny<string>())).Returns(GetCustomer());
            InquiryOperation.Setup(x => x.ProcessearchAsync(It.IsAny<ProcessearchRequest>(), It.IsAny<CancellationToken>())).Returns(GetProcessearchResponse());
            CardWriteRepository.Setup(x => x.Update(It.IsAny<Card>(), cancellationToken));
            RedisService.Setup(x => x.SetCache(It.IsAny<string>(), It.IsAny<IList<Card>>(), It.IsAny<TimeSpan>()));
            CardReadRepository.Setup(x => x.FindByCustomerId(It.IsAny<string>())).Returns(GetCards());
            ProcessConfig.Setup(p => p.Value).Returns(ProcessIntegrationTestsConfiguration.ReadJson<ProcessConfig>("ProcessConfig.json"));
            var cardService = new CardService(
                Configuration,
                RedisService.Object,
                CardReadRepository.Object,
                CardWriteRepository.Object,
                Logger.Object,
                InquiryOperation.Object,
                CustomerReadRepository.Object,
                ProcessConfig.Object);

            var result = await cardService.FindByCardType(customerId, cardType, cancellationToken);

            Assert.True(result.Count > 0);
        }

        [Fact(DisplayName = "Should Find By Last Four Successfully")]
        public async Task ShouldFindByLastFourSuccessfully()
        {
            var customerId = "SP-000";
            var lastFour = "1111";

            var cancellationToken = new CancellationToken(false);

            RedisService.Setup(x => x.GetCache<IList<Card>>(It.IsAny<string>())).Returns(GetCards());
            CustomerReadRepository.Setup(x => x.FindByCustomerId(It.IsAny<string>())).Returns(GetCustomer());
            InquiryOperation.Setup(x => x.ProcessearchAsync(It.IsAny<ProcessearchRequest>(), It.IsAny<CancellationToken>())).Returns(GetProcessearchResponse());
            CardWriteRepository.Setup(x => x.Update(It.IsAny<Card>(), cancellationToken));
            CardReadRepository.Setup(x => x.FindByCustomerId(It.IsAny<string>())).Returns(GetCards());
            RedisService.Setup(x => x.SetCache(It.IsAny<string>(), It.IsAny<IList<Card>>(), It.IsAny<TimeSpan>()));
            ProcessConfig.Setup(p => p.Value).Returns(ProcessIntegrationTestsConfiguration.ReadJson<ProcessConfig>("ProcessConfig.json"));
            var cardService = new CardService(
                Configuration,
                RedisService.Object,
                CardReadRepository.Object,
                CardWriteRepository.Object,
                Logger.Object,
                InquiryOperation.Object,
                CustomerReadRepository.Object,
                ProcessConfig.Object);

            var result = await cardService.FindByLastFour(customerId, lastFour, cancellationToken);

            Assert.True(result.Count > 0);
        }

        [Fact(DisplayName = "Should Find By Card Id Successfully")]
        public async Task ShouldFindByCardIdSuccessfully()
        {
            var customerId = "SP-000";
            var cardId = 1;

            var cancellationToken = new CancellationToken(false);

            RedisService.Setup(x => x.GetCache<IList<Card>>(It.IsAny<string>())).Returns(GetCards());
            CustomerReadRepository.Setup(x => x.FindByCustomerId(It.IsAny<string>())).Returns(GetCustomer());
            InquiryOperation.Setup(x => x.ProcessearchAsync(It.IsAny<ProcessearchRequest>(), It.IsAny<CancellationToken>())).Returns(GetProcessearchResponse());
            CardWriteRepository.Setup(x => x.Update(It.IsAny<Card>(), cancellationToken));
            RedisService.Setup(x => x.SetCache(It.IsAny<string>(), It.IsAny<IList<Card>>(), It.IsAny<TimeSpan>()));
            CardReadRepository.Setup(x => x.FindByCustomerId(It.IsAny<string>())).Returns(GetCards());
            ProcessConfig.Setup(p => p.Value).Returns(ProcessIntegrationTestsConfiguration.ReadJson<ProcessConfig>("ProcessConfig.json"));
            var cardService = new CardService(
                Configuration,
                RedisService.Object,
                CardReadRepository.Object,
                CardWriteRepository.Object,
                Logger.Object,
                InquiryOperation.Object,
                CustomerReadRepository.Object,
                ProcessConfig.Object);

            var result = await cardService.FindByCardId(customerId, cardId, cancellationToken);

            Assert.True(result.Count > 0);
        }

        [Fact(DisplayName = "Should Handle Card Update Successfully")]
        public async Task ShouldHandleCardUpdateSuccessfully()
        {
            var card = GetCards().FirstOrDefault();

            var cancellationToken = new CancellationToken(false);

            RedisService.Setup(x => x.GetCache<IList<Card>>(It.IsAny<string>())).Returns(GetCards());
            CustomerReadRepository.Setup(x => x.FindByCustomerId(It.IsAny<string>())).Returns(GetCustomer());
            InquiryOperation.Setup(x => x.ProcessearchAsync(It.IsAny<ProcessearchRequest>(), It.IsAny<CancellationToken>())).Returns(GetProcessearchResponse());
            CardWriteRepository.Setup(x => x.Update(It.IsAny<Card>(), cancellationToken));
            CardReadRepository.Setup(x => x.FindByCustomerId(It.IsAny<string>())).Returns(GetCards());
            RedisService.Setup(x => x.SetCache(It.IsAny<string>(), It.IsAny<IList<Card>>(), It.IsAny<TimeSpan>()));

            var cardService = new CardService(
                Configuration,
                RedisService.Object,
                CardReadRepository.Object,
                CardWriteRepository.Object,
                Logger.Object,
                InquiryOperation.Object,
                CustomerReadRepository.Object,
                ProcessConfig.Object);

            var result = await cardService.HandleCardUpdate(card, cancellationToken);

            Assert.NotNull(result);
        }

        [Fact(DisplayName = "Should Update Account Balance From Debit Card Successfully")]
        public void ShouldUpdateAccountBalanceFromDebitCardSuccessfully()
        {
            var card = GetCards();
            var customerId = "SP-000";
            var cancellationToken = new CancellationToken(false);

            RedisService.Setup(x => x.GetCache<IList<Card>>(It.IsAny<string>()));
            CustomerReadRepository.Setup(x => x.FindByCustomerId(It.IsAny<string>())).Returns(GetCustomer());
            InquiryOperation.Setup(x => x.ProcessearchAsync(It.IsAny<ProcessearchRequest>(), It.IsAny<CancellationToken>())).Returns(GetProcessearchResponse());
            CardWriteRepository.Setup(x => x.Update(It.IsAny<Card>(), cancellationToken));
            RedisService.Setup(x => x.SetCache(It.IsAny<string>(), It.IsAny<IList<Card>>(), It.IsAny<TimeSpan>()));

            var cardService = new CardService(
                Configuration,
                RedisService.Object,
                CardReadRepository.Object,
                CardWriteRepository.Object,
                Logger.Object,
                InquiryOperation.Object,
                CustomerReadRepository.Object,
                ProcessConfig.Object);

            var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;
            MethodInfo infoMethod = typeof(CardService).GetMethod("UpdateAccountBalanceFromDebitCard", bindingFlags);
            var result = infoMethod.Invoke(cardService, new object[] { card, customerId, cancellationToken });

            Assert.NotNull(result);
        }

        [Fact(DisplayName = "Should Update Account Balance From Debit Card Throw Exception")]
        public void ShouldUpdateAccountBalanceFromDebitCardThrowException()
        {
            var card = GetCards();
            var customerId = "SP-000";
            var cancellationToken = new CancellationToken(false);

            var cardService = new CardService(
                Configuration,
                RedisService.Object,
                CardReadRepository.Object,
                CardWriteRepository.Object,
                Logger.Object,
                InquiryOperation.Object,
                CustomerReadRepository.Object,
                ProcessConfig.Object);

            var bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic;
            MethodInfo infoMethod = typeof(CardService).GetMethod("UpdateAccountBalanceFromDebitCard", bindingFlags);

            var exception = Assert.ThrowsAsync<Exception>(() => Task.FromResult(infoMethod.Invoke(cardService, new object[] { card, customerId, cancellationToken })));

            Assert.NotNull(exception);
        }

        #endregion

        #region Methods

        private IList<Card> GetCards()
        {
            return new List<Card>
            {
                new Card
                {
                    CardId = 1,
                    CustomerId = "SP-000",
                    LastFour = "1111",
                    AccountBalance = "0",
                    CardType = "DR",
                    Bin = "111",
                    Pan = "11111111",
                    ExpirationDate = "2012",
                    CardHolder = "Test",
                    BusinessName = "Test",
                    Locked = 1,
                    CardStatus = "Active",
                    Validated = 1
                }
            };
        }

        private Customer GetCustomer()
        {
            return new Customer
            {
                BusinessCif = "SP-000",
                ApplicationId = "ApplicationId",
                SalesforceId = "SP-000",
                MerchantId = "0000001",
                LegalName = "Test",
                TinType = "Test",
                Tin = "Test",
                IndustryType = "Test",
                Address = null,
                BusinessDetail = null,
                Process = null,
                Shareholders = null,
                OriginChannel = OriginChannel.IsoPortal
            };
        }

        private async Task<ProcessearchResponse> GetProcessearchResponse()
        {
            return await Task.FromResult(new ProcessearchResponse
            {
                ProcessearchRecInfo = new List<ProcessearchRecInfo>
                {
                    new ProcessearchRecInfo
                    {
                        AccountId = new AccountId
                        {
                            AccountNumber = "string",
                            AccountType = "string"
                        },
                        Amount = 123,
                        Processtatus = "2",
                        AvailableBalance = 12,
                        ProductCode = "string",
                        ProductDesc = "string",
                        ProcesstatusDesc = "Closed"
                    }
                }
            });
        }

        #endregion
    }
}
