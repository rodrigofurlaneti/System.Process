using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Process.CrossCutting.DependencyInjection;
using System.Phoenix.Pipeline.Orchestrator;
using System.Collections.Generic;
using Xunit;

namespace System.Process.UnitTests.CrossCutting.DependencyInjection
{
    public class MongoDbServiceCollectionExtensionsTests
    {

        [Fact(DisplayName = "Should Send AddMongoClient Successfully")]
        public void ShouldSendProspectAddMongoClientSuccessfullyAsync()
        {
            var appSettings = new Dictionary<string, string>
            {
                {"MongoDb:Snapshot:ConnectionString", "mongodb://Systembackenddev:9mq5PhjGogqzip7YxDxZgTmxHS5LBJ77iq0vYYNKEvnEGW8Zq4cUOUmnXzPAoSxlJWKBpt7MV2NO21QkAQNQEQ==@Systembackenddev.documents.azure.com:10255/?ssl=true"}
            };

            IConfiguration configuration = new ConfigurationBuilder()
                  .AddInMemoryCollection(appSettings)
                  .Build();

            var services = new ServiceCollection();

            MongoDbServiceCollectionExtensions.AddMongoClient<Snapshot<string>, string>(services, configuration);
        }
    }
}
