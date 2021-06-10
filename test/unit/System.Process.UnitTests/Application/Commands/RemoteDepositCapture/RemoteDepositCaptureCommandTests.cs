using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Process.Application.Clients.Jarvis;
using System.Process.Application.Commands.RemoteDepositCapture;
using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Process.Domain.ValueObjects;
using System.Process.Infrastructure.Configs;
using System.Process.UnitTests.Common;
using System.Phoenix.Common.Exceptions;
using System.Proxy.Feedzai.Base.Config;
using System.Proxy.Feedzai.TransferInitiation;
using System.Proxy.Feedzai.TransferInitiation.Messages;
using System.Proxy.Rda.AddItem;
using System.Proxy.Rda.AddItem.Messages;
using System.Proxy.Rda.Authenticate;
using System.Proxy.Rda.Authenticate.Messages;
using System.Proxy.Rda.Common;
using System.Proxy.Rda.CreateBatch;
using System.Proxy.Rda.CreateBatch.Messages;
using System.Proxy.Rda.UpdateBatch;
using System.Proxy.Rda.UpdateBatch.Messages;
using System.Proxy.RdaAdmin.GetProcessCriteriaReference;
using System.Proxy.RdaAdmin.GetProcessCriteriaReference.Messages;
using System.Proxy.RdaAdmin.GetCustomersCriteria;
using System.Proxy.RdaAdmin.GetCustomersCriteria.Messages;
using System.Proxy.RdaAdmin.Messages;
using System.Proxy.Rdc.Common.Messages;
using System.Proxy.Salesforce.GetAccountInformations;
using System.Proxy.Salesforce.GetAccountInformations.Messages;
using System.Proxy.Salesforce.GetToken;
using System.Proxy.Salesforce.GetToken.Messages;
using System.Proxy.Salesforce.Messages;
using System.Proxy.Salesforce.SearchAddress;
using System.Proxy.Salesforce.SearchAddress.Messages;
using Xunit;

namespace System.Process.UnitTests.Application.Commands.RemoteDepositCapture
{
    public class RemoteDepositCaptureCommandTests
    {
        #region Properties

        private Mock<ILogger<RemoteDepositCaptureCommand>> Logger { get; }
        private Mock<IGetCustomersCriteriaClient> GetCustomersCriteriaClient { get; }
        private Mock<IGetProcessCriteriaReferenceClient> GetProcessCriteriaReferenceClient { get; }
        private Mock<IAuthenticateClient> AuthenticateClient { get; }
        private Mock<ICreateBatchClient> CreateBatchClient { get; }
        private Mock<IAddItemClient> AddItemClient { get; }
        private Mock<IUpdateBatchClient> UpdateBatchClient { get; }
        private Mock<ITransferInitiationClient> TransferInitiationClient { get; }
        private Mock<IGetAccountInformationsClient> GetAccountInformationsClient { get; }
        private Mock<IGetTokenClient> GetTokenClient { get; }
        private Mock<IOptions<GetTokenParams>> ConfigSalesforce { get; }
        private Mock<IOptions<RdaCredentialsConfig>> RdaCredentialsConfig { get; set; }
        private Mock<RemoteDepositCaptureRequest> Request { get; set; }
        private Mock<IOptions<FeedzaiConfig>> FeedzaiConfig { get; set; }
        private Mock<IOptions<ProcessConfig>> ProcessConfig { get; set; }
        private Mock<ISearchAddressClient> GetSearchAddressClient { get; set; }
        private Mock<IJarvisClient> JarvisClient { get; }
        private Mock<ITransferWriteRepository> TransferWriteRepository { get; set; }
        private Mock<ITransferItemWriteRepository> TransferItemWriteRepository { get; set; }

        #endregion

        #region Constructor

