using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Process.Application.DataTransferObjects;
using System.Process.Domain.Constants;
using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Process.Domain.ValueObjects;
using System.Process.Infrastructure.Configs;
using System.Phoenix.Common.Exceptions;
using System.Proxy.Rtdx.CommercialCompanyAdd;
using System.Proxy.Rtdx.GetToken;
using System.Proxy.Rtdx.NewAccountAdd;
using System.Proxy.Rtdx.NewAccountAdd.Messages;
using System.Proxy.Salesforce.GetAccountInformations;
using System.Proxy.Salesforce.GetAccountInformations.Messages;
using System.Proxy.Salesforce.GetBusinessInformation;
using System.Proxy.Salesforce.GetBusinessInformation.Message;
using System.Proxy.Salesforce.GetCommercialCompanyInfo;
using System.Proxy.Salesforce.GetCommercialCompanyInfo.Message;
using System.Proxy.Salesforce.GetCreditCards;
using System.Proxy.Salesforce.GetTerms;
using System.Proxy.Salesforce.GetTerms.Messages;
using System.Proxy.Salesforce.GetToken;
using System.Proxy.Salesforce.GetToken.Messages;
using System.Proxy.Salesforce.Messages;
using System.Proxy.Salesforce.Terms;
using System.Proxy.Salesforce.Terms.Messages;
using System.Proxy.Salesforce.UpdateAccount;
using System.Proxy.Salesforce.UpdateAccount.Messages;
using System.Proxy.Salesforce.UpdateAsset;
using System.Proxy.Salesforce.UpdateAsset.Messages;
using RtdxGetToken = System.Proxy.Rtdx.GetToken.Messages;
using SalesforceCreditCard = System.Proxy.Salesforce.GetCreditCards.Message;

namespace System.Process.Application.Commands.CreditCardAgreement
{
    public class CreditCardAgreementCommand : IRequestHandler<CreditCardAgreementRequest, CreditCardAgreementResponse>
    {
        #region Properties

        private ILogger<CreditCardAgreementCommand> Logger { get; }
        private RecordTypesConfig RecordTypesConfig { get; }
        private GetTokenParams SalesforceTokenParams { get; }
        private IGetTokenClient TokenClient { get; }
        private IGetCreditCardsClient GetCreditCardsClient { get; }
        private ICardWriteRepository CardWriteRepository { get; }
        private IUpdateAssetClient UpdateAssetClient { get; }
        private INewAccountAddOperation NewAccountAddOperation { get; }
        private ICommercialCompanyAddOperation CommercialCompanyAddOperation { get; }
        private IGetBusinessInformationClient GetBusinessInformationClient { get; }
        private IGetAccountInformationsClient GetAccountInformationsClient { get; }
        private ProcessConfig ProcessConfig { get; }
        private IGetTokenOperation GetTokenOperation { get; }
        private RtdxGetToken.GetTokenParams RtdxTokenParams { get; }
        private IGetCommercialCompanyInfoClient GetCommercialCompanyInfoClient { get; }
        private IUpdateAccountClient UpdateAccountClient { get; }
        private IGetTermsClient GetTermsClient { get; }
        private ICompanyWriteRepository CompanyWriteRepository { get; }
        private ICompanyReadRepository CompanyReadRepository { get; }
        private ITermsClient TermsClient { get; }

        #endregion

        #region Constructor

