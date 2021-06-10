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
using System.Process.Application.Commands.CreditCardAgreement;
using System.Process.Application.Commands.CreditCardAgreement.Validators;
using System.Process.Base.IntegrationTests;
using System.Process.CrossCutting.DependencyInjection;
using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Process.Domain.ValueObjects;
using System.Process.Infrastructure.Configs;
using System.Process.Infrastructure.Repositories.EntityFramework;
using System.Process.Infrastructure.Repositories.MongoDb;
using System.Process.IntegrationTests.Common;
using System.Proxy.Rtdx.CommercialCompanyAdd;
using System.Proxy.Rtdx.CommercialCompanyAdd.Messages;
using System.Proxy.Rtdx.GetToken;
using System.Proxy.Rtdx.NewAccountAdd;
using System.Proxy.Rtdx.NewAccountAdd.Messages;
using System.Proxy.Salesforce;
using System.Proxy.Salesforce.GetAccountInformations;
using System.Proxy.Salesforce.GetAccountInformations.Messages;
using System.Proxy.Salesforce.GetBusinessInformation;
using System.Proxy.Salesforce.GetBusinessInformation.Message;
using System.Proxy.Salesforce.GetCommercialCompanyInfo;
using System.Proxy.Salesforce.GetCommercialCompanyInfo.Message;
using System.Proxy.Salesforce.GetCreditCards;
using System.Proxy.Salesforce.GetCreditCards.Message;
using System.Proxy.Salesforce.GetTerms;
using System.Proxy.Salesforce.GetTerms.Messages;
using System.Proxy.Salesforce.GetToken;
using System.Proxy.Salesforce.GetToken.Messages;
using System.Proxy.Salesforce.Messages;
using System.Proxy.Salesforce.Terms;
using System.Proxy.Salesforce.UpdateAccount;
using System.Proxy.Salesforce.UpdateAccount.Messages;
using System.Proxy.Salesforce.UpdateAsset;
using System.Proxy.Salesforce.UpdateAsset.Messages;
using Xunit;
using RtdxGetToken = System.Proxy.Rtdx.GetToken.Messages;

namespace System.Process.IntegrationTests.Application.Commands.CreditCardAgreement
{
    public class CreditCardAgreementCommandTests
    {
        #region Properties

        private static ServiceProvider Provider;
        private static Mock<IHttpContextAccessor> HttpContextAccessor;
        private static ServiceCollection Services;

        private Mock<ILogger<CreditCardAgreementCommand>> Logger { get; set; }
        private IOptions<RecordTypesConfig> RecordTypesConfig { get; set; }
        private IOptions<GetTokenParams> ConfigSalesforce { get; set; }
        private Mock<IGetTokenClient> TokenClient { get; set; }
        private Mock<IGetCreditCardsClient> GetCreditCardsClient { get; set; }
        private Mock<ICardWriteRepository> CardWriteRepository { get; set; }
        private Mock<IUpdateAssetClient> UpdateAssetClient { get; set; }
        private Mock<INewAccountAddOperation> NewAccountAddOperation { get; set; }
        private Mock<ICommercialCompanyAddOperation> CommercialCompanyAddOperation { get; set; }
        private Mock<IGetBusinessInformationClient> GetBusinessInformationClient { get; set; }
        private Mock<IGetAccountInformationsClient> GetAccountInformationsClient { get; set; }
        private Mock<ICardService> CardService { get; set; }
        private IOptions<ProcessConfig> ProcessConfig { get; set; }
        private Mock<IGetTokenOperation> GetTokenOperation { get; set; }
        private IOptions<RtdxGetToken.GetTokenParams> RtdxTokenParams { get; set; }
        private Mock<IGetCommercialCompanyInfoClient> GetCommercialCompanyInfoClient { get; set; }
        private Mock<IUpdateAccountClient> UpdateAccountClient { get; set; }
        private Mock<IGetTermsClient> GetTermsClient { get; set; }
        private Mock<ICompanyWriteRepository> CompanyWriteRepository { get; set; }
        private Mock<ICompanyReadRepository> CompanyReadRepository { get; set; }
        private Mock<ITermsClient> TermsClient { get; set; }
        #endregion

