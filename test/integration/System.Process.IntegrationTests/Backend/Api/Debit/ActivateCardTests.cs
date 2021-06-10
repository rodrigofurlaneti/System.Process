using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Process.Api.Controllers.V1;
using System.Process.Application.Clients.Cards;
using System.Process.Application.Commands.ActivateCard;
using System.Process.CrossCutting.DependencyInjection;
using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Phoenix.Common.Exceptions;
using System.Phoenix.DataAccess.Redis;
using System.Phoenix.Web.Filters;
using System.Proxy.Fis.GetToken;
using System.Proxy.Fis.GetToken.Messages;
using System.Proxy.Fis.Messages;
using System.Proxy.Salesforce;
using System.Proxy.Salesforce.UpdateAsset;
using System.Proxy.Salesforce.UpdateAsset.Messages;
using System.Proxy.Silverlake.Inquiry;
using System.Proxy.Silverlake.Inquiry.Common;
using System.Proxy.Silverlake.Inquiry.Messages.Request;
using System.Proxy.Silverlake.Inquiry.Messages.Response;
using Xunit;

namespace System.Process.IntegrationTests.Backend.Api.Debit
{
    public class ActivateCardTests
    {
        #region Properties

        private static ServiceProvider Provider;
        private static Mock<IHttpContextAccessor> HttpContextAccessor;
        private static ServiceCollection Services;
        private Mock<IGetTokenClient> GetTokenClient;
        private Mock<IOptions<GetTokenParams>> GetTokenParams;
        private Mock<IRedisService> RedisService;
        private Mock<IInquiryOperation> InquiryOperation;
        private Mock<ICardReadRepository> CardReadRepository;
        private Mock<ICardWriteRepository> CardWriteRepository;
        private Mock<ICustomerReadRepository> CustomerReadRepository;
        private IConfiguration Configuration;
        private Mock<IUpdateAssetClient> UpdateAssetClient;
        private Mock<IOptions<GetTokenParams>> ConfigSalesforce;
        private Mock<Proxy.Salesforce.GetToken.IGetTokenClient> TokenClient;


        #endregion

        #region Constructor

        static ActivateCardTests()
        {
            Services = new ServiceCollection();
            Services.AddMvc(options => options.Filters.Add(new ValidationFilterAttribute()))
               .AddFluentValidation(options =>
               {
                   options.RegisterValidatorsFromAssemblyContaining<ActivateCardAttributesValidator>();
               });
            Services.AddMediator();
            Services.AddScoped(typeof(ILoggerFactory), typeof(LoggerFactory));
            Services.AddScoped(typeof(ILogger<>), typeof(Logger<>));
            Services.AddScoped(typeof(ICardService), typeof(CardService));

            Provider = Services.BuildServiceProvider();
        }

        #endregion

        #region Tests