        public CreditCardAgreementCommand(
            ILogger<CreditCardAgreementCommand> logger,
            IOptions<RecordTypesConfig> recordTypeConfig,
            IOptions<GetTokenParams> salesforceTokenSalesforce,
            IGetTokenClient tokenClient,
            IGetCreditCardsClient getCreditCardsClient,
            ICardWriteRepository cardWriteRepository,
            IUpdateAssetClient updateAssetClient,
            INewAccountAddOperation newAccountAddOperation,
            ICommercialCompanyAddOperation commercialCompanyAddOperation,
            IGetBusinessInformationClient getBusinessInformationClient,
            IGetAccountInformationsClient getAccountInformationsClient,
            IOptions<ProcessConfig> ProcessConfig,
            IGetTokenOperation getTokenOperation,
            IOptions<RtdxGetToken.GetTokenParams> rtdxTokenParams,
            IGetCommercialCompanyInfoClient getCommercialCompanyInfoClient,
            IUpdateAccountClient updateAccountClient,
            IGetTermsClient getTermsClient,
            ICompanyWriteRepository companyWriteRepository,
            ICompanyReadRepository companyReadRepository,
            ITermsClient termsClient)
        {
            Logger = logger;
            RecordTypesConfig = recordTypeConfig.Value;
            SalesforceTokenParams = salesforceTokenSalesforce.Value;
            TokenClient = tokenClient;
            GetCreditCardsClient = getCreditCardsClient;
            CardWriteRepository = cardWriteRepository;
            UpdateAssetClient = updateAssetClient;
            NewAccountAddOperation = newAccountAddOperation;
            CommercialCompanyAddOperation = commercialCompanyAddOperation;
            GetBusinessInformationClient = getBusinessInformationClient;
            GetAccountInformationsClient = getAccountInformationsClient;
            ProcessConfig = ProcessConfig.Value;
            GetTokenOperation = getTokenOperation;
            RtdxTokenParams = rtdxTokenParams.Value;
            GetCommercialCompanyInfoClient = getCommercialCompanyInfoClient;
            UpdateAccountClient = updateAccountClient;
            GetTermsClient = getTermsClient;
            CompanyWriteRepository = companyWriteRepository;
            CompanyReadRepository = companyReadRepository;
            TermsClient = termsClient;
        }

        #endregion

        #region IRequestHandler

        public async Task<CreditCardAgreementResponse> Handle(CreditCardAgreementRequest request, CancellationToken cancellationToken)
        {
            Logger.LogInformation($"Starting Credit Card Agreement. System : {request.SystemId}");

            Logger.LogInformation($"Starting Get Token Salesforce.");
            var authToken = await TokenClient.GetToken(SalesforceTokenParams, cancellationToken);

            var accountInformationsParams = new GetAccountInformationsParams
            {
                SystemId = request.SystemId
            };

            Logger.LogInformation($"Starting Get Account Information.");
            var result = await GetAccountInformationsClient.GetAccount(accountInformationsParams, authToken?.Result?.AccessToken, cancellationToken);

            Logger.LogInformation($"Starting Get Token RTDX.");
            var tokenResult = await GetTokenOperation.GetTokenAsync(RtdxTokenParams, cancellationToken);
            var securityToken = tokenResult?.Result?.SecurityToken;

            Logger.LogInformation($"Starting Get Credit Card on Salesforce.");
            var creditCard = await GetCreditCard(request, authToken?.Result?.AccessToken, cancellationToken);

            Logger.LogInformation($"Starting Get Business Informations on Salesforce.");
            var businessInformation = await GetBusinessInformations(request, authToken?.Result?.AccessToken, cancellationToken);

            var rtdxRequest = new RtdxCreditCardRequest(creditCard, request, businessInformation.Result.Records.FirstOrDefault(), securityToken, RtdxTokenParams, result?.Result?.Records?.FirstOrDefault()?.CifId, new CommercialCompanyInfo(), string.Empty);

            Logger.LogInformation($"Starting Get Commercial Comapny Informations on Salesforce.");
            var commercialCompanyInfos = await GetCommercialCompanyInfoClient.GetCommercialCompanyInfo(request.SystemId, authToken?.Result?.AccessToken, cancellationToken);
            rtdxRequest.CommercialCompanyInfo = commercialCompanyInfos?.Result?.Records?.FirstOrDefault();

            Logger.LogInformation($"Starting Validate Company.");
            await ValidateCompany(request, authToken, result, rtdxRequest, cancellationToken);

            var addCreditCardResult = await AddCreditCard(rtdxRequest, cancellationToken);

            Logger.LogInformation($"Starting Save Credit Card On Database.");
            await SaveCreditCardOnDatabase(addCreditCardResult.Result, request, cancellationToken);

            Logger.LogInformation($"Starting Update Credit Card on Salesforce.");
            await UpdateCreditCard(request, authToken?.Result?.AccessToken, cancellationToken);

            return await Task.FromResult(new CreditCardAgreementResponse
            {
                Success = true
            });
        }

        #endregion

        #region Private Methods

