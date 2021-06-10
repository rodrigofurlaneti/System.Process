using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Polly.Registry;
using System.Phoenix.Communication;
using System.Phoenix.Communication.Config;

namespace System.Process.CrossCutting.DependencyInjection
{
    public static class PoliciesServiceCollectionExtensions
    {
        public static IServiceCollection AddPolicies(this IServiceCollection services, IConfiguration configuration)
        {
            var config = configuration.GetSection("Policies");
            services.Configure<PolicyConfig>(configuration);

            if (config == null)
                throw new InvalidOperationException($"The configuration section 'Policies' was not found.");

            var policyRegistry = services.AddPolicyRegistry();

            RegisterPolicy(config.Get<PolicyConfig>(), policyRegistry);

            return services;
        }

        private static void RegisterPolicy(PolicyConfig policyOptions, IPolicyRegistry<string> policyRegistry)
        {
            policyRegistry.Add(
                            PolicyName.HttpRetry,
                            HttpPolicyExtensions
                                .HandleTransientHttpError()
                                .WaitAndRetryAsync(
                                    policyOptions.HttpRetry.Count,
                                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));

            policyRegistry.Add(
                PolicyName.HttpCircuitBreaker,
                HttpPolicyExtensions
                    .HandleTransientHttpError()
                    .CircuitBreakerAsync(
                        handledEventsAllowedBeforeBreaking: policyOptions.HttpCircuitBreaker.ExceptionsAllowedBeforeBreaking,
                        durationOfBreak: policyOptions.HttpCircuitBreaker.DurationOfBreak));
        }
    }
}
