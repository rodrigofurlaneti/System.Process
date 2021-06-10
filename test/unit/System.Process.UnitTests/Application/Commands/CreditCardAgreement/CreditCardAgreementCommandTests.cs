using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Process.Application.Commands.CreditCardAgreement;
using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Process.Domain.ValueObjects;
using System.Process.Infrastructure.Configs;
using System.Process.UnitTests.Common;
using System.Phoenix.Common.Exceptions;
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

namespace System.Process.UnitTests.Application.Commands.CreditCardAgreement
{
    public class CreditCardAgreementCommandTests
    {
        #region Properties

        private Mock<ILogger<CreditCardAgreementCommand>> Logger { get; }
        private IOptions<RecordTypesConfig> RecordTypesConfig { get; }
        private IOptions<GetTokenParams> ConfigSalesforce { get; }
        private Mock<IGetTokenClient> TokenClient { get; }
        private Mock<IGetCreditCardsClient> GetCreditCardsClient { get; }
        private Mock<ICardWriteRepository> CardWriteRepository { get; }
        private Mock<IUpdateAssetClient> UpdateAssetClient { get; }
        private Mock<INewAccountAddOperation> NewAccountAddOperation { get; }
        private Mock<ICommercialCompanyAddOperation> CommercialCompanyAddOperation { get; }
        private Mock<IGetBusinessInformationClient> GetBusinessInformationClient { get; }
        private Mock<IGetAccountInformationsClient> GetAccountInformationsClient { get; }
        private IOptions<ProcessConfig> ProcessConfig { get; }
        private Mock<IGetTokenOperation> GetTokenOperation { get; }
        private IOptions<RtdxGetToken.GetTokenParams> RtdxTokenParams { get; }
        private Mock<IGetCommercialCompanyInfoClient> GetCommercialCompanyInfoClient { get; }
        private Mock<IUpdateAccountClient> UpdateAccountClient { get; }
        private Mock<IGetTermsClient> GetTermsClient { get; }
        private Mock<ICompanyWriteRepository> CompanyWriteRepository { get; }
        private Mock<ICompanyReadRepository> CompanyReadRepository { get; }
        private Mock<ITermsClient> TermsClient { get; }

        #endregion

        #region Constructor

        public CreditCardAgreementCommandTests()
        {
            Logger = new Mock<ILogger<CreditCardAgreementCommand>>();
            var config = new RecordTypesConfig();
            RecordTypesConfig = Options.Create(config);
            var param = new GetTokenParams();
            ConfigSalesforce = Options.Create(param);
            TokenClient = new Mock<IGetTokenClient>();
            GetCreditCardsClient = new Mock<IGetCreditCardsClient>();
            CardWriteRepository = new Mock<ICardWriteRepository>();
            UpdateAssetClient = new Mock<IUpdateAssetClient>();
            NewAccountAddOperation = new Mock<INewAccountAddOperation>();
            CommercialCompanyAddOperation = new Mock<ICommercialCompanyAddOperation>();
            GetBusinessInformationClient = new Mock<IGetBusinessInformationClient>();
            GetAccountInformationsClient = new Mock<IGetAccountInformationsClient>();
            var ProcessConfig = ConvertJson.ReadJson<ProcessConfig>("ProcessConfig.json");
            ProcessConfig = Options.Create(ProcessConfig);
            GetTokenOperation = new Mock<IGetTokenOperation>();
            var configRtdx = new RtdxGetToken.GetTokenParams();
            RtdxTokenParams = Options.Create(configRtdx);
            GetCommercialCompanyInfoClient = new Mock<IGetCommercialCompanyInfoClient>();
            UpdateAccountClient = new Mock<IUpdateAccountClient>();
            GetTermsClient = new Mock<IGetTermsClient>();
            CompanyWriteRepository = new Mock<ICompanyWriteRepository>();
            CompanyReadRepository = new Mock<ICompanyReadRepository>();
            TermsClient = new Mock<ITermsClient>();
        }

        #endregion

        #region Methods

