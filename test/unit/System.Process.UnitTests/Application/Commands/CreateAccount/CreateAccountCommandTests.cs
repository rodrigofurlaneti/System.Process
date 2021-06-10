using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Process.Application.Commands.CreateAccount;
using System.Process.CrossCutting.DependencyInjection;
using System.Process.Domain.Enums;
using System.Process.Domain.Repositories;
using System.Process.Domain.ValueObjects;
using System.Process.Infrastructure.Configs;
using System.Process.Infrastructure.Messages;
using System.Phoenix.DataAccess.MongoDb;
using System.Phoenix.Event;
using System.Phoenix.Event.Kafka.Config;
using System.Phoenix.Event.Messages;
using System.Phoenix.Pipeline.Orchestrator;
using System.Phoenix.Pipeline.Orchestrator.Config;
using System.Phoenix.Pipeline.Orchestrator.Factory;
using System.Proxy.Fis.RegisterCard;
using System.Proxy.Salesforce;
using System.Proxy.Salesforce.GetBusinessInformation;
using System.Proxy.Salesforce.GetTerms;
using System.Proxy.Salesforce.GetToken;
using System.Proxy.Salesforce.GetToken.Messages;
using System.Proxy.Salesforce.Messages;
using System.Proxy.Salesforce.RegisterAsset;
using System.Proxy.Salesforce.Terms;
using System.Proxy.Salesforce.UpdateAsset;
using System.Proxy.Salesforce.UpdateAsset.Messages;
using System.Proxy.Silverlake.Base.Config;
using System.Proxy.Silverlake.Customer;
using System.Proxy.Silverlake.Customer.Common;
using System.Proxy.Silverlake.Customer.Messages.Request;
using System.Proxy.Silverlake.Customer.Messages.Response;
using System.Proxy.Silverlake.Deposit;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using FisToken = System.Proxy.Fis.GetToken;

