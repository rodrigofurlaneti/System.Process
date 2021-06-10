using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Phoenix.Communication.Soap;

namespace System.Process.CrossCutting.DependencyInjection
{
    public static class CommunicationServiceCollectionExtensions
    {
        public static IServiceCollection AddServiceFactory<TService>(this IServiceCollection services, IConfiguration configuration)
            where TService : IFactoryConfig, new() =>
            services.Configure<ServiceConfig<TService>>(configuration.GetSection(typeof(TService).Name))
                .AddScoped<IServiceFactory<TService>, ServiceFactory<TService>>();
    }
}