        private async Task ValidateCompany(CreditCardAgreementRequest request, BaseResult<GetTokenResult> authToken, BaseResult<QueryResult<GetAccountInformationsResponse>> result, RtdxCreditCardRequest rtdxRequest, CancellationToken cancellationToken)
        {
            if (result.Result.Records.Any() && !result.Result.Records.FirstOrDefault().HasCompany)
            {
                GenerateCompanyId(rtdxRequest);
                await AddNewCompany(rtdxRequest, cancellationToken);
                AddNewCompanyIdOnDatabase(request?.SystemId, result?.Result?.Records?.FirstOrDefault()?.CifId, rtdxRequest?.CompanyId);
                await UpdateHasCompanyFlag(request, authToken, cancellationToken);
            }
            else
            {
                var company = CompanyReadRepository.FindBySystemId(request?.SystemId);
                rtdxRequest.CompanyId = company.CompanyId;
            }
        }

        private void AddNewCompanyIdOnDatabase(string SystemId, string cifId, string companyId)
        {
            Company company = new Company()
            {
                CompanyId = companyId,
                SystemId = SystemId,
                CifId = cifId,
                CreationDate = DateTime.UtcNow
            };

            Logger.LogInformation($"Starting Add New CompanyId on Database. Company :{JsonConvert.SerializeObject(company)}");
            CompanyWriteRepository.Save(company);
        }

        private void GenerateCompanyId(RtdxCreditCardRequest rtdxRequest)
        {
            Logger.LogInformation($"Starting Generate CompanyId.");
            var lastcompany = CompanyReadRepository.GetLastCompanyId();
            var newCompanyId = 0;

            if (lastcompany != null)
            {
                newCompanyId = int.Parse(lastcompany?.CompanyId);
            }

            rtdxRequest.CompanyId = (newCompanyId + 1).ToString("D8");

            Logger.LogInformation($"Generate CompanyId {rtdxRequest.CompanyId}. System : {rtdxRequest.CreditCardAgreementRequest.SystemId}");
        }

        private async Task UpdateHasCompanyFlag(CreditCardAgreementRequest request, BaseResult<GetTokenResult> authToken, CancellationToken cancellationToken)
        {
            Logger.LogInformation($"Starting Update has comapny flag.");
            var updateHasCompanyFlagParams = new UpdateAccountParams()
            {
                Id = request.SystemId,
                UpdateAccountBody = new UpdateAccountBody()
                {
                    HasCompany = true
                }
            };

            await UpdateAccountClient.UpdateHasCompanyFlag(updateHasCompanyFlagParams, authToken.Result.AccessToken, cancellationToken);
        }

        private async Task AddNewCompany(RtdxCreditCardRequest rtdxRequest, CancellationToken cancellationToken)
        {
            try
            {
                Logger.LogInformation($"Starting Add New Company.");
                var adapter = new Adapters.CommercialCompanyAddAdapter(ProcessConfig, Logger);
                var result = await CommercialCompanyAddOperation.CommercialCompanyAddAsync(adapter.Adapt(rtdxRequest), cancellationToken);

                if (!result.IsSuccess || result.Result.ResponseCode != "00")
                {
                    Logger.LogError("Error on CommercialCompanyAdd", $"Message FIS: {result.Result.Message}. SystemId {rtdxRequest.CreditCardAgreementRequest.SystemId} ");
                    throw new UnprocessableEntityException($"Error on CommercialCompanyAdd, Message FIS: {result.Result.Message}. SystemId {rtdxRequest.CreditCardAgreementRequest.SystemId}");
                }

            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Execution error on add new company");
                throw new UnprocessableEntityException("Error on CommercialCompanyAdd", ex.Message);
            }
        }

