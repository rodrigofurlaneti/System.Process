using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Process.Domain.Repositories;
using System.Phoenix.Common.Exceptions;
using System.Proxy.Fis.GetToken;
using System.Proxy.Fis.GetToken.Messages;
using System.Proxy.Fis.TransactionDetail;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace System.Process.Application.Commands.TransactionDetail
{
    public class TransactionDetailCommand : IRequestHandler<ConsultCardsByKeyTransactionRequest, TransactionDetailResponse>
    {
        #region Properties

        private ILogger<TransactionDetailCommand> Logger { get; }
        private IGetTokenClient GetTokenClient { get; }
        private GetTokenParams GetTokenParams { get; }
        private ITransactionDetailClient TransactionDetailClient { get; }
        private ICardReadRepository CardReadRepository { get; }
        #endregion

        #region Constructor

        public TransactionDetailCommand(
            ILogger<TransactionDetailCommand> logger,
            IGetTokenClient getTokenClient,
            IOptions<GetTokenParams> getTokenParams,
            ITransactionDetailClient transactionDetailClient,
            ICardReadRepository cardReadRepository
            )
        {
            Logger = logger;
            GetTokenClient = getTokenClient;
            GetTokenParams = getTokenParams.Value;
            TransactionDetailClient = transactionDetailClient;
            CardReadRepository = cardReadRepository;
        }

        #endregion

        #region IRequestHandler implementation

        public async Task<TransactionDetailResponse> Handle(ConsultCardsByKeyTransactionRequest requestKey, CancellationToken cancellationToken)
        {
            try
            {
                var cards = CardReadRepository.Find(requestKey.CardId);

                TransactionDetailRequest request = new TransactionDetailRequest
                {
                    Pan = cards[0].Pan,
                    PrimaryKey = requestKey.PrimaryKey
                };

                var adapter = new TransactionDetailAdapter();

                var paramsTransactionDetail = adapter.Adapt(request);
                var token = await GetTokenClient.GetToken(GetTokenParams, cancellationToken);
                var result = await TransactionDetailClient.TransactionDetailAsync(paramsTransactionDetail, token.Result.AccessToken, cancellationToken);

                if (!result.IsSuccess)
                {
                    var error = JsonConvert.DeserializeObject<ReturnMetadata>(result.Message);
                    throw new UnprocessableEntityException(error?.Metadata.Messages[0].Text);
                }

                return await Task.FromResult(new TransactionDetailResponse
                {
                    Metadata = result.Result.Metadata,
                    Transaction = result.Result.Transaction
                });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                throw new UnprocessableEntityException(ex.Message);
            }
        }

        #endregion
    }
}