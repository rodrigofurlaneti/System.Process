using Microsoft.AspNetCore.Builder;

namespace System.Process.CrossCutting.Web.Middlewares.MessageDecoder
{
    public static class MessageDecoderBuilderExtensions
    {
        public static IApplicationBuilder UseMessageDecoderMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<MessageDecoderMiddleware>();
        }
    }
}