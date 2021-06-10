using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Process.Application.Commands.CreateCustomerId;
using System.Process.Base.IntegrationTests;
using System.Process.Domain.Containers;
using System.Process.Infrastructure.Configs;
using System.Process.Infrastructure.Messages;
using System.Phoenix.Common.Exceptions;
using System.Phoenix.Event;
using System.Phoenix.Event.Kafka.Config;
using System.Phoenix.Pipeline.Orchestrator;
using System.Proxy.RdaAdmin.AddAccount;
using System.Proxy.RdaAdmin.AddCutosmer;
using System.Proxy.RdaAdmin.GetProcessCriteriaReference;
using System.Proxy.RdaAdmin.GetCustomersCriteria;
using System.Proxy.RdaAdmin.UpdateAccount;
using System.Proxy.RdaAdmin.UpdateCustomer;
using System.Proxy.Salesforce.GetToken;
using System.Proxy.Salesforce.GetToken.Messages;
using System.Proxy.Salesforce.UpdateAsset;
using Xunit;

namespace System.Process.IntegrationTests.Application.Commands.CreateCustomerId
{
    public class CreateCustomerIdCommandTests
    {
        #region Properties

        private Mock<IOptions<RdaCredentialsConfig>> RdaConfig { get; set; }
        private Mock<IProducer> Producer { get; set; }
        private IOptions<ProducerConfig> ProducerConfig { get; }
        private Mock<ILogger<CreateCustomerIdCommand>> Logger { get; }
        private Mock<IGetCustomersCriteriaClient> GetCustomersCriteriaClient { get; }
        private Mock<IGetProcessCriteriaReferenceClient> GetProcessCriteriaReferenceClient { get; }
        private Mock<IAddCustomerClient> AddCustomerClient { get; }
        private Mock<IAddAccountClient> AddAccountClient { get; }
        private Mock<IUpdateCustomerClient> UpdateCustomerClient { get; }
        private Mock<IUpdateAccountClient> UpdateAccountClient { get; }
        private Mock<IUpdateAssetClient> UpdateAssetClient { get; }
        private Mock<IGetTokenClient> GetTokenClient { get; }
        private Mock<IOptions<GetTokenParams>> ConfigSalesforce { get; }
        private Mock<IPipeline<string>> Pipeline { get; }
        private Mock<PipelineMessageContainer<AccountMessage>> AccountMessage { get; set; }

        #endregion

        #region Constructor
        public CreateCustomerIdCommandTests()
        {
            RdaConfig = new Mock<IOptions<RdaCredentialsConfig>>();
            Producer = new Mock<IProducer>();
            ProducerConfig = Options.Create(new ProducerConfig());
            Logger = new Mock<ILogger<CreateCustomerIdCommand>>();
            GetCustomersCriteriaClient = new Mock<IGetCustomersCriteriaClient>();
            GetProcessCriteriaReferenceClient = new Mock<IGetProcessCriteriaReferenceClient>();
            AddCustomerClient = new Mock<IAddCustomerClient>();
            AddAccountClient = new Mock<IAddAccountClient>();
            UpdateCustomerClient = new Mock<IUpdateCustomerClient>();
            UpdateAccountClient = new Mock<IUpdateAccountClient>();
            UpdateAssetClient = new Mock<IUpdateAssetClient>();
            GetTokenClient = new Mock<IGetTokenClient>();
            ConfigSalesforce = new Mock<IOptions<GetTokenParams>>();
            Pipeline = new Mock<IPipeline<string>>();
            AccountMessage = new Mock<PipelineMessageContainer<AccountMessage>>();
    }

        #endregion

        #region Tests

        [Fact(DisplayName = "Should Send Handle Create CustomerId")]
        public async Task ShouldSendHandleCreateCustomerIdAsync()
        {
            var createCustomerIdCommand = new CreateCustomerIdCommand(
                RdaConfig.Object,
                Producer.Object,
                ProducerConfig,
                Logger.Object,
                Pipeline.Object,
                GetCustomersCriteriaClient.Object,
                AddCustomerClient.Object,
                AddAccountClient.Object,
                UpdateCustomerClient.Object,
                GetProcessCriteriaReferenceClient.Object,
                UpdateAccountClient.Object,
                GetTokenClient.Object,
                UpdateAssetClient.Object,              
                ConfigSalesforce.Object
                );

            var notification = ProcessIntegrationTestsConfiguration.ReadJson<CreateCustomerIdNotification>("CreateCustomerIdNotification.json");
            var cancellationToken = new CancellationToken(false);

            await createCustomerIdCommand.Handle(notification, cancellationToken);
        }

        [Fact(DisplayName = "Should Send Handle Create CustomerId Error")]
        public async Task ShouldSendHandleCreateCustomerIdErrorAsync()
        {
            var command = new CreateCustomerIdCommand(
               RdaConfig.Object,
               Producer.Object,
               ProducerConfig,
               Logger.Object,
               Pipeline.Object,
               GetCustomersCriteriaClient.Object,
               AddCustomerClient.Object,
               AddAccountClient.Object,
               UpdateCustomerClient.Object,
               GetProcessCriteriaReferenceClient.Object,
               UpdateAccountClient.Object,
               GetTokenClient.Object,
               UpdateAssetClient.Object,
               ConfigSalesforce.Object
               );

            var notification = new CreateCustomerIdNotification();
            var cancellationToken = new CancellationToken(false);

            await Assert.ThrowsAsync<UnprocessableEntityException>(() => command.Handle(notification, cancellationToken));
        }

        #endregion
    }
}
