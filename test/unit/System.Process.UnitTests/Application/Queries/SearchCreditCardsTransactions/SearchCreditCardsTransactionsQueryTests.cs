using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using System.Process.Application.Queries.SearchCreditCardsTransactions;
using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Process.Infrastructure.Configs;
using System.Process.UnitTests.Common;
using System.Phoenix.Common.Exceptions;
using System.Proxy.Rtdx.GetToken;
using System.Proxy.Rtdx.Messages;
using System.Proxy.Rtdx.PendingActivityDetails;
using System.Proxy.Rtdx.PendingActivityDetails.Messages;
using System.Proxy.Rtdx.PendingActivityDetails.Messages.Results;
using System.Proxy.Rtdx.TransactionDetails;
using System.Proxy.Rtdx.TransactionDetails.Messages;
using System.Proxy.Rtdx.TransactionDetails.Messages.Result;
using System.Proxy.Salesforce.GetToken.Messages;
using Xunit;
using RtdxGetToken = System.Proxy.Rtdx.GetToken.Messages;

namespace System.Process.UnitTests.Application.Queries.SearchCreditCardsTransactions
{
    public class SearchCreditCardsTransactionsQueryTests
    {

        #region Properties

        private Mock<ILogger<SearchCreditCardsTransactionsQuery>> Logger { get; set; }
        private IOptions<RecordTypesConfig> RecordTypesConfig { get; set; }
        private Mock<ITransactionDetailsOperation> GetTransactionDetailsOperation { get; set; }
        private Mock<IPendingActivityDetailsOperation> GetPendingActivityDetailsOperation { get; set; }
        private Mock<ICardReadRepository> CardReadRepository { get; set; }
        private Mock<IGetTokenOperation> GetTokenOperation { get; }
        private IOptions<RtdxGetToken.GetTokenParams> RtdxTokenParams { get; }

        #endregion

        #region Constructor

        public SearchCreditCardsTransactionsQueryTests()
        {
            Logger = new Mock<ILogger<SearchCreditCardsTransactionsQuery>>();
            var config = new RecordTypesConfig
            {
                AssetCreditCard = "test"
            };
            RecordTypesConfig = Options.Create(config);
            GetTransactionDetailsOperation = new Mock<ITransactionDetailsOperation>();
            GetPendingActivityDetailsOperation = new Mock<IPendingActivityDetailsOperation>();
            CardReadRepository = new Mock<ICardReadRepository>();
            GetTokenOperation = new Mock<IGetTokenOperation>();
            var configRtdx = new RtdxGetToken.GetTokenParams();
            RtdxTokenParams = Options.Create(configRtdx);
        }

        #endregion

        #region Tests

