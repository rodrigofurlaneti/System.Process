using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Phoenix.Common.Exceptions;
using System.Proxy.Salesforce.GetToken;
using System.Proxy.Salesforce.GetToken.Messages;
using System.Proxy.Salesforce.UpdateAsset;

namespace System.Process.Application.Commands.CreditCardDeclinedByCredit
{
    public class CreditCardDeclinedByCreditCommand : IRequestHandler<CreditCardDeclinedByCreditRequest, CreditCardDeclinedByCreditResponse>
    {
        #region Properties

        private ILogger<CreditCardDeclinedByCreditCommand> Logger { get; }
        private GetTokenParams ConfigSalesforce { get; }
        private IGetTokenClient TokenClient { get; }
        private IUpdateAssetClient UpdateAssetClient { get; }

        #endregion

        #region Constructor

        public CreditCardDeclinedByCreditCommand(
            ILogger<CreditCardDeclinedByCreditCommand> logger,
            IOptions<GetTokenParams> configSalesforce,
            IGetTokenClient tokenClient,
            IUpdateAssetClient updateAssetClient)
        {
            Logger = logger;
            ConfigSalesforce = configSalesforce.Value;
            TokenClient = tokenClient;
            UpdateAssetClient = updateAssetClient;
        }

        #endregion

        #region IRequestHandler
        public async Task<CreditCardDeclinedByCreditResponse> Handle(CreditCardDeclinedByCreditRequest request, CancellationToken cancellationToken)
        {
            var authToken = await TokenClient.GetToken(ConfigSalesforce, cancellationToken);

            await UpdateCreditCard(request.AssetId, authToken.Result.AccessToken, cancellationToken);

            return await Task.FromResult(new CreditCardDeclinedByCreditResponse
            {
                Success = true
            });
        }

        #endregion

        #region Private methods

        private async Task UpdateCreditCard(string assetId, string accessToken, CancellationToken cancellationToken)
        {
            try
            {
                var adapter = new CreditCardDeclinedByCreditAdapter();
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
