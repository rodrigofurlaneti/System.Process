using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Process.Api.Controllers.V1;
using System.Process.Application.Clients.Cards;
using System.Process.Application.Queries.SearchCreditCardsTransactions;
using System.Process.CrossCutting.DependencyInjection;
using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Process.Infrastructure.Configs;
using System.Proxy.Rtdx.GetToken;
using System.Proxy.Rtdx.GetToken.Messages;
using System.Proxy.Rtdx.Messages;
using System.Proxy.Rtdx.PendingActivityDetails;
using System.Proxy.Rtdx.PendingActivityDetails.Messages;
using System.Proxy.Rtdx.PendingActivityDetails.Messages.Results;
using System.Proxy.Rtdx.TransactionDetails;
using System.Proxy.Rtdx.TransactionDetails.Messages;
using System.Proxy.Rtdx.TransactionDetails.Messages.Result;
using Xunit;
using RtdxGetToken = System.Proxy.Rtdx.GetToken.Messages;

namespace System.Process.IntegrationTests.Application.Queries.SearchCreditCardsTransactions
{
    public class SearchCreditCardsTransactionsQueryTests
    {
        #region Properties

        private static ServiceProvider Provider;
        private static Mock<IHttpContextAccessor> HttpContextAccessor;
        private static ServiceCollection Services;

        private Mock<ILogger<SearchCreditCardsTransactionsQuery>> Logger { get; set; }
        private IOptions<RecordTypesConfig> RecordTypesConfig { get; set; }
        private Mock<ITransactionDetailsOperation> GetTransactionDetailsOperation { get; set; }
        private Mock<IPendingActivityDetailsOperation> GetPendingActivityDetailsOperation { get; set; }
        private Mock<ICardReadRepository> CardReadRepository { get; set; }
        private Mock<ICardService> CardService { get; set; }
        private Mock<IGetTokenOperation> GetTokenOperation { get; set; }
        private IOptions<RtdxGetToken.GetTokenParams> RtdxTokenParams { get; set; }

        #endregion

        #region Constructor

        static SearchCreditCardsTransactionsQueryTests()
        {
            Services = new ServiceCollection();
            Services.AddMediator();
            Services.AddScoped(typeof(ILoggerFactory), typeof(LoggerFactory));
            Services.AddScoped(typeof(ILogger<>), typeof(Logger<>));

            Provider = Services.BuildServiceProvider();
        }

        #endregion

        #region Main Flow

        [Fact(DisplayName = "Main Flow - Success")]
        public async void ShouldSearchCreditCardsTransactionsSuccessfully()
        {
            Logger = new Mock<ILogger<SearchCreditCardsTransactionsQuery>>();
            var config = new RecordTypesConfig
            {
                AssetCreditCard = "test"
            };
            CardService = new Mock<ICardService>();
            RecordTypesConfig = Options.Create(config);
            CardReadRepository = new Mock<ICardReadRepository>();
            GetTransactionDetailsOperation = new Mock<ITransactionDetailsOperation>();
            GetPendingActivityDetailsOperation = new Mock<IPendingActivityDetailsOperation>();
            GetTransactionDetailsOperation.Setup(x => x.TransactionDetailsAsync(It.IsAny<TransactionDetailsParams>(), It.IsAny<CancellationToken>())).Returns(GetTransactionDetail());
            GetPendingActivityDetailsOperation.Setup(x => x.PendingActivityDetailsAsync(It.IsAny<PendingActivityDetailsParams>(), It.IsAny<CancellationToken>())).Returns(GetPendingActivityDetails());
            CardReadRepository.Setup(x => x.Find(It.IsAny<int>())).Returns(GetCreditCard());
            GetTokenOperation = new Mock<IGetTokenOperation>();
            GetTokenOperation.Setup(x => x.GetTokenAsync(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(GetRtdxToken());
            var configRtdx = new RtdxGetToken.GetTokenParams
            {
                CorpId = "string",
                ExternalTraceId = "string",
                Application = "",
                NewPassword = "",
                System = new List<RtdxGetToken.Params.SystemParams>
                {
                    new RtdxGetToken.Params.SystemParams
                    {
                        Password = "string",
                        SysFlag = "B",
                        UserId = "string"
                    }
                }
            };
            RtdxTokenParams = Options.Create(configRtdx);

            Services.AddSingleton(Logger.Object);
            Services.AddSingleton(RecordTypesConfig);
            Services.AddSingleton(CardReadRepository.Object);
            Services.AddSingleton(GetTransactionDetailsOperation.Object);
            Services.AddSingleton(GetPendingActivityDetailsOperation.Object);
            Services.AddSingleton(GetTokenOperation.Object);
            Services.AddSingleton(RtdxTokenParams);

            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<CardsController>>();
            var mediator = GetInstance<IMediator>();
            var controller = new CardsController(mediator, logger.Object, CardService.Object);

            var result = await controller.SearchCreditCardsTransactions("1", new CancellationToken());

            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region Methods

        public static T GetInstance<T>()
        {
            T result = Provider.GetRequiredService<T>();
            ControllerBase controllerBase = result as ControllerBase;
            if (controllerBase != null)
            {
                SetControllerContext(controllerBase);
            }
            Controller controller = result as Controller;
            if (controller != null)
            {
                SetControllerContext(controller);
            }
            return result;
        }

        private static void SetControllerContext(Controller controller)
        {
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = HttpContextAccessor.Object.HttpContext
            };
        }

        private static void SetControllerContext(ControllerBase controller)
        {
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = HttpContextAccessor.Object.HttpContext
            };
        }

        private Task<BaseResult<GetTokenResult>> GetRtdxToken()
        {
            return Task.FromResult(new BaseResult<GetTokenResult>
            {
                IsSuccess = true,
                Result = new GetTokenResult
                {
                    SecurityToken = "test"
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
                                AuthReason = 1,
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

        #endregion
    }
}