        private async Task<SalesforceCreditCard.CreditCard> GetCreditCard(CreditCardAgreementRequest request, string accessToken, CancellationToken cancellationToken)
        {
            try
            {
                var adapter = new CreditCardAgreementAdapter(RecordTypesConfig);
                var creditCards = await GetCreditCardsClient.GetCreditCards(adapter.Adapt(request), accessToken, cancellationToken);

                if (!creditCards.Result.Records.Any(x => x.AssetId == request.AssetId))
                {
                    Logger.LogError("The credit card does not belong to the customer", $"AssetId {request.AssetId} and SystemId {request.SystemId}");
                    throw new UnprocessableEntityException("The credit card does not belong to the customer", $"AssetId {request.AssetId} and SystemId {request.SystemId}");
                }

                var creditCard = creditCards.Result.Records.FirstOrDefault(x => x.AssetId == request.AssetId);

                if (creditCard.Status != Constants.ApprovedPendingAcceptance)
                {
                    Logger.LogError("The Credit Card asset must have the status 'Approved Pending Acceptance'", $"AssetId {request.AssetId} and SystemId {request.SystemId}");
                    throw new UnprocessableEntityException("The Credit Card asset must have the status 'Approved Pending Acceptance'", $"AssetId {request.AssetId} and SystemId {request.SystemId}");
                }

                return creditCard;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Execution error on GetCreditCard");
                throw new UnprocessableEntityException("Error on GetCreditCard", ex.Message);
            }
        }

        private async Task<BaseResult<QueryResult<GetBusinessInformationResponse>>> GetBusinessInformations(CreditCardAgreementRequest request, string accessToken, CancellationToken cancellationToken)
        {
            try
            {
                var adapter = new BusinessInformationAdapter();
                var result = await GetBusinessInformationClient.GetBusinessInformation(adapter.Adapt(request), accessToken, cancellationToken);

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Execution error on GetBusinessInformations");
                throw new UnprocessableEntityException("Cannot get business informations on Salesforce", ex.Message);
            }
        }

        private async Task<Proxy.Rtdx.Messages.BaseResult<NewAccountAddResult>> AddCreditCard(RtdxCreditCardRequest rtdxRequest, CancellationToken cancellationToken)
        {
            try
            {
                var adapter = new Adapters.NewAccountAddAdapter(ProcessConfig, Logger);
                var result = await NewAccountAddOperation.NewAccountAddAsync(adapter.Adapt(rtdxRequest), cancellationToken);

                if (!result.IsSuccess || result.Result.ResponseCode != "00")
                {
                    Logger.LogError("Error on NewAccountAddAsync", $"Message FIS: {result.Result.Message}. SystemId {rtdxRequest.CreditCardAgreementRequest.SystemId} ");
                    throw new UnprocessableEntityException($"Error on NewAccountAddAsync, Message FIS: {result.Result.Message}. SystemId {rtdxRequest.CreditCardAgreementRequest.SystemId}");
                }

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Execution error on AddCreditCard");
                throw new UnprocessableEntityException("Cannot add credit card on Rtdx", ex.Message);
            }
        }

        private async Task SaveCreditCardOnDatabase(NewAccountAddResult addCreditCardResult, CreditCardAgreementRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var pan = addCreditCardResult.AccountBin.ToString() + addCreditCardResult.Processequence;

                var creditCard = new Card
                {
                    CustomerId = request.SystemId,
                    AccountBalance = "0",
                    ExpirationDate = addCreditCardResult.AgreementExpirationDate,
                    CardHolder = FullName(addCreditCardResult),
                    BusinessName = addCreditCardResult.CorpName,
                    Pan = pan,
                    LastFour = GetLastFourDigits(pan),
                    Bin = addCreditCardResult.AccountBin.ToString(),
                    AssetId = request.AssetId,
                    CardType = Constants.CreditCarType,
                    Locked = Constants.CreditCardLocked,
                    CardStatus = Constants.PendingActivationStatus,
                    Validated = Constants.CreditCardValidated
                };

                await CardWriteRepository.Add(creditCard, cancellationToken);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Execution error on SaveCreditCardOnDatabase");
                throw new UnprocessableEntityException("Cannot add credit card on database", ex.Message);
            }
        }