        [Fact(DisplayName = "Should Handle credit card agreement Successfully")]
        public async void ShouldCreditCardAgreementSuccessfully()
        {
            TokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(GetSalesforceToken());
            GetCreditCardsClient.Setup(x => x.GetCreditCards(It.IsAny<GetCreditCardsParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetCreditCard());
            NewAccountAddOperation.Setup(x => x.NewAccountAddAsync(It.IsAny<NewAccountAddParams>(), It.IsAny<CancellationToken>())).Returns(GetAccountAddResult());
            CommercialCompanyAddOperation.Setup(x => x.CommercialCompanyAddAsync(It.IsAny<CommercialCompanyAddParams>(), It.IsAny<CancellationToken>())).Returns(GetCommercialCompanyAddResult());
            CardWriteRepository.Setup(x => x.Add(It.IsAny<Card>(), It.IsAny<CancellationToken>()));
            UpdateAssetClient.Setup(x => x.UpdateAsset(It.IsAny<UpdateAssetParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetSalesforceResult());
            GetBusinessInformationClient.Setup(x => x.GetBusinessInformation(It.IsAny<GetBusinessInformationParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetBusinessInformationResult());
            GetAccountInformationsClient.Setup(x => x.GetAccount(It.IsAny<GetAccountInformationsParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetAccountInformationsResult());
            GetTokenOperation.Setup(x => x.GetTokenAsync(It.IsAny<RtdxGetToken.GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(GetTokenResult());
            GetCommercialCompanyInfoClient.Setup(x => x.GetCommercialCompanyInfo(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetCommercialCompanyInfoResult());
            UpdateAccountClient.Setup(x => x.UpdateHasCompanyFlag(It.IsAny<UpdateAccountParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(UpdateAccountResult());
            GetTermsClient.Setup(x => x.GetTerms(It.IsAny<GetTermsParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetTermsResult());
            CompanyWriteRepository.Setup(x => x.Save(It.IsAny<Company>()));
            CompanyReadRepository.Setup(x => x.FindBySystemId(It.IsAny<string>()));
            CompanyReadRepository.Setup(x => x.GetLastCompanyId());
            var command = new CreditCardAgreementCommand(Logger.Object, RecordTypesConfig, ConfigSalesforce, TokenClient.Object, GetCreditCardsClient.Object, CardWriteRepository.Object, UpdateAssetClient.Object, NewAccountAddOperation.Object, CommercialCompanyAddOperation.Object, GetBusinessInformationClient.Object, GetAccountInformationsClient.Object, ProcessConfig, GetTokenOperation.Object, RtdxTokenParams, GetCommercialCompanyInfoClient.Object, UpdateAccountClient.Object, GetTermsClient.Object, CompanyWriteRepository.Object, CompanyReadRepository.Object, TermsClient.Object);

            var request = ConvertJson.ReadJson<CreditCardAgreementRequest>("CreditCardAgreementRequest.json");

            var result = await command.Handle(request, new CancellationToken());

            result.Should().NotBeNull();
        }

        [Fact(DisplayName = "Should GetCreditCards throws new exception")]
        public async void ShouldGetCreditCardsThrowsNewException()
        {
            TokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(GetSalesforceToken());
            GetAccountInformationsClient.Setup(x => x.GetAccount(It.IsAny<GetAccountInformationsParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetAccountInformationsResult());
            GetCreditCardsClient.Setup(x => x.GetCreditCards(It.IsAny<GetCreditCardsParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Throws(new Exception());
            GetCommercialCompanyInfoClient.Setup(x => x.GetCommercialCompanyInfo(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetCommercialCompanyInfoResult());
            UpdateAccountClient.Setup(x => x.UpdateHasCompanyFlag(It.IsAny<UpdateAccountParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(UpdateAccountResult());
            GetTermsClient.Setup(x => x.GetTerms(It.IsAny<GetTermsParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetTermsResult());
            var command = new CreditCardAgreementCommand(Logger.Object, RecordTypesConfig, ConfigSalesforce, TokenClient.Object, GetCreditCardsClient.Object, CardWriteRepository.Object, UpdateAssetClient.Object, NewAccountAddOperation.Object, CommercialCompanyAddOperation.Object, GetBusinessInformationClient.Object, GetAccountInformationsClient.Object, ProcessConfig, GetTokenOperation.Object, RtdxTokenParams, GetCommercialCompanyInfoClient.Object, UpdateAccountClient.Object, GetTermsClient.Object, CompanyWriteRepository.Object, CompanyReadRepository.Object, TermsClient.Object);
            var request = ConvertJson.ReadJson<CreditCardAgreementRequest>("CreditCardAgreementRequest.json");

            await Assert.ThrowsAsync<UnprocessableEntityException>(() => command.Handle(request, new CancellationToken()));
        }

        [Fact(DisplayName = "Should GetCreditCards returns 'The credit card does not belong to the customer'")]
        public async void ShouldGetCreditCardsException()
        {
            TokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(GetSalesforceToken());
            GetCreditCardsClient.Setup(x => x.GetCreditCards(It.IsAny<GetCreditCardsParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetCreditCard());
            GetCommercialCompanyInfoClient.Setup(x => x.GetCommercialCompanyInfo(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetCommercialCompanyInfoResult());
            UpdateAccountClient.Setup(x => x.UpdateHasCompanyFlag(It.IsAny<UpdateAccountParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(UpdateAccountResult());
            GetTermsClient.Setup(x => x.GetTerms(It.IsAny<GetTermsParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetTermsResult());
            var command = new CreditCardAgreementCommand(Logger.Object, RecordTypesConfig, ConfigSalesforce, TokenClient.Object, GetCreditCardsClient.Object, CardWriteRepository.Object, UpdateAssetClient.Object, NewAccountAddOperation.Object, CommercialCompanyAddOperation.Object, GetBusinessInformationClient.Object, GetAccountInformationsClient.Object, ProcessConfig, GetTokenOperation.Object, RtdxTokenParams, GetCommercialCompanyInfoClient.Object, UpdateAccountClient.Object, GetTermsClient.Object, CompanyWriteRepository.Object, CompanyReadRepository.Object, TermsClient.Object);
            var request = ConvertJson.ReadJson<CreditCardAgreementRequest>("CreditCardAgreementRequest.json");

            request.AssetId = "error";

            await Assert.ThrowsAsync<UnprocessableEntityException>(() => command.Handle(request, new CancellationToken()));
        }

        [Fact(DisplayName = "Should GetCreditCards returns 'The Credit Card asset must have the status Approved Pending Acceptance'")]
        public async void ShouldGetCreditCardsApprovedPendingAcceptance()
        {
            TokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(GetSalesforceToken());
            var creditCardRequest = await GetCreditCard();
            creditCardRequest.Result.Records[0].Status = "Error";
            GetCreditCardsClient.Setup(x => x.GetCreditCards(It.IsAny<GetCreditCardsParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(creditCardRequest);
            GetCommercialCompanyInfoClient.Setup(x => x.GetCommercialCompanyInfo(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetCommercialCompanyInfoResult());
            UpdateAccountClient.Setup(x => x.UpdateHasCompanyFlag(It.IsAny<UpdateAccountParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(UpdateAccountResult());
            GetTermsClient.Setup(x => x.GetTerms(It.IsAny<GetTermsParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetTermsResult());
            var command = new CreditCardAgreementCommand(Logger.Object, RecordTypesConfig, ConfigSalesforce, TokenClient.Object, GetCreditCardsClient.Object, CardWriteRepository.Object, UpdateAssetClient.Object, NewAccountAddOperation.Object, CommercialCompanyAddOperation.Object, GetBusinessInformationClient.Object, GetAccountInformationsClient.Object, ProcessConfig, GetTokenOperation.Object, RtdxTokenParams, GetCommercialCompanyInfoClient.Object, UpdateAccountClient.Object, GetTermsClient.Object, CompanyWriteRepository.Object, CompanyReadRepository.Object, TermsClient.Object);
            var request = ConvertJson.ReadJson<CreditCardAgreementRequest>("CreditCardAgreementRequest.json");

            await Assert.ThrowsAsync<UnprocessableEntityException>(() => command.Handle(request, new CancellationToken()));
        }

        [Fact(DisplayName = "Should AddCreditCard throws new exception")]
        public async void ShouldAddCreditCardThrowsNewException()
        {
            TokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(GetSalesforceToken());
            GetAccountInformationsClient.Setup(x => x.GetAccount(It.IsAny<GetAccountInformationsParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetAccountInformationsResult());
            GetCreditCardsClient.Setup(x => x.GetCreditCards(It.IsAny<GetCreditCardsParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetCreditCard());
            NewAccountAddOperation.Setup(x => x.NewAccountAddAsync(It.IsAny<NewAccountAddParams>(), It.IsAny<CancellationToken>())).Throws(new Exception());
            GetBusinessInformationClient.Setup(x => x.GetBusinessInformation(It.IsAny<GetBusinessInformationParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetBusinessInformationResult());
            GetCommercialCompanyInfoClient.Setup(x => x.GetCommercialCompanyInfo(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetCommercialCompanyInfoResult());
            UpdateAccountClient.Setup(x => x.UpdateHasCompanyFlag(It.IsAny<UpdateAccountParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(UpdateAccountResult());
            GetTermsClient.Setup(x => x.GetTerms(It.IsAny<GetTermsParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetTermsResult());
            var command = new CreditCardAgreementCommand(Logger.Object, RecordTypesConfig, ConfigSalesforce, TokenClient.Object, GetCreditCardsClient.Object, CardWriteRepository.Object, UpdateAssetClient.Object, NewAccountAddOperation.Object, CommercialCompanyAddOperation.Object, GetBusinessInformationClient.Object, GetAccountInformationsClient.Object, ProcessConfig, GetTokenOperation.Object, RtdxTokenParams, GetCommercialCompanyInfoClient.Object, UpdateAccountClient.Object, GetTermsClient.Object, CompanyWriteRepository.Object, CompanyReadRepository.Object, TermsClient.Object);
            var request = ConvertJson.ReadJson<CreditCardAgreementRequest>("CreditCardAgreementRequest.json");

            await Assert.ThrowsAsync<UnprocessableEntityException>(() => command.Handle(request, new CancellationToken()));
        }

        [Fact(DisplayName = "Should SaveCreditCardOnDatabase throws new exception")]
        public async void ShouldSaveCreditCardOnDatabaseThrowsNewException()
        {
            TokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(GetSalesforceToken());
            GetAccountInformationsClient.Setup(x => x.GetAccount(It.IsAny<GetAccountInformationsParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetAccountInformationsResult());
            GetCreditCardsClient.Setup(x => x.GetCreditCards(It.IsAny<GetCreditCardsParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetCreditCard());
            NewAccountAddOperation.Setup(x => x.NewAccountAddAsync(It.IsAny<NewAccountAddParams>(), It.IsAny<CancellationToken>())).Returns(GetAccountAddResult());
            GetBusinessInformationClient.Setup(x => x.GetBusinessInformation(It.IsAny<GetBusinessInformationParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetBusinessInformationResult());
            CardWriteRepository.Setup(x => x.Add(It.IsAny<Card>(), It.IsAny<CancellationToken>())).Throws(new Exception());
            GetCommercialCompanyInfoClient.Setup(x => x.GetCommercialCompanyInfo(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetCommercialCompanyInfoResult());
            UpdateAccountClient.Setup(x => x.UpdateHasCompanyFlag(It.IsAny<UpdateAccountParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(UpdateAccountResult());
            GetTermsClient.Setup(x => x.GetTerms(It.IsAny<GetTermsParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetTermsResult());
            var command = new CreditCardAgreementCommand(Logger.Object, RecordTypesConfig, ConfigSalesforce, TokenClient.Object, GetCreditCardsClient.Object, CardWriteRepository.Object, UpdateAssetClient.Object, NewAccountAddOperation.Object, CommercialCompanyAddOperation.Object, GetBusinessInformationClient.Object, GetAccountInformationsClient.Object, ProcessConfig, GetTokenOperation.Object, RtdxTokenParams, GetCommercialCompanyInfoClient.Object, UpdateAccountClient.Object, GetTermsClient.Object, CompanyWriteRepository.Object, CompanyReadRepository.Object, TermsClient.Object);
            var request = ConvertJson.ReadJson<CreditCardAgreementRequest>("CreditCardAgreementRequest.json");

            await Assert.ThrowsAsync<UnprocessableEntityException>(() => command.Handle(request, new CancellationToken()));
        }

        [Fact(DisplayName = "Should UpdateCreditCard throws new exception")]
        public async void ShouldUpdateCreditCardThrowsNewException()
        {
            TokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(GetSalesforceToken());
            GetAccountInformationsClient.Setup(x => x.GetAccount(It.IsAny<GetAccountInformationsParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetAccountInformationsResult());
            GetCreditCardsClient.Setup(x => x.GetCreditCards(It.IsAny<GetCreditCardsParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetCreditCard());
            GetBusinessInformationClient.Setup(x => x.GetBusinessInformation(It.IsAny<GetBusinessInformationParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetBusinessInformationResult());
            NewAccountAddOperation.Setup(x => x.NewAccountAddAsync(It.IsAny<NewAccountAddParams>(), It.IsAny<CancellationToken>())).Returns(GetAccountAddResult());
            CardWriteRepository.Setup(x => x.Add(It.IsAny<Card>(), It.IsAny<CancellationToken>()));
            UpdateAssetClient.Setup(x => x.UpdateAsset(It.IsAny<UpdateAssetParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Throws(new Exception());
            GetCommercialCompanyInfoClient.Setup(x => x.GetCommercialCompanyInfo(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetCommercialCompanyInfoResult());
            UpdateAccountClient.Setup(x => x.UpdateHasCompanyFlag(It.IsAny<UpdateAccountParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(UpdateAccountResult());
            GetTermsClient.Setup(x => x.GetTerms(It.IsAny<GetTermsParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetTermsResult());
            var command = new CreditCardAgreementCommand(Logger.Object, RecordTypesConfig, ConfigSalesforce, TokenClient.Object, GetCreditCardsClient.Object, CardWriteRepository.Object, UpdateAssetClient.Object, NewAccountAddOperation.Object, CommercialCompanyAddOperation.Object, GetBusinessInformationClient.Object, GetAccountInformationsClient.Object, ProcessConfig, GetTokenOperation.Object, RtdxTokenParams, GetCommercialCompanyInfoClient.Object, UpdateAccountClient.Object, GetTermsClient.Object, CompanyWriteRepository.Object, CompanyReadRepository.Object, TermsClient.Object);
            var request = ConvertJson.ReadJson<CreditCardAgreementRequest>("CreditCardAgreementRequest.json");

            await Assert.ThrowsAsync<UnprocessableEntityException>(() => command.Handle(request, new CancellationToken()));
        }

        #endregion

        #region Private Methods

        private Task<BaseResult<GetTokenResult>> GetSalesforceToken()
        {
            return Task.FromResult(new BaseResult<GetTokenResult>
            {
                IsSuccess = true,
                Result = ConvertJson.ReadJson<GetTokenResult>("GetTokenResult.json")
            });
        }

        private Task<BaseResult<QueryResult<Proxy.Salesforce.GetCreditCards.Message.CreditCard>>> GetCreditCard()
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

        private Task<Proxy.Rtdx.Messages.BaseResult<NewAccountAddResult>> GetAccountAddResult()
        {
            return Task.FromResult(new Proxy.Rtdx.Messages.BaseResult<NewAccountAddResult>
            {
                IsSuccess = true,
                Result = ConvertJson.ReadJson<NewAccountAddResult>("NewAccountAddResult.json")
            });
        }

        private Task<Proxy.Rtdx.Messages.BaseResult<CommercialCompanyAddResult>> GetCommercialCompanyAddResult()
        {
            return Task.FromResult(new Proxy.Rtdx.Messages.BaseResult<CommercialCompanyAddResult>
            {
                IsSuccess = true,
                Result = ConvertJson.ReadJson<CommercialCompanyAddResult>("CommercialCompanyAddResult.json")
            });
        }

        private Task<BaseResult<SalesforceResult>> GetSalesforceResult()
        {
            return Task.FromResult(new BaseResult<SalesforceResult>
            {
                IsSuccess = true,
                Result = new SalesforceResult
                {
                    Success = true
                }
            });
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
                        ConvertJson.ReadJson<GetBusinessInformationResponse>("GetBusinessInformationResponse.json")
                    }
                }
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

        private Task<Proxy.Rtdx.Messages.BaseResult<RtdxGetToken.GetTokenResult>> GetTokenResult()
        {
            return Task.FromResult(new Proxy.Rtdx.Messages.BaseResult<RtdxGetToken.GetTokenResult>
            {
                IsSuccess = true,
                Result = ConvertJson.ReadJson<RtdxGetToken.GetTokenResult>("RtdxGetTokenResult.json")
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
