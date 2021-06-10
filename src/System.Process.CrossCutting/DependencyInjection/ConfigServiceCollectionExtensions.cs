using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Process.Application.Clients.Jarvis;
using System.Process.Domain.ValueObjects;
using System.Phoenix.Communication;

namespace System.Process.CrossCutting.DependencyInjection
{
    public static class ConfigServiceCollectionExtensions
    {
        public static IServiceCollection AddProcessConfig(this IServiceCollection services, IConfiguration configuration)
        {
            var configSection = configuration.GetSection($"ProcessConfig");
            var config = configSection.Get<ProcessConfig>();

            if (config == null)
            {
                throw new InvalidOperationException($"The configuration section 'ProcessConfig' was not found.");
            }

            services.Configure<ProcessConfig>(configSection);

            return services;
        }

        public static IServiceCollection AddJarvisConfig(this IServiceCollection services, IConfiguration configuration)
        {
            var configSection = configuration.GetSection($"JarvisConfig");
            var config = configSection.Get<JarvisConfig>();

            if (config == null)
            {
                throw new InvalidOperationException($"The configuration section 'Jarvis' was not found.");
            }

            services.Configure<JarvisConfig>(configSection);

            services.AddHttpClient<IJarvisClient, JarvisClient>()
                    .ConfigurePrimaryHttpMessageHandler(x => new DefaultHttpClientHandler())
                    .AddHttpMessageHandler<CorrelationIdDelegatingHandler>()
                    .AddHttpMessageHandler<UserAgentDelegatingHandler>()
                    .AddPolicyHandlerFromRegistry(PolicyName.HttpRetry)
                    .AddPolicyHandlerFromRegistry(PolicyName.HttpCircuitBreaker);

            return services;
        }
    }
}
