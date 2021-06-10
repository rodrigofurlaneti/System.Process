using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Process.CrossCutting.DependencyInjection;
using System.Collections.Generic;
using Xunit;

namespace System.Process.UnitTests.CrossCutting.DependencyInjection
{
    public class ProxyServiceCollectionExtensionsTests
    {
        [Fact(DisplayName = "Should Send AddProxies Successfully")]
        public void ShouldSendProspectAddProxiesSuccessfullyAsync()
        {
            var appSettings = new Dictionary<string, string>
            {
                {"JHFactoryConfig:Url", "http://10.9.11.128/jarvis/"},
                { "JackHenry:JackHenryConfig:Url", "" },
                { "JackHenry:JackHenryConfig:Username", "" },
                { "JackHenry:JackHenryConfig:Password", "" },
                { "JackHenry:HeaderParams:AuditUserId", "System" },
                { "JackHenry:HeaderParams:AuditWorkstationId", "IDG" },
                { "JackHenry:HeaderParams:ErrorCode", "500019" },
                { "JackHenry:HeaderParams:InstitutionEnvironment", "UAT" },
                { "JackHenry:HeaderParams:InstitutionRoutingId", "026008413" },
                { "JackHenry:HeaderParams:ValidConsumerName", "System" },
                { "JackHenry:HeaderParams:ValidConsumerProduct", "IDA" }
            };

            IConfiguration configuration = new ConfigurationBuilder()
                  .AddInMemoryCollection(appSettings)
                  .Build();

            var services = new ServiceCollection();

            ProxyServiceCollectionExtensions.AddProxies(services, configuration);
        }
    }
}