        [Fact(DisplayName = "Should Handle Search Credit Card Successfully")]
        public async void ShouldSearchCreditCardSuccessfully()
        {
            CardReadRepository.Setup(x => x.Find(It.IsAny<int>())).Returns(GetCreditCard());
            GetTransactionDetailsOperation.Setup(x => x.TransactionDetailsAsync(It.IsAny<TransactionDetailsParams>(), It.IsAny<CancellationToken>())).Returns(GetTransactionDetail());
            GetPendingActivityDetailsOperation.Setup(x => x.PendingActivityDetailsAsync(It.IsAny<PendingActivityDetailsParams>(), It.IsAny<CancellationToken>())).Returns(GetPendingActivityDetails());
            GetTokenOperation.Setup(x => x.GetTokenAsync(It.IsAny<RtdxGetToken.GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(GetTokenResult());
            var searchCreditCardQuery = new SearchCreditCardsTransactionsQuery(Logger.Object, CardReadRepository.Object, RecordTypesConfig, GetTransactionDetailsOperation.Object, GetPendingActivityDetailsOperation.Object, GetTokenOperation.Object, RtdxTokenParams);

            var result = await searchCreditCardQuery.Handle(GetSearchCreditCardsTransactionsRequest(), new CancellationToken());
            var teste = JsonConvert.SerializeObject(result);

            result.Should().NotBeNull();
        }

        [Fact(DisplayName = "Should Handle Search Credit Card Throws UnprocessableEntityException")]
        public async void ShouldSearchCreditCardError()
        {
            var searchCreditCardQuery = new SearchCreditCardsTransactionsQuery(Logger.Object, CardReadRepository.Object, RecordTypesConfig, GetTransactionDetailsOperation.Object, GetPendingActivityDetailsOperation.Object, GetTokenOperation.Object, RtdxTokenParams);

            await Assert.ThrowsAsync<UnprocessableEntityException>(() => searchCreditCardQuery.Handle(GetSearchCreditCardsTransactionsRequest(), new CancellationToken()));
        }

        #endregion

        #region Methods
        private Task<Proxy.Salesforce.Messages.BaseResult<GetTokenResult>> GetSalesforceToken()
        {
            return Task.FromResult(new Proxy.Salesforce.Messages.BaseResult<GetTokenResult>
            {
                IsSuccess = true,
                Result = new GetTokenResult
                {
                    AccessToken = "test"
                }
            });
        }

        private Task<BaseResult<TransactionDetailsResult>> GetTransactionDetail()
        {
            return Task.FromResult(new BaseResult<TransactionDetailsResult>
            {
                IsSuccess = true,
                Message = "",
                Result = new TransactionDetailsResult()
                {
                    AccountNumber = "AccountNumber",
                    Message = "Message",
                    ResponseCode = "ResponseCode",
                    StatementLineCount = 1,
                    StatementLines = new List<StatementLines> {
                        new StatementLines
                        {
                            ItemAmount = 12,
                            MerchantCategoryCode = "21",
                            MessageText12 = "MessageText12",
                            MessageText11 = "MessageText11",
                            MessageText10 = "MessageText10",
                            MessageText9 = "MessageText9",
                            MessageText8 = "MessageText8",
                            MessageText7 = "MessageText7",
                            MessageText6 = "MessageText6",
                            MessageText5 = "MessageText5",
                            MessageText4 = "MessageText4",
                            MessageText3 = "MessageText3",
                            MessageText2 = "MessageText2",
                            MessageText1 = "MessageText1",
                            PostedDate = "20201010",
                            ReasonCode = "1",
                            ReferenceNumber = "1",
                            TransactionCode = "5",
                            TransactionDate = "20201009"
                        }
                    }
                }
            });
        }

        private Task<BaseResult<PendingActivityDetailsResult>> GetPendingActivityDetails()
        {
            return Task.FromResult(new BaseResult<PendingActivityDetailsResult>
            {
                IsSuccess = true,
                Message = "",
                Result = new PendingActivityDetailsResult()
                {
                    AccountNumber = "AccountNumber",
                    Message = "Message",
                    ResponseCode = "ResponseCode",
                    PendingAuthsAmount = "1",
                    PendingAuthsCount = 0,
                    TransactionRecordCount = 1,
                    UnmatchedPaymentAuthAmount = "1",
                    UnmatchedPaymentAuthNumber = 0,
                    Authorizations = new Authorizations()
                    {
                        AuthorizationItems = new List<AuthorizationItems> {
                            new AuthorizationItems
                            {
                                AuthorizationAmount = "1",
                                AuthorizationDate = 20201010,
                                AuthorizationNumber= 1,
                                AuthorizationTime = 1010,
                                AuthReason = 0,
                                AuthResponse = "AuthResponse",
                                MerchantCategoryCode = "MerchantCategoryCode",
                                MerchantGroup = "MerchantGroup",
                                MerchantId = "1"

                            }
                        }
                    }
                }
            });
        }

        private IList<Card> GetCreditCard()
        {

            return new List<Card> {
                        new Card
                        {
                            AccountBalance = "AccountBalance",
                            AssetId = "AssetId",
                            Bin = "Bin",
                            BusinessName  = "BusinessName",
                            CardHolder  = "CardHolder",
                            CardId  = 1,
                            CardStatus  = "CardStatus",
                            CardType  = "CardType",
                            CustomerId  = "CustomerId",
                            ExpirationDate = "ExpirationDate",
                            LastFour = "LastFour",
                            Locked  = 1,
                            Pan = "Pan",
                            Validated  = 1,
                        }
                    };
        }

        private SearchCreditCardsTransactionsRequest GetSearchCreditCardsTransactionsRequest()
        {
            return new SearchCreditCardsTransactionsRequest { CardId = "1" };
        }

        private Task<BaseResult<RtdxGetToken.GetTokenResult>> GetTokenResult()
        {
            return Task.FromResult(new BaseResult<RtdxGetToken.GetTokenResult>
            {
                IsSuccess = true,
                Result = ConvertJson.ReadJson<RtdxGetToken.GetTokenResult>("RtdxGetTokenResult.json")
            });
        }

        #endregion
    }
}
