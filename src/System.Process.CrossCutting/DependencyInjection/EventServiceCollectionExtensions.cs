using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Phoenix.Event;
using System.Phoenix.Event.Kafka.Clients;
using System.Phoenix.Event.Kafka.Config;
using System.Phoenix.Event.Kafka.Factories;

namespace System.Process.CrossCutting.DependencyInjection
{
    public static class EventServiceCollectionExtensions
    {
        public static IServiceCollection AddKafka(this IServiceCollection services, IConfiguration configuration)
        {
            var configSection = configuration.GetSection($"Events:Producers:CreateAccount");
            var config = configSection.Get<ProducerConfig>();

            if (config == null)
            {
                throw new InvalidOperationException($"The configuration section 'Events:Producers:CreateAccount' was not found.");
            }

            services.Configure<ProducerConfig>(configSection);
            services.AddScoped<IProducer, KafkaProducer>();
            services.AddScoped<IProducerFactory<long, string>, ProducerFactory<long, string>>();

            return services;
        }
    }
}