        #region Constructor

        static CreditCardAgreementCommandTests()
        {
            Services = new ServiceCollection();
            Services.AddMediator();
            Services.AddScoped(typeof(ILoggerFactory), typeof(LoggerFactory));
            Services.AddScoped(typeof(ILogger<>), typeof(Logger<>));
            Services.AddScoped<ICardWriteRepository, CardWriteRepository>();
            Services.AddScoped<ICompanyWriteRepository, CompanyWriteRepository>();
            Services.AddScoped<ICompanyReadRepository, CompanyReadRepository>();

            Provider = Services.BuildServiceProvider();
        }

        #endregion

        #region Main Flow

        [Fact(DisplayName = "Main Flow - Success")]
        public async void ShouldRequestCreditCardAgreementSuccessfully()
        {
            CardService = new Mock<ICardService>();
            Logger = new Mock<ILogger<CreditCardAgreementCommand>>();
            ConfigSalesforce = Options.Create(new GetTokenParams());
            RecordTypesConfig = Options.Create(new RecordTypesConfig() { AssetCreditCard = "test" });
            TokenClient = new Mock<IGetTokenClient>();
            UpdateAssetClient = new Mock<IUpdateAssetClient>();
            NewAccountAddOperation = new Mock<INewAccountAddOperation>();
            CommercialCompanyAddOperation = new Mock<ICommercialCompanyAddOperation>();
            GetCreditCardsClient = new Mock<IGetCreditCardsClient>();
            CardWriteRepository = new Mock<ICardWriteRepository>();
            GetBusinessInformationClient = new Mock<IGetBusinessInformationClient>();
            GetAccountInformationsClient = new Mock<IGetAccountInformationsClient>();
            GetCommercialCompanyInfoClient = new Mock<IGetCommercialCompanyInfoClient>();
            UpdateAccountClient = new Mock<IUpdateAccountClient>();
            GetTermsClient = new Mock<IGetTermsClient>();
            CompanyWriteRepository = new Mock<ICompanyWriteRepository>();
            CompanyReadRepository = new Mock<ICompanyReadRepository>();
            TermsClient = new Mock<ITermsClient>();

            var ProcessConfig = ProcessIntegrationTestsConfiguration.ReadJson<ProcessConfig>("ProcessConfig.json");
            ProcessConfig = Options.Create(ProcessConfig);

            GetTokenOperation = new Mock<IGetTokenOperation>();
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

            CardWriteRepository.Setup(x => x.Add(It.IsAny<Card>(), It.IsAny<CancellationToken>()));
            TokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(GetSalesforceToken());
            UpdateAssetClient.Setup(x => x.UpdateAsset(It.IsAny<UpdateAssetParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(UpdateAssetResult());
            GetCreditCardsClient.Setup(x => x.GetCreditCards(It.IsAny<GetCreditCardsParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetCreditCardsResult());
            NewAccountAddOperation.Setup(x => x.NewAccountAddAsync(It.IsAny<NewAccountAddParams>(), It.IsAny<CancellationToken>())).Returns(NewAccountAddResult());
            CommercialCompanyAddOperation.Setup(x => x.CommercialCompanyAddAsync(It.IsAny<CommercialCompanyAddParams>(), It.IsAny<CancellationToken>())).Returns(GetCommercialCompanyAddResult());
            GetAccountInformationsClient.Setup(x => x.GetAccount(It.IsAny<GetAccountInformationsParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetAccountInformationsResult());
            GetBusinessInformationClient.Setup(x => x.GetBusinessInformation(It.IsAny<GetBusinessInformationParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetBusinessInformationResult());
            GetCommercialCompanyInfoClient.Setup(x => x.GetCommercialCompanyInfo(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetCommercialCompanyInfoResult());
            UpdateAccountClient.Setup(x => x.UpdateHasCompanyFlag(It.IsAny<UpdateAccountParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(UpdateAccountResult());
            GetTokenOperation.Setup(x => x.GetTokenAsync(It.IsAny<RtdxGetToken.GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(GetTokenResult());
            GetTermsClient.Setup(x => x.GetTerms(It.IsAny<GetTermsParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetTermsResult());
            CompanyWriteRepository.Setup(x => x.Save(It.IsAny<Company>()));
            CompanyReadRepository.Setup(x => x.FindBySystemId(It.IsAny<string>()));
            CompanyReadRepository.Setup(x => x.GetLastCompanyId());

            Services.AddSingleton(Logger.Object);
            Services.AddSingleton(ConfigSalesforce);
            Services.AddSingleton(RecordTypesConfig);
            Services.AddSingleton(TokenClient.Object);
            Services.AddSingleton(UpdateAssetClient.Object);
            Services.AddSingleton(NewAccountAddOperation.Object);
            Services.AddSingleton(CommercialCompanyAddOperation.Object);
            Services.AddSingleton(GetCreditCardsClient.Object);
            Services.AddSingleton(CardWriteRepository.Object);
            Services.AddSingleton(GetBusinessInformationClient.Object);
            Services.AddSingleton(GetAccountInformationsClient.Object);
            Services.AddSingleton(ProcessConfig);
            Services.AddSingleton(GetTokenOperation.Object);
            Services.AddSingleton(RtdxTokenParams);
            Services.AddSingleton(UpdateAccountClient.Object);
            Services.AddSingleton(GetCommercialCompanyInfoClient.Object);
            Services.AddSingleton(GetTermsClient.Object);
            Services.AddSingleton(CompanyWriteRepository.Object);
            Services.AddSingleton(CompanyReadRepository.Object);
            Services.AddSingleton(TermsClient.Object);

            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<CardsController>>();
            var mediator = GetInstance<IMediator>();
            var request = GetCreditCardAgreementRequest();
            var controller = new CardsController(mediator, logger.Object, CardService.Object);

            var result = await controller.CreditCardAgreement(request, new CancellationToken());

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact(DisplayName = "Required information not provided")]
        public void ShoulRequestCreditCardAgreementError()
        {
            var validator = new CreditCardAgreementValidator();
            var request = new CreditCardAgreementRequest();

            var error = validator.Validate(request);

            Assert.False(error.IsValid);
        }

        #endregion

        #region Methods

        public static T GetInstance<T>()
        {
            T result = Provider.GetRequiredService<T>();
            if (result is ControllerBase controllerBase)
            {
                SetControllerContext(controllerBase);
            }
            if (result is Controller controller)
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

        private Task<BaseResult<GetTokenResult>> GetSalesforceToken()
        {
            return Task.FromResult(new BaseResult<GetTokenResult>
            {
                IsSuccess = true,
                Result = new GetTokenResult
                {
                    AccessToken = "test"
                }
            });
        }

        private Task<BaseResult<SalesforceResult>> UpdateAssetResult()
        {
            return Task.FromResult(new BaseResult<SalesforceResult>
            {
                IsSuccess = true,
                Result = ProcessIntegrationTestsConfiguration.ReadJson<SalesforceResult>("SalesforceResult.json")
            });
        }

        private Task<Proxy.Rtdx.Messages.BaseResult<NewAccountAddResult>> NewAccountAddResult()
        {
            return Task.FromResult(new Proxy.Rtdx.Messages.BaseResult<NewAccountAddResult>
            {
                IsSuccess = true,
                Result = ProcessIntegrationTestsConfiguration.ReadJson<NewAccountAddResult>("NewAccountAddResult.json")
            });
        }

        private Task<Proxy.Rtdx.Messages.BaseResult<CommercialCompanyAddResult>> GetCommercialCompanyAddResult()
        {
            return Task.FromResult(new Proxy.Rtdx.Messages.BaseResult<CommercialCompanyAddResult>
            {
                IsSuccess = true,
                Result = ProcessIntegrationTestsConfiguration.ReadJson<CommercialCompanyAddResult>("CommercialCompanyAddResult.json")
            });
        }

        private Task<BaseResult<QueryResult<Proxy.Salesforce.GetCreditCards.Message.CreditCard>>> GetCreditCardsResult()
        {
            return Task.FromResult(new BaseResult<QueryResult<Proxy.Salesforce.GetCreditCards.Message.CreditCard>>
            {
                IsSuccess = true,
                Result = new QueryResult<Proxy.Salesforce.GetCreditCards.Message.CreditCard>
                {
                    Records = new List<Proxy.Salesforce.GetCreditCards.Message.CreditCard>
                    {
                        new Proxy.Salesforce.GetCreditCards.Message.CreditCard
                        {
                              AssetId = "string",
                              CreditLimit =  1,
                              Status = "Approved Pending Acceptance",
                              CreditCardType = "string"
                        }
                    }
                }
            });
        }

        private CreditCardAgreementRequest GetCreditCardAgreementRequest()
        {
            return ProcessIntegrationTestsConfiguration.ReadJson<CreditCardAgreementRequest>("CreditCardAgreementRequest.json");
        }

        private Task<BaseResult<QueryResult<GetBusinessInformationResponse>>> GetBusinessInformationResult()
        {
            return Task.FromResult(new BaseResult<QueryResult<GetBusinessInformationResponse>>
            {
                IsSuccess = true,
                Result = new QueryResult<GetBusinessInformationResponse>
                {
                    Records = new List<GetBusinessInformationResponse>
                    {
                        ProcessIntegrationTestsConfiguration.ReadJson<GetBusinessInformationResponse>("GetBusinessInformationResponse.json")
                    }
                }
            });
        }

        private Task<Proxy.Rtdx.Messages.BaseResult<RtdxGetToken.GetTokenResult>> GetTokenResult()
        {
            return Task.FromResult(new Proxy.Rtdx.Messages.BaseResult<RtdxGetToken.GetTokenResult>
            {
                IsSuccess = true,
                Result = ProcessIntegrationTestsConfiguration.ReadJson<RtdxGetToken.GetTokenResult>("RtdxGetTokenResult.json")
            });
        }

        private Task<BaseResult<QueryResult<GetAccountInformationsResponse>>> GetAccountInformationsResult()
        {
            return Task.FromResult(new BaseResult<QueryResult<GetAccountInformationsResponse>>
            {
                IsSuccess = true,
                Result = new QueryResult<GetAccountInformationsResponse>
                {
                    Records = new List<GetAccountInformationsResponse>
                    {
                        ConvertJson.ReadJson<GetAccountInformationsResponse>("GetAccountInformationsResponse.json")
                    }
                }
            });
        }


        private Task<BaseResult<SalesforceResult>> UpdateAccountResult()
        {
            return Task.FromResult(new BaseResult<SalesforceResult>
            {
                IsSuccess = true,
                Result = new SalesforceResult
                {
                    Id = "0016w000008A7EeAAK",
                    Success = true
                },
            });
        }

        private Task<BaseResult<QueryResult<CommercialCompanyInfo>>> GetCommercialCompanyInfoResult()
        {
            return Task.FromResult(new BaseResult<QueryResult<CommercialCompanyInfo>>
            {
                IsSuccess = true,
                Result = new QueryResult<CommercialCompanyInfo>
                {
                    Records = new List<CommercialCompanyInfo>
                    {
                        ConvertJson.ReadJson<CommercialCompanyInfo>("GetCommercialCompanyInfoResponse.json")
                    }
                }
            });
        }

        private Task<BaseResult<QueryResult<Terms>>> GetTermsResult()
        {
            return Task.FromResult(new BaseResult<QueryResult<Terms>>
            {
                IsSuccess = true,
                Result = new QueryResult<Terms>
                {
                    Records = new List<Terms>
                    {
                         new Terms
                            {
                                Version = 0,
                                Type = "Prohibited_Activities",
                                Name = "Card Scheme prohibited"
                           }
                    }
                }
            });
        }

        #endregion
    }
}
