using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Process.CrossCutting.DependencyInjection;
using System.Collections.Generic;
using Xunit;

namespace System.Process.UnitTests.CrossCutting.DependencyInjection
{
    public class EventServiceCollectionExtensionsTests
    {
        [Fact(DisplayName = "Should Send AddKafka Successfully")]
        public void ShouldSendProspectAddKafkaClientSuccessfullyAsync()
        {
            var appSettings = new Dictionary<string, string>
            {
                 {"Events:Producers:CreateAccount:BootstrapServers", "10.9.10.102:9092" },
                 { "Events:Producers:CreateAccount:Topic", "onb_create_account_completed" }
            };

            IConfiguration configuration = new ConfigurationBuilder()
                  .AddInMemoryCollection(appSettings)
                  .Build();

            var services = new ServiceCollection();

            EventServiceCollectionExtensions.AddKafka(services, configuration);
        }
    }
}
