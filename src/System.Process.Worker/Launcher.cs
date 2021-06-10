using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Process.Worker;
using System.Process.Worker.Commands;
using System.CustomerIds.Worker.Commands;
using System.Phoenix.Event;
using System.Phoenix.Event.Kafka.Clients;
using System.Phoenix.Event.Kafka.Config;
using System.Phoenix.Event.Kafka.Factories;
using Steeltoe.Extensions.Configuration.ConfigServer;
using System.Process.CrossCutting.DependencyInjection;
using System.Process.CrossCutting.Logging;

namespace System.Process.Worker
{
    public static class Launcher
    {
        #region Attributes

        private static readonly Dictionary<string, Action> Commands = new Dictionary<string, Action>();

        #endregion

        #region Methods

        public static void Main(string[] args)
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Local";

            var configuration = new ConfigurationBuilder()
                      .SetBasePath(Directory.GetCurrentDirectory())
                      .AddJsonFile("appsettings.json", true, true)
                      .AddEnvironmentVariables()
                      .AddConfigServer(environmentName)
                      .Build();

            new HostBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.SetBasePath(Directory.GetCurrentDirectory())
                          .AddJsonFile("appsettings.json", true, true)
                          .AddEnvironmentVariables()
                          .AddConfigServer(environmentName);
                })
                .ConfigureLogging()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddProxies(configuration);
                    services.AddProcessConfig(configuration);
                    services.AddSalesforce(configuration);
                    services.AddRecordTypes(configuration);
                    services.AddFis(configuration);
                    services.AddFeedzai(configuration);
                    services.AddRda(configuration);
                    services.AddRdaConfig(configuration);
                    services.AddRdaAdmin(configuration);
                    services.AddStatementsConfig(configuration);

                    services.AddOptions();
                    services.AddMediator();

                    services.AddSingleton(configuration);
                    services.AddCorrelationId();

                    services.AddPipeline<string>(configuration);

                    services.AddRtdx(configuration);

                    services.AddScoped<IProducer, KafkaProducer>();
                    services.AddScoped<IProducerFactory<long, string>, ProducerFactory<long, string>>();

                    services.AddScoped<IConsumer, KafkaConsumer>();
                    services.AddScoped<IConsumerFactory<long, string>, ConsumerFactory<long, string>>();

                    services.AddHostedService<WorkerService>();
                    services.AddRepositories(configuration);
                    services.AddOracleCardClient(configuration, "Receiver");
                    services.AddOracleTransferClient(configuration, "Receiver");

                    AddCommands(services, configuration);

                    ExecuteCommand();
                })
                .RunConsoleAsync();
        }

        private static void ExecuteCommand()
        {
            var commandName = Environment.GetEnvironmentVariable("COMMAND");
            //var commandName = "CreateCustomerId";

            if (!Commands.ContainsKey(commandName))
            {
                throw new InvalidOperationException(commandName);
            }

            Commands[commandName].Invoke();
        }

        private static void AddCommands(IServiceCollection services, IConfiguration configuration)
        {
            Commands.Add("CreateAccount", () =>
            {
                services.AddCommand(configuration, "CreateAccount");
                services.AddScoped<ICommand, CreateAccountCommand>();
            });
            Commands.Add("CreateCustomerId", () =>
            {
                services.AddCommand(configuration, "CreateCustomerId");
                services.AddScoped<ICommand, CreateCustomerIdCommand>();
            });
        }

        public static IServiceCollection AddCommand(this IServiceCollection services, IConfiguration configuration, string command)
        {
            var configSectionConsumer = configuration.GetSection($"Events:Consumers:{command}");
            var configConsumer = configSectionConsumer.Get<ConsumerConfig>();

            if (configConsumer == null)
            {
                throw new InvalidOperationException($"The configuration section 'Events:Consumers:{command}' was not found.");
            }

            services.Configure<ConsumerConfig>(configSectionConsumer);

            var configSectionProducer = configuration.GetSection($"Events:Producers:{command}");
            var configProducer = configSectionProducer.Get<ProducerConfig>();

            if (configProducer != null)
            {
                services.Configure<ProducerConfig>(configSectionProducer);
            }

            return services;
        }

        #endregion
    }
}