        public RemoteDepositCaptureCommandTests()
        {
            Logger = new Mock<ILogger<RemoteDepositCaptureCommand>>();
            GetCustomersCriteriaClient = new Mock<IGetCustomersCriteriaClient>();
            GetProcessCriteriaReferenceClient = new Mock<IGetProcessCriteriaReferenceClient>();
            AuthenticateClient = new Mock<IAuthenticateClient>();
            CreateBatchClient = new Mock<ICreateBatchClient>();
            AddItemClient = new Mock<IAddItemClient>();
            UpdateBatchClient = new Mock<IUpdateBatchClient>();
            TransferInitiationClient = new Mock<ITransferInitiationClient>();
            GetAccountInformationsClient = new Mock<IGetAccountInformationsClient>();
            GetTokenClient = new Mock<IGetTokenClient>();
            GetSearchAddressClient = new Mock<ISearchAddressClient>();
            ConfigSalesforce = new Mock<IOptions<GetTokenParams>>();
            RdaCredentialsConfig = new Mock<IOptions<RdaCredentialsConfig>>();
            RdaCredentialsConfig
                .Setup(p => p.Value)
                .Returns(ConvertJson.ReadJson<RdaCredentialsConfig>("RdaCredentialsConfig.json"));
            Request = new Mock<RemoteDepositCaptureRequest>();
            FeedzaiConfig = new Mock<IOptions<FeedzaiConfig>>();
            FeedzaiConfig
                .Setup(x => x.Value)
                .Returns(new FeedzaiConfig { Token = "123" });
            ProcessConfig = new Mock<IOptions<ProcessConfig>>();
            ProcessConfig
                .Setup(p => p.Value)
                .Returns(ConvertJson.ReadJson<ProcessConfig>("ProcessConfig.json"));
            JarvisClient = new Mock<IJarvisClient>();
            JarvisClient.Setup(x => x.GetDeviceDetails(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DeviceDetails
                {
                    IpAddress = "fe80::9088:a0a2:9e3d:47e6",
                    Platform = "iOS",
                    OsVersion = "13.5.1",
                    Latitude = "-23.706879",
                    Longitude = "-46.539313",
                    Altitude = "799.602993",
                    MacAddress = "4690FA88-FF6C-48CC-A205-5B687B46C7A8",
                    Model = "iPhone 7 (GSM+CDMA)"
                });
            TransferWriteRepository = new Mock<ITransferWriteRepository>();
            TransferItemWriteRepository = new Mock<ITransferItemWriteRepository>();
        }

        #endregion

        #region Tests

        [Fact(DisplayName = "Should Send Handle Remote Deposit Capture")]
        public async Task ShouldSendHandleRemoteDepositCaptureAsync()
        {
            var command = new RemoteDepositCaptureCommand(
                Logger.Object,
                GetCustomersCriteriaClient.Object,
                GetProcessCriteriaReferenceClient.Object,
                AuthenticateClient.Object,
                CreateBatchClient.Object,
                AddItemClient.Object,
                UpdateBatchClient.Object,
                TransferInitiationClient.Object,
                GetAccountInformationsClient.Object,
                GetTokenClient.Object,
                JarvisClient.Object,
                ConfigSalesforce.Object,
                RdaCredentialsConfig.Object,
                FeedzaiConfig.Object,
                ProcessConfig.Object,
                GetSearchAddressClient.Object,
                TransferWriteRepository.Object,
                TransferItemWriteRepository.Object
              );    

            GetTokenClient
                .Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(GetBaseTokenResult()));
            GetAccountInformationsClient
                .Setup(x => x.GetAccount(It.IsAny<GetAccountInformationsParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(GetAccountInformationsResult()));

            GetCustomersCriteriaClient
                .Setup(x => x.GetCustomersCriteria(It.IsAny<GetCustomersCriteriaParams>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(GetCustomersCriteriaResult()));

            AuthenticateClient
                .Setup(x => x.Authenticate(It.IsAny<AuthenticateRequest>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(GetAuthenticateResult()));

            GetProcessCriteriaReferenceClient
                .Setup(x => x.GetProcessCriteriaReference(It.IsAny<GetProcessCriteriaReferenceParams>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(GetProcessCriteriaReferenceResult()));

            TransferInitiationClient
                .Setup(x => x.TransferInitiation(It.IsAny<TransferInitiationParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(GetTransferInitiationResult()));

            CreateBatchClient
                .Setup(x => x.CreateBatch(It.IsAny<CreateBatchRequest>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(GetCreateBatchResult()));

            AddItemClient
                .Setup(x => x.AddItem(It.IsAny<AddItemRequest>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(GetAddItemResult()));

            UpdateBatchClient
                .Setup(x => x.UpdateBatch(It.IsAny<UpdateBatchRequest>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(GetUpdateBatchResult()));

            GetSearchAddressClient
                .Setup(x => x.SearchAddress(It.IsAny<SearchAddressParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(GetSearchAddressResult()));

            TransferWriteRepository
                .Setup(x => x.Add(It.IsAny<Transfer>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(new { }));

            TransferItemWriteRepository
               .Setup(x => x.Add(It.IsAny<TransferItem>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(new { }));


            var request = ConvertJson.ReadJson<RemoteDepositCaptureRequest>("RemoteDepositCaptureRequest.json");

            var cancellationToken = new CancellationToken();

            var result = await command.Handle(request, cancellationToken);

            result.Should().NotBeNull();
        }

        [Fact(DisplayName = "Should Send Handle Remote Deposit Capture Error")]
        public async Task ShouldSendUnprocessableEntityException()
        {
            var command = new RemoteDepositCaptureCommand(
                 Logger.Object,
                 GetCustomersCriteriaClient.Object,
                 GetProcessCriteriaReferenceClient.Object,
                 AuthenticateClient.Object,
                 CreateBatchClient.Object,
                 AddItemClient.Object,
                 UpdateBatchClient.Object,
                 TransferInitiationClient.Object,
                 GetAccountInformationsClient.Object,
                 GetTokenClient.Object,
                 JarvisClient.Object,
                 ConfigSalesforce.Object,
                 RdaCredentialsConfig.Object,
                 FeedzaiConfig.Object,
                 ProcessConfig.Object,
                 GetSearchAddressClient.Object,
                 TransferWriteRepository.Object,
                 TransferItemWriteRepository.Object
               );

            var request = ConvertJson.ReadJson<RemoteDepositCaptureRequest>("RemoteDepositCaptureRequest.json");
            var cancellationToken = new CancellationToken();

            await Assert.ThrowsAsync<UnprocessableEntityException>(() => command.Handle(request, cancellationToken));
        }

        #endregion

        #region Methods

        private Proxy.Salesforce.Messages.BaseResult<GetTokenResult> GetBaseTokenResult()
        {
            return new Proxy.Salesforce.Messages.BaseResult<GetTokenResult>
            {
                IsSuccess = true,
                Result = new GetTokenResult
                {
                    AccessToken = "D923GDM9-2943-9385-6B98-HFJC8742X901"
                }
            };
        }

        private Proxy.Salesforce.Messages.BaseResult<Proxy.Salesforce.Messages.QueryResult<GetAccountInformationsResponse>> GetAccountInformationsResult()
        {
            return new Proxy.Salesforce.Messages.BaseResult<Proxy.Salesforce.Messages.QueryResult<GetAccountInformationsResponse>>
            {
                IsSuccess = true,
                ItemReferenceId = "123",
                Result = new Proxy.Salesforce.Messages.QueryResult<GetAccountInformationsResponse>
                {
                    Records = new List<GetAccountInformationsResponse>
                    {
                        new GetAccountInformationsResponse
                        {
                            SystemId = "1234554321"
                        }
                    },

                }
            };
        }

        private AdminBaseResult<GetCustomersCriteriaResponse> GetCustomersCriteriaResult()
        {
            return new AdminBaseResult<GetCustomersCriteriaResponse>
            {
                Result = new GetCustomersCriteriaResponse
                {
                    Customers = new List<Proxy.RdaAdmin.Common.Customers>
                     {
                         new Proxy.RdaAdmin.Common.Customers
                         {
                             HomeBankingId = "1234554321",
                             IsEnabled = true
                         }
                     },
                    Result = 1
                }
            };
        }

        private Proxy.Rda.Messages.BaseResult<AuthenticateResponse> GetAuthenticateResult()
        {
            return new Proxy.Rda.Messages.BaseResult<AuthenticateResponse>
            {
                Result = new AuthenticateResponse
                {
                    RequestId = "123",
                    ValidationResults = new List<ValidationResult>(),
                    Credentials = new TokenCredentials
                    {
                        SecurityToken = "123",
                        Type = "123"
                    }
                },
            };
        }

        private AdminBaseResult<GetProcessCriteriaReferenceResponse> GetProcessCriteriaReferenceResult()
        {
            return new AdminBaseResult<GetProcessCriteriaReferenceResponse>
            {
                Result = new GetProcessCriteriaReferenceResponse
                {
                    Process = new List<Proxy.RdaAdmin.Common.Process>
                    {
                        new Proxy.RdaAdmin.Common.Process
                        {
                            IsEnabled = true,
                            AccountNumber = "33335114"
                        }
                    },
                    ValidationResults = new List<Proxy.RdaAdmin.Messages.ErrorMessage>()
                }
            };
        }

        private Proxy.Feedzai.Base.Messages.BaseResult<TransferInitiationResult> GetTransferInitiationResult()
        {
            return new Proxy.Feedzai.Base.Messages.BaseResult<TransferInitiationResult>
            {
                IsSuccess = true,
                Result = new TransferInitiationResult
                {
                    Status = "Success",
                    Decision = "success",
                    Errors = new List<string>()
                }
            };
        }

        private Proxy.Rda.Messages.BaseResult<CreateBatchResponse> GetCreateBatchResult()
        {
            return new Proxy.Rda.Messages.BaseResult<CreateBatchResponse>
            {
                Result = new CreateBatchResponse
                {
                    Batch = new Batch
                    {
                        BatchReference = "abc123"
                    },
                    ValidationResults = new List<ValidationResult>()
                }
            };
        }

        private Proxy.Rda.Messages.BaseResult<AddItemResponse> GetAddItemResult()
        {
            return new Proxy.Rda.Messages.BaseResult<AddItemResponse>
            {
                Result = new AddItemResponse
                {
                    RequestId = "123",
                    ValidationResults = new List<ValidationResults>(),
                    Item = new Item
                    {
                        Amount = 125
                    }
                }
            };
        }

        private Proxy.Rda.Messages.BaseResult<UpdateBatchResponse> GetUpdateBatchResult()
        {
            return new Proxy.Rda.Messages.BaseResult<UpdateBatchResponse>
            {
                Result = new UpdateBatchResponse
                {
                    RequestId = "123",
                    ValidationResults = new List<ValidationResult>(),
                    Batch = new Batch
                    {
                        StatusDescription = "Submitted"
                    }
                }
            };
        }

        private Proxy.Salesforce.Messages.BaseResult<QueryResult<SearchAddressResponse>> GetSearchAddressResult()
        {
            return new Proxy.Salesforce.Messages.BaseResult<QueryResult<SearchAddressResponse>>
            {
                Result = new QueryResult<SearchAddressResponse>
                {
                    Records = new List<SearchAddressResponse>
                    {   new SearchAddressResponse
                        {
                            Account = new Account
                            {
                                CorporateAddress1 = "CorporateAddress1",
                                CorporateCity = "CorporateCity",
                                CorporateCountry = "CorporateCountry",
                                CorporateState = "CorporateState",
                                CorporateZipCode = "CorporateZipCode",
                                LegalName = "LegalName",
                                SalesforceId = "SalesforceId"
                            },
                            AddressLine1 = "AddressLine1",
                            City = "City",
                            Country = "Country",
                            Email = "Email",
                            Name = "Name" ,
                            State = "State",
                            Zip = "Zip"
                        }
                    }
                }
            };
        }

        #endregion
    }
}