namespace System.Process.UnitTests.Application.Commands.CreateAccount
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
        private Mock<IOptions<FisToken.Messages.GetTokenParams>> configFis;
        private Mock<IGetTokenClient> tokenClient;
        private Mock<FisToken.IGetTokenClient> tokenClientFis;
        private Mock<IRegisterAssetClient> registerAsset;
        private Mock<IUpdateAssetClient> updateAssetClient;
        private Mock<IOptions<RecordTypesConfig>> recordTypesConfig;
        private Mock<IRegisterCardClient> registerCardClient;
        private Mock<IOptions<HeaderParams>> headerParams;
        private Mock<ICardReadRepository> registerCardReadRepository;
        private Mock<ICardWriteRepository> registerCardWhiteRepository;
        private Mock<IOptions<ProcessConfig>> ProcessConfig;
        private Mock<IGetTermsClient> getTermsClient;
        private Mock<IGetBusinessInformationClient> getBusinessInformationClient;
        private Mock<ITermsClient> termsClient;

        #endregion

        #region Constructor
        public CreateAccountCommandTests()
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
            configFis = new Mock<IOptions<FisToken.Messages.GetTokenParams>>();
            headerParams = new Mock<IOptions<HeaderParams>>();
            tokenClient = new Mock<IGetTokenClient>();
            tokenClientFis = new Mock<FisToken.IGetTokenClient>();
            registerAsset = new Mock<IRegisterAssetClient>();
            recordTypesConfig = new Mock<IOptions<RecordTypesConfig>>();
            updateAssetClient = new Mock<IUpdateAssetClient>();
            registerCardClient = new Mock<IRegisterCardClient>();
            getTermsClient = new Mock<IGetTermsClient>();
            getBusinessInformationClient = new Mock<IGetBusinessInformationClient>();
            termsClient = new Mock<ITermsClient>();

            customerOperation
              .Setup(c => c.AccountIdGeneratorAsync(It.IsAny<AccountIdGeneratorRequest>(), It.IsAny<CancellationToken>()))
              .Returns(Task<AccountIdGeneratorResponse>.FromResult(
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
                  }));
        }

        #endregion

        #region Tests

        [Fact(DisplayName = "Should Send Handle Create Account", Skip = "true")]
        public async Task ShouldSendHandleCreateAccountAsync()
        {
            var createAccountCommand = new CreateAccountCommand(
                customerOperation.Object,
                depositOperation.Object,
                producerConfig,
                producer.Object,
                pipeline,
                logger.Object,
                config.Object,
                tokenClient.Object,
                configFis.Object,
                tokenClientFis.Object,
                registerAsset.Object,
                recordTypesConfig.Object,
                updateAssetClient.Object,
                registerCardClient.Object,
                headerParams.Object,
                registerCardWhiteRepository.Object,
                registerCardReadRepository.Object,
                ProcessConfig.Object,
                getTermsClient.Object,
                getBusinessInformationClient.Object,
                termsClient.Object);

            var cancellationToken = new CancellationToken(false);

            producerConfig.Value.TopicBlacklist = "test";
            var notification = CreateNotification(true);

            await createAccountCommand.Handle(notification, cancellationToken);
        }

        [Fact(DisplayName = "Should Send Handle Do Not Create Account", Skip = "true")]
        public async Task ShouldSendHandleDoNotCreateAccountAsync()
        {
            tokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(GetTokenBaseResult());
            updateAssetClient.Setup(x => x.UpdateAsset(It.IsAny<UpdateAssetParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetSalesforceResult());

            var createAccountCommand = new CreateAccountCommand(
                customerOperation.Object,
                depositOperation.Object,
                producerConfig,
                producer.Object,
                pipeline,
                logger.Object,
                config.Object,
                tokenClient.Object,
                configFis.Object,
                tokenClientFis.Object,
                registerAsset.Object,
                recordTypesConfig.Object,
                updateAssetClient.Object,
                registerCardClient.Object,
                headerParams.Object,
                registerCardWhiteRepository.Object,
                registerCardReadRepository.Object,
                ProcessConfig.Object,
                getTermsClient.Object,
                getBusinessInformationClient.Object,
                termsClient.Object);

            var cancellationToken = new CancellationToken(false);

            producerConfig.Value.TopicBlacklist = "test";
            var notification = CreateNotification(false);

            await createAccountCommand.Handle(notification, cancellationToken);
        }

        #endregion

        #region Methods

        private CreateAccountNotification CreateNotification(bool createAccount)
        {
            return new CreateAccountNotification
            {
                MessageContent = new MessageContent<AccountMessage>
                {
                    Payload = new AccountMessage
                    {
                        Process = new List<AccountInfo>
                        {
                            new AccountInfo
                            {
                                Number = "number",
                                Origin = OriginAccount.E,
                                RoutingNumber = "number",
                                Type = "type"
                            }
                        },
                        Documents = new List<Document>
                        {
                            new Document
                            {
                                AccountId = "string",
                                LeadId = "string",
                                Name = "string",
                                SynergyId = "string",
                                Type = "string",
                                URL = "string"
                            }
                        },
                        Principals = new List<Principal>
                        {
                            new Principal
                            {
                                Address = new Domain.ValueObjects.Address{},
                                Bankruptcy = false,
                                BankruptcyText = "none",
                                Cif = "cif",
                                Contacts = new List<Contact>
                                {
                                    new Contact()
                                },
                                DateOfBirth = DateTime.Now,
                                FirstName = "name",
                                LastName = "name",
                                MiddleName = "name",
                                NameSuffix = "sufix",
                                Principalship = 0,
                                TaxId = new TaxId {},
                                Title = "title"
                            }
                        },
                        ApplicationId = "Id",
                        BankAccount = new BankAccount
                        {
                            AccountNumber = "Number",
                            BankCode = "code",
                            RoutingNumber = "number"
                        },
                        BusinessCif = "Cif",
                        BusinessInformation = new BusinessInformation
                        {
                            DbaName = "name",
                            TaxId = new TaxId
                            {
                                Number = "number",
                                Type = "type"
                            },
                            EntityType = "type",
                            FormationCountry = "Country",
                            FormationDate = DateTime.Now,
                            FormationState = "State",
                            IndustryCode = "code",
                            LegalName = "Name",
                            NumberOfEmployees = 1,
                            Transaction = new Transaction
                            {
                                MonthlySales = 0,
                                MonthlyTransactions = 1
                            },
                            Website = "site"
                        },
                        BusinessRepresentative = false,
                        CustomerReturnPolicy = "none",
                        CustomerReturnPolicyText = "none",
                        Iso = new Iso
                        {
                            InternalContact = new InternalContact
                            {
                                FirstName = "name",
                                LastName = "name",
                                MiddleName = "name",
                                NameSuffix = "sufix"
                            },
                            Name = "name"
                        },
                        MerchantId = "id",
                        OnboardingStatus = OnboardingStatus.Success,
                        OpenCheckingAccount = createAccount,
                        Order = new Order
                        {
                            OrderNumber = "OrderNumber",
                            ShippingAddress = new Domain.ValueObjects.Address
                            {
                                City = "city",
                                Country = "country",
                                Line1 = "line",
                                Line2 = "line",
                                Line3 = "line",
                                State = "state",
                                Type = "type",
                                ZipCode = "code"
                            }
                        },
                        OriginChannel = OriginChannel.BankingApp,
                        PaperApplication = false,
                        Pricing = new System.Process.Domain.ValueObjects.Pricing
                        {
                            Type = "type"
                        },
                        ProcessStep = ProcessStep.CifCreated,
                        SalesforceId = "id",
                        UnderwritingProcess = new UnderwritingProcess
                        {
                            Level = 0,
                            Status = UnderwritingProcessStatus.Activated
                        }
                    },
                    Topic = "Topic"
                }
            };
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

        #endregion
    }
}