        private async Task UpdateCreditCard(CreditCardAgreementRequest request, string accessToken, CancellationToken cancellationToken)
        {
            try
            {
                var adapter = new CreditCardAgreementAdapter(RecordTypesConfig);
                var updateAssetParams = adapter.Adapt(request.AssetId);

                var signer = await GetBusinessInformations(request.SystemId, accessToken, cancellationToken);

                await AddAssetTerm(request, accessToken, signer.Id, updateAssetParams, cancellationToken);

                await AddTerms(accessToken, request.Terms, cancellationToken, request.SystemId);

                var response = await UpdateAssetClient.UpdateAsset(updateAssetParams, accessToken, cancellationToken);

                if (!response.IsSuccess)
                {
                    throw new UnprocessableEntityException("Cannot update asset on Salesforce", response.Message);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Execution error on UpdateCreditCard");
                throw new UnprocessableEntityException("Cannot update the field Status of the Credit Card asset on Salesforce", ex.Message);
            }
        }

        private string GetLastFourDigits(string pan)
        {
            if (string.IsNullOrEmpty(pan))
            {
                return string.Empty;
            }

            return pan.Substring(pan.Length - 4);
        }

        private static string FullName(NewAccountAddResult addCreditCardResult)
        {
            var fullName = string.Concat(addCreditCardResult.FirstNameTwo, " ", addCreditCardResult.MiddleNameTwo, " ", addCreditCardResult.LastNameTwo);

            if (fullName.Length > 25)
            {
                fullName = fullName.Substring(0, 25);
            }

            return fullName;
        }

        private async Task AddAssetTerm(CreditCardAgreementRequest request, string token, string singerId, UpdateAssetParams updateAssetParams, CancellationToken cancellationToken)
        {
            var term = await GetTermsSalesforce(request.Terms.FirstOrDefault().Type, token, cancellationToken);
            updateAssetParams.UpdateAssetBody.TermAndConditionSignDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");
            updateAssetParams.UpdateAssetBody.TermAndConditionVersion = term.Id;
            updateAssetParams.UpdateAssetBody.TermAndConditionSigner = singerId;
            updateAssetParams.UpdateAssetBody.TermAndConditionType = term.Type;
        }

        private async Task<GetBusinessInformationResponse> GetBusinessInformations(string SystemId, string accessToken, CancellationToken cancellationToken)
        {
            try
            {
                var getBusinessInformationParams = new GetBusinessInformationParams
                {
                    SystemId = SystemId
                };
                var result = await GetBusinessInformationClient.GetBusinessInformation(getBusinessInformationParams, accessToken, cancellationToken);

                return result.Result.Records.FirstOrDefault();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Execution error on GetBusinessInformations");
                throw new UnprocessableEntityException("Cannot get business informations on Salesforce", ex.Message);
            }
        }

        private async Task<Terms> GetTermsSalesforce(string termType, string accessToken, CancellationToken cancellationToken)
        {
            try
            {
                var req = new GetTermsParams
                {
                    OriginalChannel = OriginChannelConstants.Application,
                    TermType = termType
                };

                var result = await GetTermsClient.GetTerms(req, accessToken, cancellationToken);
                var response = new Terms();
                var salesforceTerms = result.Result.Records?.ToList();

                if (salesforceTerms != null && salesforceTerms?.Count > 0)
                {
                    var version = salesforceTerms.Max(st => st.Version);
                    response = salesforceTerms.Find(t => t.Version == version);
                }

                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Error during the GetTerms request");
                throw new UnprocessableEntityException("Cannot Get Terms on Salesforce", ex.Message);
            }

        }
        
        private async Task AddTerms(string token, IList<TermDto> terms, CancellationToken cancellationToken, string SystemId)
        {
            if (terms != null && terms.Count > 0)
            {
                var termsParams = new List<Proxy.Salesforce.Terms.Messages.Term>();

                var validTerms = terms.Where(term => term.Type != null && term.Type.Equals("BusinessCreditCardFeeSchedule")).ToList();

                validTerms.ForEach(async term =>
                {
                    var latestTerm = await GetTermsSalesforce(term.Type, token, cancellationToken);
                    termsParams.Add(new Proxy.Salesforce.Terms.Messages.Term() { Id = latestTerm.Id });
                });

                if (termsParams.Count > 0)
                {
                    var registerTermsParams = new RegisterTermsParams()
                    {
                        Request = new Proxy.Salesforce.Terms.Messages.Request()
                        {
                            SystemId = SystemId,
                            Terms = termsParams
                        }
                    };
                    await TermsClient.RegisterTerms(registerTermsParams, token, cancellationToken);
                }
            }
        }
        #endregion
    }
}
