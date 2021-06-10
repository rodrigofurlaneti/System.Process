using Microsoft.Extensions.DependencyInjection;
using System.Phoenix.Common.CorrelationId;
using System.Phoenix.Communication;

namespace System.Process.CrossCutting.DependencyInjection
{
    public static class CorrelationServiceCollectionExtensions
    {
        public static IServiceCollection AddCorrelationId(this IServiceCollection services)
        {
            services.AddSingleton<ICorrelationContextAccessor, CorrelationContextAccessor>();
            services.AddTransient<ICorrelationContextFactory, CorrelationContextFactory>();
            services.AddTransient<CorrelationIdDelegatingHandler>();
            services.AddTransient<UserAgentDelegatingHandler>();
            return services;
        }
    }
}