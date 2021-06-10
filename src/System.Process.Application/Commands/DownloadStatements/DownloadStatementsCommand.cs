using Ardalis.GuardClauses;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Process.Application.Commands.DownloadStatements.Adapter;
using System.Process.Infrastructure.Configs;
using System.Proxy.FourSight.StatementGenerate;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace System.Process.Application.Commands.DownloadStatements
{
    public class DownloadStatementsCommand : IRequestHandler<DownloadStatementsRequest, DownloadStatementsResponse>
    {
        private ILogger<DownloadStatementsCommand> Logger { get; }
        private IStatementGenerate StatementGenerate { get; }
        private GenerateStatementsConfig StatementsConfig { get; }

        public DownloadStatementsCommand(
            ILogger<DownloadStatementsCommand> logger,
            IStatementGenerate chkImgStatementGenerateOperation,
            IOptions<GenerateStatementsConfig> statementsConfig
            )
        {
            Logger = logger;
            StatementGenerate = chkImgStatementGenerateOperation;
            StatementsConfig = statementsConfig.Value;
        }

        public async Task<DownloadStatementsResponse> Handle(DownloadStatementsRequest request, CancellationToken cancellationToken)
        {
            Guard.Against.Null(request, nameof(request));
            Guard.Against.NullOrEmpty(request.StatementId, nameof(request.StatementId));

            var adapter = new DownloadStatementsParamsAdapter(StatementsConfig);
            var statement = new DownloadStatementsResponse();
            var generateParam = adapter.AdaptGeneration(request.StatementId);
            var generateResponse = await StatementGenerate.Generate(generateParam, cancellationToken);

            if (generateResponse != null)
            {
                var docImage = generateResponse.DocImage;

                while (generateResponse.MoreBytes)
                {
                    generateParam.Cursor = (Convert.ToInt32(generateParam.Cursor) + generateParam.MaxBytes).ToString();
                    generateResponse = await StatementGenerate.Generate(generateParam, cancellationToken);
                    docImage = docImage.Concat(generateResponse.DocImage).ToArray();
                }

                statement = adapter.AdaptResponse(generateResponse, docImage);
            }
            else
            {
                Logger.LogInformation("Statement Generate on FourSight Proxy returned null");
            }
            return statement;
        }
    }
}
