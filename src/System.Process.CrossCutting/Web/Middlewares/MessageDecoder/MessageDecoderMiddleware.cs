using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Process.Domain.Repositories.ErrorMessages;
using System.Phoenix.Common.Exceptions;

namespace System.Process.CrossCutting.Web.Middlewares.MessageDecoder
{
    public class MessageDecoderMiddleware : IMiddleware
    {
        private ILogger<MessageDecoderMiddleware> Logger { get; }
        private IErrorMessagesRepository ErrorCodeRepository { get; }

        public MessageDecoderMiddleware(
            ILogger<MessageDecoderMiddleware> logger,
            IErrorMessagesRepository errorCodeRepository)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            ErrorCodeRepository = errorCodeRepository;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (UnprocessableEntityException exception)
            {
                Logger.LogDebug("MessageDecodeMiddleware: started execution");
                if (!string.IsNullOrWhiteSpace(exception.ErrorCode))
                {
                    Logger.LogInformation($"MessageDecodeMiddleware: Fetching ErrorMessage with {exception.ErrorCode}");
                    var errorCode = ErrorCodeRepository.FindErrorCodeByCode(exception.ErrorCode);

                    if (errorCode != null)
                    {
                        Logger.LogDebug($"MessageDecodeMiddleware: ErrorMessage found.");
                        throw new UnprocessableEntityException(errorCode.Title, exception, new ErrorStructure
                        {
                            Details = errorCode.Details,
                            ErrorCode = errorCode.ErrorCode,
                            IsSelfHealing = errorCode.IsSelfHealing
                        });
                    }
                }

                Logger.LogWarning($"Failed to decode {exception.ErrorCode}. Returning default error: {exception.Message}");
                throw;
            }
        }
    }
}