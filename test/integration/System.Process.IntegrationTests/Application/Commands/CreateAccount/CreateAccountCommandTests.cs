using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Process.Application.Commands.CreateAccount;
using System.Process.Base.IntegrationTests;
using System.Process.CrossCutting.DependencyInjection;
using System.Process.Infrastructure.Configs;
using System.Phoenix.DataAccess.MongoDb;
using System.Phoenix.Event;
using System.Phoenix.Event.Kafka.Config;
using System.Phoenix.Pipeline.Orchestrator;
using System.Phoenix.Pipeline.Orchestrator.Config;
using System.Phoenix.Pipeline.Orchestrator.Factory;
using System.Proxy.Salesforce;
using System.Proxy.Salesforce.GetToken;
using System.Proxy.Salesforce.GetToken.Messages;
using System.Proxy.Salesforce.Messages;
using System.Proxy.Salesforce.RegisterAsset;
using System.Proxy.Salesforce.UpdateAsset;
using System.Proxy.Salesforce.UpdateAsset.Messages;
using System.Proxy.Silverlake.Base.Config;
using System.Proxy.Silverlake.Customer;
using System.Proxy.Silverlake.Customer.Common;
using System.Proxy.Silverlake.Customer.Messages.Request;
using System.Proxy.Silverlake.Customer.Messages.Response;
using System.Proxy.Silverlake.Deposit;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using worker = System.Process.Worker.Commands;

namespace System.Process.IntegrationTests.Application.Commands.CreateAccount
{
    public class CreateAccountCommandTests
    {
        #region Properties

        private ServiceCollection services;
        private Mock<ICustomerOperation> customerOperation;
        private Mock<IDepositOperation> depositOperation;
        private IOptions<ProducerConfig> producerConfig;
        private Mock<IProducer> producer;
        private Mock<ILogger<CreateAccountCommand>> logger;
        private IPipeline<string> pipeline;
        private Mock<IOptions<GetTokenParams>> config;
        private Mock<IGetTokenClient> tokenClient;
        private Mock<IRegisterAssetClient> registerAsset;
        private Mock<IUpdateAssetClient> updateAssetClient;
        private Mock<IOptions<RecordTypesConfig>> recordTypesConfig;
        private Mock<IOptions<HeaderParams>> headerParams;
        #endregion

        #region Constructor

        public CreateAccountCommandTests()
        {
            InitializeProperties();
        }

        #endregion

        #region Tests

        [Fact(DisplayName = "Should Create a new WorkerCommand - Success")]
        public void ShouldCreateNewWorkerCommandAsync()
        {
            var consumerConfig = new Mock<IOptions<ConsumerConfig>>();
            var consumer = new Mock<IConsumer>();
            var mediator = new Mock<IMediator>();
            var workerLogger = new Mock<ILogger<worker.CreateAccountCommand>>();

            var workerCommand = new worker.CreateAccountCommand(consumerConfig.Object, consumer.Object, mediator.Object, workerLogger.Object);

            Assert.NotNull(workerCommand);
        }

