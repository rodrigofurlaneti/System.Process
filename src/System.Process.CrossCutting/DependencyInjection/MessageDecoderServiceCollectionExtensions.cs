using Microsoft.Extensions.DependencyInjection;
using System.Process.CrossCutting.Web.Middlewares.MessageDecoder;

namespace System.Process.CrossCutting.DependencyInjection
{
    public static class MessageDecoderServiceCollectionExtensions
    {
        public static IServiceCollection AddMessageDecoder(this IServiceCollection services)
        {
            services.AddScoped<MessageDecoderMiddleware>();

            return services;
        }
    }
}