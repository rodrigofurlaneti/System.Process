using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Process.Domain.Constants;
using System.Process.Infrastructure.Configs;
using System.Phoenix.Common.Exceptions;
using System.Proxy.Salesforce.GetBusinessInformation;
using System.Proxy.Salesforce.GetBusinessInformation.Message;
using System.Proxy.Salesforce.GetCreditCards;
using System.Proxy.Salesforce.GetTerms;
using System.Proxy.Salesforce.GetTerms.Messages;
using System.Proxy.Salesforce.GetToken;
using System.Proxy.Salesforce.GetToken.Messages;
using System.Proxy.Salesforce.Messages;
using System.Proxy.Salesforce.RegisterAsset;
using System.Proxy.Salesforce.RegisterAsset.Messages;

namespace System.Process.Application.Commands.CreditCard
{
    public class CreditCardCommand : IRequestHandler<CreditCardRequest, CreditCardResponse>
    {
        #region Properties

        private ILogger<CreditCardCommand> Logger { get; }
        private RecordTypesConfig RecordTypesConfig { get; }
        private GetTokenParams ConfigSalesforce { get; }
        private IGetTokenClient TokenClient { get; }
        private IRegisterAssetClient RegisterAssetClient { get; }
        private IGetCreditCardsClient GetCreditCardsClient { get; }
        private IGetTermsClient GetTermsClient { get; }
        private IGetBusinessInformationClient GetBusinessInformationClient { get; }

        #endregion

        #region Constructor

        public CreditCardCommand(
            ILogger<CreditCardCommand> logger,
            IOptions<RecordTypesConfig> recordTypeConfig,
            IOptions<GetTokenParams> configSalesforce,
            IGetTokenClient tokenClient,
            IRegisterAssetClient registerAssetClient,
            IGetCreditCardsClient getCreditCardsClient,
            IGetTermsClient getTermsClient,
            IGetBusinessInformationClient getBusinessInformationClient
            )
        {
            Logger = logger;
            RecordTypesConfig = recordTypeConfig.Value;
            ConfigSalesforce = configSalesforce.Value;
            TokenClient = tokenClient;
            RegisterAssetClient = registerAssetClient;
            GetCreditCardsClient = getCreditCardsClient;
            GetTermsClient = getTermsClient;
            GetBusinessInformationClient = getBusinessInformationClient;
        }

        #endregion

        #region IRequestHandler implementation

        public async Task<CreditCardResponse> Handle(CreditCardRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var authToken = await TokenClient.GetToken(ConfigSalesforce, cancellationToken);
                var adapter = new CreditCardAdapter(RecordTypesConfig);
                var creditCards = await GetCreditCardsClient.GetCreditCards(adapter.Adapt(request.SystemId), authToken.Result.AccessToken, cancellationToken);

                if (creditCards?.Result?.Records != null)
                {
                    var cards = new List<Proxy.Salesforce.GetCreditCards.Message.CreditCard>();
                    foreach (var card in creditCards?.Result?.Records)
                    {
                        if (card.Status != Constants.DeclinedByCreditStatus && card.Status != Constants.DeclinedByCustomerStatus && card.Status != Constants.DeclinedPendingAcknowledgeStatus)
                        {
                            cards.Add(card);
                        }
                    }

                    if (cards.Count > 0)
                    {
                        Logger.LogError("User must not have a Credit Card Asset in Salesforce", $"The System ID {request.SystemId} already contains an active credit card or credit card order");
                        throw new UnprocessableEntityException("User must not have a Credit Card Asset in Salesforce", $"The System ID {request.SystemId} already contains an active credit card or credit card order");
                    }
                }

                var registerAssetParams = adapter.Adapt(request);

                var businessInformations = await GetBusinessInformations(request.SystemId, authToken.Result.AccessToken, cancellationToken);

                registerAssetParams.ContactId = businessInformations.Id;

                await AddAssetTerm(authToken.Result.AccessToken, registerAssetParams, cancellationToken, businessInformations.Id);

                var response = await RegisterAssetClient.RegisterAsset(registerAssetParams, authToken.Result.AccessToken, cancellationToken);

                return adapter.Adapt(response);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Cannot register asset in Salesforce. The System ID {request.SystemId}");
                throw new UnprocessableEntityException($"Cannot register asset in Salesforce. The System ID {request.SystemId} ", ex.Message);
            }
        }

        #endregion

        private async Task AddAssetTerm(string token, RegisterAssetParams registerAssetParams, CancellationToken cancellationToken, string signerId)
        {
            var term = await GetTermsSalesforce(token, cancellationToken);
            registerAssetParams.OfferTermAndConditionSignDate = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");
            registerAssetParams.OfferTermAndConditionVersion = term.Id;
            registerAssetParams.OfferTermAndConditionSigner = signerId;
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
                throw new UnprocessableEntityException("Cannot get business informations in Salesforce", ex.Message);
            }
        }

        private async Task<Terms> GetTermsSalesforce(string accessToken, CancellationToken cancellationToken)
        {
            try
            {
                var req = new GetTermsParams
                {
                    OriginalChannel = OriginChannelConstants.Application,
                    TermType = Constants.BusinessCreditCardOfferAgreement
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
                throw new UnprocessableEntityException("Cannot Get Terms in Salesforce", ex.Message);
            }

        }
    }
}