        [Fact(DisplayName = "Should Send Handle Create Account")]
        public void ShouldSendHandleCreateTrueAccount()
        {
            customerOperation
                .Setup(x => x.AccountIdGeneratorAsync(It.IsAny<AccountIdGeneratorRequest>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(
                    new AccountIdGeneratorResponse
                    {
                        AccountIdGeneratorInfo = new List<AccountIdGeneratorInfo>
                        {
                            new AccountIdGeneratorInfo
                            {
                                AccountId = "1234"
                            }
                            }
                    }));

            //var createAccountCommand = new CreateAccountCommand(
            //    customerOperation.Object,
            //    depositOperation.Object,
            //    producerConfig,
            //    producer.Object,
            //    pipeline,
            //    logger.Object,
            //    config.Object,
            //    tokenClient.Object,
            //    registerAsset.Object,
            //    recordTypesConfig.Object,
            //    updateAssetClient.Object,
            //    headerParams.Object);

            var cancellationToken = new CancellationToken(false);

            producerConfig.Value.TopicBlacklist = "test";
            var notification = CreateNotification(true);

            //await createAccountCommand.Handle(notification, cancellationToken);
        }

        [Fact(DisplayName = "Do Not Create Account - Success")]
        public void ShouldDoNotCreateProcessuccessfully()
        {
            tokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(GetTokenBaseResult());
            updateAssetClient.Setup(x => x.UpdateAsset(It.IsAny<UpdateAssetParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetSalesforceResult());
            customerOperation
             .Setup(c => c.AccountIdGeneratorAsync(It.IsAny<AccountIdGeneratorRequest>(), It.IsAny<CancellationToken>()))
             .Returns(GetCustomerOperation());

            headerParams.SetupGet(x => x.Value).Returns(
                new HeaderParams
                {
                    InstitutionRoutingId = "integratiotest"
                });

            //var createAccountCommand = new CreateAccountCommand(
            //    customerOperation.Object,
            //    depositOperation.Object,
            //    producerConfig,
            //    producer.Object,
            //    pipeline,
            //    logger.Object,
            //    config.Object,
            //    tokenClient.Object,
            //    registerAsset.Object,
            //    recordTypesConfig.Object,
            //    updateAssetClient.Object,
            //    headerParams.Object);

            var cancellationToken = new CancellationToken(false);

            producerConfig.Value.TopicBlacklist = "test";
            var notification = CreateNotification(false);

            //await createAccountCommand.Handle(notification, cancellationToken);

            Assert.NotEqual(notification, new CreateAccountNotification());
        }

        #endregion

        #region Methods

        private void InitializeProperties()
        {
            var appSettings = new Dictionary<string, string>
            {
                {"Pipeline:Retries", "2"},
                {"Pipeline:Delay", "2000"},
                {"MongoDB:Snapshot:ConnectionString", "mongodb://10.9.10.103:27017" },
                {"MongoDB:Snapshot:Database", "account"},
                {"MongoDB:Snapshot:Collection", "Snapshot"},
            };

            IConfiguration configuration = new ConfigurationBuilder()
                   .AddInMemoryCollection(appSettings)
                   .Build();

            var loggerPipeline = new Mock<ILogger<Pipeline<string>>>();
            var loggerFactory = new Mock<ILogger<TransformBlockFactory<string>>>();
            var loggerMongo = new Mock<ILogger<MongoDbClient<Snapshot<string>, string>>>();
            services = new ServiceCollection();
            services.Configure<PipelineConfig>(setting => configuration.GetSection("Pipeline").Bind(setting));
            services.AddSingleton(configuration);
            services.AddSingleton(loggerPipeline.Object);
            services.AddSingleton(loggerFactory.Object);
            services.AddSingleton(loggerMongo.Object);
            services.AddSingleton<IPipeline<string>, Pipeline<string>>();
            services.AddSingleton<ITransformBlockFactory<string>, TransformBlockFactory<string>>();
            services.AddMongoClient<Snapshot<string>, string>(configuration);
            services.AddPipeline<string>(configuration);

            customerOperation = new Mock<ICustomerOperation>();
            depositOperation = new Mock<IDepositOperation>();
            producerConfig = Options.Create(new ProducerConfig());
            producer = new Mock<IProducer>();
            logger = new Mock<ILogger<CreateAccountCommand>>();
            var serviceProvider = services.BuildServiceProvider();
            pipeline = serviceProvider.GetService<IPipeline<string>>();
            config = new Mock<IOptions<GetTokenParams>>();
            tokenClient = new Mock<IGetTokenClient>();
            registerAsset = new Mock<IRegisterAssetClient>();
            recordTypesConfig = new Mock<IOptions<RecordTypesConfig>>();
            updateAssetClient = new Mock<IUpdateAssetClient>();
            headerParams = new Mock<IOptions<HeaderParams>>();
        }
        private CreateAccountNotification CreateNotification(bool createAccount)
        {

            var notification = ProcessIntegrationTestsConfiguration.ReadJson<CreateAccountNotification>("CreateAccountNotification.json");

            notification.MessageContent.Payload.OpenCheckingAccount = createAccount;

            return notification;
        }
        private Task<BaseResult<GetTokenResult>> GetTokenBaseResult()
        {
            return Task.FromResult(new BaseResult<GetTokenResult>
            {
                IsSuccess = true,
                Result = new GetTokenResult
                {
                    AccessToken = "test"
                }
            });
        }
        private Task<BaseResult<SalesforceResult>> GetSalesforceResult()
        {
            return Task.FromResult(new BaseResult<SalesforceResult>
            {
                IsSuccess = true
            });
        }
        private Task<AccountIdGeneratorResponse> GetCustomerOperation()
        {
            return Task.FromResult(
                new AccountIdGeneratorResponse
                {
                    AccountIdGeneratorInfo = new List<AccountIdGeneratorInfo>
                    {
                        new AccountIdGeneratorInfo
                        {
                            AccountId = "accountId"
                        }
                    },
                    Process = new List<Proxy.Silverlake.Customer.Messages.Response.Process>
                    {
                        new Proxy.Silverlake.Customer.Messages.Response.Process
                        {
                            AccountId = "id",
                            AccountType = "type"
                        }
                    }
                });
        }

        #endregion
    }
}
