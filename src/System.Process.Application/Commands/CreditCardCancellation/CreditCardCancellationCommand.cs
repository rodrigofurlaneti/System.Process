using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Process.Domain.Constants;
using System.Process.Infrastructure.Configs;
using System.Phoenix.Common.Exceptions;
using System.Proxy.Salesforce.GetCreditCards;
using System.Proxy.Salesforce.GetToken;
using System.Proxy.Salesforce.GetToken.Messages;
using System.Proxy.Salesforce.UpdateAsset;

namespace System.Process.Application.Commands.CreditCardCancellation
{
    public class CreditCardCancellationCommand : IRequestHandler<CreditCardCancellationRequest, CreditCardCancellationResponse>
    {
        #region Properties

        private ILogger<CreditCardCancellationCommand> Logger { get; }
        private RecordTypesConfig RecordTypesConfig { get; }
        private GetTokenParams ConfigSalesforce { get; }
        private IGetTokenClient TokenClient { get; }
        private IGetCreditCardsClient GetCreditCardsClient { get; }
        private IUpdateAssetClient UpdateAssetClient { get; }

        #endregion

        #region Constructor

        public CreditCardCancellationCommand(
            ILogger<CreditCardCancellationCommand> logger,
            IOptions<RecordTypesConfig> recordTypeConfig,
            IOptions<GetTokenParams> configSalesforce,
            IGetTokenClient tokenClient,
            IGetCreditCardsClient getCreditCardsClient,
            IUpdateAssetClient updateAssetClient)
        {
            Logger = logger;
            RecordTypesConfig = recordTypeConfig.Value;
            ConfigSalesforce = configSalesforce.Value;
            TokenClient = tokenClient;
            GetCreditCardsClient = getCreditCardsClient;
            UpdateAssetClient = updateAssetClient;
        }

        #endregion

        #region IRequestHandler
        public async Task<CreditCardCancellationResponse> Handle(CreditCardCancellationRequest request, CancellationToken cancellationToken)
        {
            var authToken = await TokenClient.GetToken(ConfigSalesforce, cancellationToken);

            await GetCreditCard(request, authToken.Result.AccessToken, cancellationToken);

            await UpdateCreditCard(request.AssetId, authToken.Result.AccessToken, cancellationToken);

            return await Task.FromResult(new CreditCardCancellationResponse
            {
                Success = true
            });
        }

        #endregion

        #region Private methods

        private async Task GetCreditCard(CreditCardCancellationRequest request, string accessToken, CancellationToken cancellationToken)
        {
            try
            {
                var adapter = new CreditCardCancellationAdapter(RecordTypesConfig);
                var creditCards = await GetCreditCardsClient.GetCreditCards(adapter.Adapt(request), accessToken, cancellationToken);

                if (!creditCards.Result.Records.Any(x => x.AssetId == request.AssetId))
                {
                    Logger.LogError("The credit card does not belong to the customer", $"AssetId {request.AssetId} and SystemId {request.SystemId}");
                    throw new UnprocessableEntityException("The credit card does not belong to the customer", $"AssetId {request.AssetId} and SystemId {request.SystemId}");
                }

                var creditCard = creditCards.Result.Records.Where(x => x.AssetId == request.AssetId).FirstOrDefault();

                if (creditCard.Status != Constants.ApprovedPendingAcceptance)
                {
                    Logger.LogError("The Credit Card asset must have the status 'Approved Pending Acceptance'", $"AssetId {request.AssetId} and SystemId {request.SystemId}");
                    throw new UnprocessableEntityException("The Credit Card asset must have the status 'Approved Pending Acceptance'", $"AssetId {request.AssetId} and SystemId {request.SystemId}");
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Execution error on GetCreditCard");
                throw new UnprocessableEntityException("Cannot get credit cards in Salesforce", ex.Message);
            }
        }

        private async Task UpdateCreditCard(string assetId, string accessToken, CancellationToken cancellationToken)
        {
            try
            {
                var adapter = new CreditCardCancellationAdapter(RecordTypesConfig);
                var response = await UpdateAssetClient.UpdateAsset(adapter.Adapt(assetId), accessToken, cancellationToken);

                if (!response.IsSuccess)
                {
                    throw new UnprocessableEntityException("Cannot update asset in Salesforce", response.Message);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Execution error on UpdateCreditCard");
                throw new UnprocessableEntityException("Cannot update the field Status of the Credit Card asset in Salesforce", ex.Message);
            }
        }

        #endregion
    }
}