        [Fact(DisplayName = "Main Flow - Success")]
        public async void ShouldActivateCardSuccessfully()
        {
            var request = new ActivateCardRequest
            {
                CardId = 1,
                CustomerId = "SP-000",
                Pan = "1111",
                ExpireDate = "2012"
            };

            var cancellationToken = new CancellationToken();
            GetTokenClient = new Mock<IGetTokenClient>();
            GetTokenParams = new Mock<IOptions<GetTokenParams>>();
            RedisService = new Mock<IRedisService>();
            InquiryOperation = new Mock<IInquiryOperation>();
            CardReadRepository = new Mock<ICardReadRepository>();
            CardWriteRepository = new Mock<ICardWriteRepository>();
            CustomerReadRepository = new Mock<ICustomerReadRepository>();
            UpdateAssetClient = new Mock<IUpdateAssetClient>();
            ConfigSalesforce = new Mock<IOptions<GetTokenParams>>();
            TokenClient = new Mock<Proxy.Salesforce.GetToken.IGetTokenClient>();

            var cards = GetCards();
            cards[0].CardStatus = "Pending Activation";

            GetTokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(GetBaseTokenResult()));
            RedisService.Setup(x => x.GetCache<IList<Card>>(It.IsAny<string>())).Returns(cards);
            CardReadRepository.Setup(x => x.FindByCustomerId(It.IsAny<string>())).Returns(cards);
            CustomerReadRepository.Setup(x => x.FindByCustomerId(It.IsAny<string>())).Returns(GetCustomer());
            InquiryOperation.Setup(x => x.ProcessearchAsync(It.IsAny<ProcessearchRequest>(), It.IsAny<CancellationToken>())).Returns(GetProcessearchResponse());
            CardWriteRepository.Setup(x => x.Update(It.IsAny<Card>(), cancellationToken));
            RedisService.Setup(x => x.SetCache(It.IsAny<string>(), It.IsAny<IList<Card>>(), It.IsAny<TimeSpan>()));
            TokenClient.Setup(x => x.GetToken(It.IsAny<Proxy.Salesforce.GetToken.Messages.GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(GetTokenBaseResult());
            UpdateAssetClient.Setup(x => x.UpdateAsset(It.IsAny<UpdateAssetParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetSalesforceResult());

            var appSettings = new Dictionary<string, string> { { "Redis:expirationTime", "60" } };

            Configuration = new ConfigurationBuilder().AddInMemoryCollection(appSettings).Build();

            Services.AddSingleton(GetTokenClient.Object);
            Services.AddSingleton(GetTokenParams.Object);
            Services.AddSingleton(RedisService.Object);
            Services.AddSingleton(InquiryOperation.Object);
            Services.AddSingleton(CardReadRepository.Object);
            Services.AddSingleton(CardWriteRepository.Object);
            Services.AddSingleton(CustomerReadRepository.Object);
            Services.AddSingleton(Configuration);
            Services.AddSingleton(TokenClient.Object);
            Services.AddSingleton(UpdateAssetClient.Object);
            Services.AddSingleton(ConfigSalesforce.Object);

            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<CardsController>>();
            var mediator = GetInstance<IMediator>();
            var cardService = GetInstance<ICardService>();
            var controller = new CardsController(mediator, logger.Object, cardService);

            var validate = new ActivateCardAttributesValidator();
            validate.Validate(request);

            var result = await controller.ActivateCard(request, cancellationToken);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact(DisplayName = "Should throw Unprocessable Entity For Card Already Active")]
        public async void ShouldThrowUnprocessableExceptionForCardAlreadyActive()
        {
            var request = new ActivateCardRequest
            {
                CardId = 1,
                CustomerId = "SP-000",
                Pan = "1111",
                ExpireDate = "2012"
            };

            var cancellationToken = new CancellationToken();
            GetTokenClient = new Mock<IGetTokenClient>();
            GetTokenParams = new Mock<IOptions<GetTokenParams>>();
            RedisService = new Mock<IRedisService>();
            InquiryOperation = new Mock<IInquiryOperation>();
            CardReadRepository = new Mock<ICardReadRepository>();
            CardWriteRepository = new Mock<ICardWriteRepository>();
            CustomerReadRepository = new Mock<ICustomerReadRepository>();
            UpdateAssetClient = new Mock<IUpdateAssetClient>();
            ConfigSalesforce = new Mock<IOptions<GetTokenParams>>();
            TokenClient = new Mock<Proxy.Salesforce.GetToken.IGetTokenClient>();

            var cards = GetCards();

            GetTokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(GetBaseTokenResult()));
            RedisService.Setup(x => x.GetCache<IList<Card>>(It.IsAny<string>())).Returns(cards);
            CustomerReadRepository.Setup(x => x.FindByCustomerId(It.IsAny<string>())).Returns(GetCustomer());
            InquiryOperation.Setup(x => x.ProcessearchAsync(It.IsAny<ProcessearchRequest>(), It.IsAny<CancellationToken>())).Returns(GetProcessearchResponse());
            CardWriteRepository.Setup(x => x.Update(It.IsAny<Card>(), cancellationToken));
            RedisService.Setup(x => x.SetCache(It.IsAny<string>(), It.IsAny<IList<Card>>(), It.IsAny<TimeSpan>()));
            TokenClient.Setup(x => x.GetToken(It.IsAny<Proxy.Salesforce.GetToken.Messages.GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(GetTokenBaseResult());
            UpdateAssetClient.Setup(x => x.UpdateAsset(It.IsAny<UpdateAssetParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetSalesforceResult());

            var appSettings = new Dictionary<string, string> { { "Redis:expirationTime", "60" } };

            Configuration = new ConfigurationBuilder().AddInMemoryCollection(appSettings).Build();

            Services.AddSingleton(GetTokenClient.Object);
            Services.AddSingleton(GetTokenParams.Object);
            Services.AddSingleton(RedisService.Object);
            Services.AddSingleton(InquiryOperation.Object);
            Services.AddSingleton(CardReadRepository.Object);
            Services.AddSingleton(CardWriteRepository.Object);
            Services.AddSingleton(CustomerReadRepository.Object);
            Services.AddSingleton(Configuration);
            Services.AddSingleton(TokenClient.Object);
            Services.AddSingleton(UpdateAssetClient.Object);
            Services.AddSingleton(ConfigSalesforce.Object);

            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<CardsController>>();
            var mediator = GetInstance<IMediator>();
            var cardService = GetInstance<ICardService>();
            var controller = new CardsController(mediator, logger.Object, cardService);

            var validate = new ActivateCardAttributesValidator();
            validate.Validate(request);

            var result = await Assert.ThrowsAsync<UnprocessableEntityException>(() => controller.ActivateCard(request, cancellationToken));

            Assert.IsType<UnprocessableEntityException>(result);
        }

        [Fact(DisplayName = "Should throw Unprocessable Entity For Card Wrong Information Provided")]
        public async void ShouldThrowUnprocessableExceptionForCardWrongInformation()
        {
            var request = new ActivateCardRequest
            {
                CardId = 1,
                CustomerId = "SP-001",
                Pan = "1112",
                ExpireDate = "2013"
            };

            var cancellationToken = new CancellationToken();
            GetTokenClient = new Mock<IGetTokenClient>();
            GetTokenParams = new Mock<IOptions<GetTokenParams>>();
            RedisService = new Mock<IRedisService>();
            InquiryOperation = new Mock<IInquiryOperation>();
            CardReadRepository = new Mock<ICardReadRepository>();
            CardWriteRepository = new Mock<ICardWriteRepository>();
            CustomerReadRepository = new Mock<ICustomerReadRepository>();
            UpdateAssetClient = new Mock<IUpdateAssetClient>();
            ConfigSalesforce = new Mock<IOptions<GetTokenParams>>();
            TokenClient = new Mock<Proxy.Salesforce.GetToken.IGetTokenClient>();

            var cards = GetCards();
            cards[0].CardStatus = "Pending Activation";

            GetTokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(GetBaseTokenResult()));
            RedisService.Setup(x => x.GetCache<IList<Card>>(It.IsAny<string>())).Returns(cards);
            CustomerReadRepository.Setup(x => x.FindByCustomerId(It.IsAny<string>())).Returns(GetCustomer());
            InquiryOperation.Setup(x => x.ProcessearchAsync(It.IsAny<ProcessearchRequest>(), It.IsAny<CancellationToken>())).Returns(GetProcessearchResponse());
            CardWriteRepository.Setup(x => x.Update(It.IsAny<Card>(), cancellationToken));
            RedisService.Setup(x => x.SetCache(It.IsAny<string>(), It.IsAny<IList<Card>>(), It.IsAny<TimeSpan>()));
            TokenClient.Setup(x => x.GetToken(It.IsAny<Proxy.Salesforce.GetToken.Messages.GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(GetTokenBaseResult());
            UpdateAssetClient.Setup(x => x.UpdateAsset(It.IsAny<UpdateAssetParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetSalesforceResult());

            var appSettings = new Dictionary<string, string> { { "Redis:expirationTime", "60" } };

            Configuration = new ConfigurationBuilder().AddInMemoryCollection(appSettings).Build();

            Services.AddSingleton(GetTokenClient.Object);
            Services.AddSingleton(GetTokenParams.Object);
            Services.AddSingleton(RedisService.Object);
            Services.AddSingleton(InquiryOperation.Object);
            Services.AddSingleton(CardReadRepository.Object);
            Services.AddSingleton(CardWriteRepository.Object);
            Services.AddSingleton(CustomerReadRepository.Object);
            Services.AddSingleton(Configuration);
            Services.AddSingleton(TokenClient.Object);
            Services.AddSingleton(UpdateAssetClient.Object);
            Services.AddSingleton(ConfigSalesforce.Object);

            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<CardsController>>();
            var mediator = GetInstance<IMediator>();
            var cardService = GetInstance<ICardService>();
            var controller = new CardsController(mediator, logger.Object, cardService);

            var validate = new ActivateCardAttributesValidator();
            validate.Validate(request);

            var result = await Assert.ThrowsAsync<UnprocessableEntityException>(() => controller.ActivateCard(request, cancellationToken));

            Assert.IsType<UnprocessableEntityException>(result);
        }

        [Fact(DisplayName = "Should throw Unprocessable Entity For Card Not Found")]
        public async void ShouldThrowUnprocessableExceptionForCardNotFound()
        {
            var request = new ActivateCardRequest
            {
                CardId = 1,
                CustomerId = "SP-001",
                Pan = "1112",
                ExpireDate = "2013"
            };

            var cancellationToken = new CancellationToken();
            GetTokenClient = new Mock<IGetTokenClient>();
            GetTokenParams = new Mock<IOptions<GetTokenParams>>();
            RedisService = new Mock<IRedisService>();
            InquiryOperation = new Mock<IInquiryOperation>();
            CardReadRepository = new Mock<ICardReadRepository>();
            CardWriteRepository = new Mock<ICardWriteRepository>();
            CustomerReadRepository = new Mock<ICustomerReadRepository>();
            UpdateAssetClient = new Mock<IUpdateAssetClient>();
            ConfigSalesforce = new Mock<IOptions<GetTokenParams>>();
            TokenClient = new Mock<System.Proxy.Salesforce.GetToken.IGetTokenClient>();

            var cards = GetCards();
            cards[0].CardStatus = "Pending Activation";

            var appSettings = new Dictionary<string, string> { { "Redis:expirationTime", "60" } };

            Configuration = new ConfigurationBuilder().AddInMemoryCollection(appSettings).Build();

            Services.AddSingleton(GetTokenClient.Object);
            Services.AddSingleton(GetTokenParams.Object);
            Services.AddSingleton(RedisService.Object);
            Services.AddSingleton(InquiryOperation.Object);
            Services.AddSingleton(CardReadRepository.Object);
            Services.AddSingleton(CardWriteRepository.Object);
            Services.AddSingleton(CustomerReadRepository.Object);
            Services.AddSingleton(Configuration);
            Services.AddSingleton(TokenClient.Object);
            Services.AddSingleton(UpdateAssetClient.Object);
            Services.AddSingleton(ConfigSalesforce.Object);

            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<CardsController>>();
            var mediator = GetInstance<IMediator>();
            var cardService = GetInstance<ICardService>();
            var controller = new CardsController(mediator, logger.Object, cardService);

            var validate = new ActivateCardAttributesValidator();
            validate.Validate(request);

            var result = await Assert.ThrowsAsync<UnprocessableEntityException>(() => controller.ActivateCard(request, cancellationToken));

            Assert.IsType<UnprocessableEntityException>(result);
        }

        [Fact(DisplayName = "Required information not provided")]
        public void ShouldThrowRequestNotCompliantError()
        {
            var request = new ActivateCardRequest
            {
                CardId = 0,
                CustomerId = null,
                Pan = null,
                ExpireDate = null
            };

            var validate = new ActivateCardAttributesValidator();

            var error = validate.Validate(request);

            Assert.False(error.IsValid);
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

        private BaseResult<GetTokenResult> GetBaseTokenResult()
        {
            return new BaseResult<GetTokenResult>
            {
                IsSuccess = true,
                Result = new GetTokenResult
                {
                    AccessToken = "D923GDM9-2943-9385-6B98-HFJC8742X901"
                }
            };
        }

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

        private Task<Proxy.Salesforce.Messages.BaseResult<Proxy.Salesforce.GetToken.Messages.GetTokenResult>> GetTokenBaseResult()
        {
            return Task.FromResult(new Proxy.Salesforce.Messages.BaseResult<Proxy.Salesforce.GetToken.Messages.GetTokenResult>
            {
                IsSuccess = true,
                Result = new Proxy.Salesforce.GetToken.Messages.GetTokenResult
                {
                    AccessToken = "test"
                }
            });
        }

        private Task<Proxy.Salesforce.Messages.BaseResult<SalesforceResult>> GetSalesforceResult()
        {
            return Task.FromResult(new Proxy.Salesforce.Messages.BaseResult<SalesforceResult>
            {
                IsSuccess = true
            });
        }

        #endregion
    }
}
