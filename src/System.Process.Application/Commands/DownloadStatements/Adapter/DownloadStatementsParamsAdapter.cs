using System.Process.Domain.Constants;
using System.Process.Infrastructure.Configs;
using System.Proxy.FourSight.StatementGenerate.Messages;
using System.Proxy.FourSight.StatementGenerateInquiry.Messages;
using System;

namespace System.Process.Application.Commands.DownloadStatements.Adapter
{
    public class DownloadStatementsParamsAdapter
    {
        private GenerateStatementsConfig Config { get; set; }

        public DownloadStatementsParamsAdapter(GenerateStatementsConfig config)
        {
            Config = config;
        }

        public StatementGenerateInquiryParams AdaptInquiry(string id)
        {
            var param = new StatementGenerateInquiryParams
            {
                StatementStatusId = id
            };

            return param;
        }

        public StatementGenerateParams AdaptGeneration(string statement)
        {
            var param = new StatementGenerateParams
            {
                StatementId = statement,
                DeliveryType = Constants.InLine,
                ContentType = Constants.Full,
                Cursor = Config.Cursor,
                MaxBytes = Config.MaxBytes
            };

            return param;
        }

        public DownloadStatementsResponse AdaptResponse(StatementGenerateResult generateResponse, byte[] docImage)
        {
            var base64 = Convert.ToBase64String(docImage);

            return new DownloadStatementsResponse
            {
                StatementId = generateResponse.StatementId,
                StatementContent = Constants.Base64ToPdfd + base64
            };
        }
    }
}

