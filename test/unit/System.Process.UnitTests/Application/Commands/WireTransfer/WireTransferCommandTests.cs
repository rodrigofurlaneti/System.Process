using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Process.Api.Controllers.V1;
using System.Process.Application.Clients.Jarvis;
using System.Process.Application.Commands.WireTransfer;
using System.Process.Base.IntegrationTests;
using System.Process.CrossCutting.DependencyInjection;
using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Process.Domain.ValueObjects;
using System.Process.UnitTests.Common;
using System.Phoenix.Common.Exceptions;
using System.Proxy.Feedzai.Base.Config;
using System.Proxy.Feedzai.Base.Messages;
using System.Proxy.Feedzai.TransferInitiation;
using System.Proxy.Feedzai.TransferInitiation.Messages;
using System.Proxy.Salesforce.GetToken;
using System.Proxy.Salesforce.GetToken.Messages;
using System.Proxy.Salesforce.Messages;
using System.Proxy.Salesforce.SearchAddress;
using System.Proxy.Salesforce.SearchAddress.Messages;
using System.Proxy.Silverlake.Base.Exceptions;
using System.Proxy.Silverlake.Inquiry;
using System.Proxy.Silverlake.Inquiry.Common;
using System.Proxy.Silverlake.Inquiry.Messages.Request;
using System.Proxy.Silverlake.Inquiry.Messages.Response;
using System.Proxy.Silverlake.TranferWire;
using System.Proxy.Silverlake.TranferWire.Messages.Request;
using System.Proxy.Silverlake.TranferWire.Messages.Response;
using System.Proxy.Silverlake.Transaction;
using System.Proxy.Silverlake.Transaction.Messages;
using System.Proxy.Silverlake.Transaction.Messages.Request;
using System.Proxy.Silverlake.Transaction.Messages.Response;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace System.Process.IntegrationTests.Application.Commands.WireTransfer
{
    public class WireTransferCommandTests
    {
        #region Properties

        private static ServiceCollection Services;
        private static ServiceProvider Provider;
        private static Mock<IHttpContextAccessor> HttpContextAccessor;

        private Mock<ILogger<WireTransferCommand>> Logger { get; set; }
        private Mock<IOptions<ProcessConfig>> ProcessConfig { get; set; }
        private Mock<ITransferWireOperation> TransferWireOperation { get; set; }
        private Mock<ITransferInitiationClient> TransferInitiationClient { get; set; }
        private Mock<IOptions<FeedzaiConfig>> FeedzaiConfig { get; set; }
        private Mock<IInquiryOperation> InquiryOperation { get; set; }
        private Mock<ITransactionOperation> TransactionOperation { get; set; }
        private Mock<IReceiverReadRepository> ReceiverReadRepository { get; set; }
        private Mock<IGetTokenClient> TokenClient { get; set; }
        private Mock<ISearchAddressClient> SearchAddressClient { get; set; }
        private Mock<IOptions<GetTokenParams>> SalesforceTokenParams { get; set; }
        private Mock<IJarvisClient> JarvisClient { get; }
        private Mock<ITransferWriteRepository> TransferWriteRepository { get; set; }

        #endregion

        #region Constructor

        public WireTransferCommandTests()
        {
            Logger = new Mock<ILogger<WireTransferCommand>>();
            TransferWireOperation = new Mock<ITransferWireOperation>();

            TransferInitiationClient = new Mock<ITransferInitiationClient>();
            TransferInitiationClient
                .Setup(x => x.TransferInitiation(It.IsAny<TransferInitiationParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(GeTransferInitiationResponse());

            InquiryOperation = new Mock<IInquiryOperation>();
            InquiryOperation
                .Setup(x => x.ProcessearchAsync(It.IsAny<ProcessearchRequest>(), It.IsAny<CancellationToken>()))
                .Returns(GetProcessearchResponse());

            TransactionOperation = new Mock<ITransactionOperation>();
            TransactionOperation
            .Setup(x => x.StopCheckAddAsync(It.IsAny<StopCheckAddRequest>(), It.IsAny<CancellationToken>()))
            .Returns(GetStopCheckAddResponse());
            TransactionOperation
           .Setup(x => x.StopCheckCancelAsync(It.IsAny<StopCheckCancelRequest>(), It.IsAny<CancellationToken>()))
           .Returns(GetStopCheckCancelResponse());
            ReceiverReadRepository = new Mock<IReceiverReadRepository>();
            ReceiverReadRepository
                .Setup(x => x.Find(It.IsAny<int>()))
                .Returns(new List<Receiver> {
                    new Receiver
                    {
                        CustomerId = "DAA0001",
                        AccountNumber = "100000037",
                        AccountType = "D",
                        RoutingNumber = "026003023"
                    }
                });

            FeedzaiConfig = new Mock<IOptions<FeedzaiConfig>>();
            FeedzaiConfig.Setup(p => p.Value)
                .Returns(new FeedzaiConfig
                {
                    Token = "344j43h4",
                    Url = "httptest"
                });
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

            SalesforceTokenParams = new Mock<IOptions<GetTokenParams>>();
            SalesforceTokenParams.Setup(x => x.Value).Returns(new GetTokenParams { ClientId = "ClientId", ClientSecret = "ClientSecret", GrantType = "GrantType", Password = "Password", Username = "Username" });
            SearchAddressClient = new Mock<ISearchAddressClient>();
            SearchAddressClient.Setup(x => x.SearchAddress(It.IsAny<SearchAddressParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(GetSearchAddressResult()));
            TokenClient = new Mock<IGetTokenClient>();
            TokenClient
                .Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(GetBaseTokenResult()));

            ProcessConfig = new Mock<IOptions<ProcessConfig>>();
            ProcessConfig.Setup(p => p.Value).Returns(ProcessIntegrationTestsConfiguration.ReadJson<ProcessConfig>("ProcessConfig.json"));
            Services = new ServiceCollection();
            Services.AddMediator();
            Services.AddScoped(typeof(ILoggerFactory), typeof(LoggerFactory));
            Services.AddScoped(typeof(ILogger<>), typeof(Logger<>));
            Provider = Services.BuildServiceProvider();
            TransferWriteRepository = new Mock<ITransferWriteRepository>();
        }

        #endregion

        #region Tests

        [Trait("Integration", "Success")]
        [Fact(DisplayName = "WireTransferCommand_Success", Skip = "true")]
        public async void ShouldTranserMoneyWireSuccessfully()
        {
            var transferWireResponse = GetTransferWireAddResponse();

            var ProcessearchResponse = GetProcessearchResponse();
            var receivers = new List<Receiver>
            {
                new Receiver
                {
                   CustomerId = "DAA0001",
                   AccountNumber = "100000037",
                   AccountType = "D",
                   RoutingNumber = "026003023"
                }
            };

            TransferInitiationClient
                .Setup(t => t.TransferInitiation(It.IsAny<TransferInitiationParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetTransferInitiationResult);
            ReceiverReadRepository
                .Setup(t => t.Find(It.IsAny<int>())).Returns(receivers);
            InquiryOperation.Setup(x => x.ProcessearchAsync(It.IsAny<ProcessearchRequest>(), It.IsAny<CancellationToken>())).Returns(ProcessearchResponse);

            TransferWireOperation.Setup(x => x.TransferWireAddAsync(It.IsAny<TransferWireAddRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(transferWireResponse);

            Services.AddSingleton(Logger.Object);
            Services.AddSingleton(TransferWireOperation.Object);
            Services.AddSingleton(TransferInitiationClient.Object);
            Services.AddSingleton(FeedzaiConfig.Object);
            Services.AddSingleton(InquiryOperation.Object);
            Services.AddSingleton(ReceiverReadRepository.Object);
            Services.AddSingleton(TokenClient.Object);
            Services.AddSingleton(SearchAddressClient.Object);
            Services.AddSingleton(SalesforceTokenParams.Object);
            Services.AddSingleton(ProcessConfig.Object);
            Services.AddSingleton(JarvisClient.Object);
            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<TransferController>>();
            var mediator = GetInstance<IMediator>();
            var controller = new TransferController(mediator, logger.Object);
            ProcessConfig.Setup(p => p.Value).Returns(ProcessIntegrationTestsConfiguration.ReadJson<ProcessConfig>("ProcessConfig.json"));

            var transferMoneyValidator = new WireTransferCommand(
                Logger.Object, 
                TransferWireOperation.Object,
                ProcessConfig.Object, 
                TransferInitiationClient.Object, 
                FeedzaiConfig.Object, 
                InquiryOperation.Object,
                ReceiverReadRepository.Object, 
                TokenClient.Object,
                SearchAddressClient.Object,
                SalesforceTokenParams.Object,
                JarvisClient.Object,
                TransferWriteRepository.Object, 
                TransactionOperation.Object);

            var request = GetWireTransferAddRequest();

            var result = await controller.WireTransferAdd(request, new CancellationToken());

            Assert.IsType<OkObjectResult>(result);
        }

        [Trait("Integration", "Error")]
        [Fact(DisplayName = "Required information not provided")]
        public void ShouldTranserMoneyWireError()
        {
            var validator = new WireTransferValidator();
            var request = new WireTransferAddRequest();

            var error = validator.Validate(request);

            Assert.False(error.IsValid);
        }

        [Trait("Integration", "Error")]
        [Fact(DisplayName = "WireTransferMoneyCommand_Error", Skip ="true")]
        public async void ShouldNotTranserMoneWire()
        {
            var transferWireResponse = GetTransferWireAddResponse();

            var ProcessearchResponse = GetProcessearchResponse();
            var receivers = new List<Receiver>
            {
                new Receiver
                {
                   AccountNumber = "286914",
                   AccountType = "Outgoing",
                   ReceiverId = 2,
                   RoutingNumber = "011001276"
                }
            };
            TransferInitiationClient
                .Setup(t => t.TransferInitiation(It.IsAny<TransferInitiationParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetTransferInitiationResult);
            ReceiverReadRepository
                .Setup(t => t.Find(It.IsAny<int>())).Returns(receivers);
            InquiryOperation.Setup(x => x.ProcessearchAsync(It.IsAny<ProcessearchRequest>(), It.IsAny<CancellationToken>())).Returns(ProcessearchResponse);

            transferWireResponse.ResponseStatus = "Fail";
            TransferWireOperation.Setup(x => x.TransferWireAddAsync(It.IsAny<TransferWireAddRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(transferWireResponse);

            var transferMoneyValidator = new WireTransferCommand(
                Logger.Object,
                TransferWireOperation.Object,
                ProcessConfig.Object,
                TransferInitiationClient.Object,
                FeedzaiConfig.Object,
                InquiryOperation.Object,
                ReceiverReadRepository.Object,
                TokenClient.Object,
                SearchAddressClient.Object,
                SalesforceTokenParams.Object,
                JarvisClient.Object,
                TransferWriteRepository.Object,
                TransactionOperation.Object);

            var request = GetWireTransferAddRequest();
            TransferWireOperation.Setup(x => x.TransferWireAddAsync(It.IsAny<TransferWireAddRequest>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception());

            Services.AddSingleton(Logger.Object);
            Services.AddSingleton(TransferWireOperation.Object);
            Services.AddSingleton(TransferInitiationClient.Object);
            Services.AddSingleton(FeedzaiConfig.Object);
            Services.AddSingleton(InquiryOperation.Object);
            Services.AddSingleton(ReceiverReadRepository.Object);
            Services.AddSingleton(TokenClient.Object);
            Services.AddSingleton(SearchAddressClient.Object);
            Services.AddSingleton(SalesforceTokenParams.Object);
            Services.AddSingleton(ProcessConfig.Object);
            Services.AddSingleton(JarvisClient.Object);

            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<TransferController>>();
            var mediator = GetInstance<IMediator>();
            var controller = new TransferController(mediator, logger.Object);

            await Assert.ThrowsAsync<UnprocessableEntityException>(() => controller.WireTransferAdd(request, new CancellationToken()));
        }
        #endregion

        #region Methods

        public static T GetInstance<T>()
        {
            T result = Provider.GetRequiredService<T>();
            ControllerBase controllerBase = result as ControllerBase;
            if (controllerBase != null)
            {
                SetControllerContext(controllerBase);
            }
            Controller controller = result as Controller;
            if (controller != null)
            {
                SetControllerContext(controller);
            }
            return result;
        }

        private static void SetControllerContext(Controller controller)
        {
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = HttpContextAccessor.Object.HttpContext
            };
        }

        private static void SetControllerContext(ControllerBase controller)
        {
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = HttpContextAccessor.Object.HttpContext
            };
        }

        #endregion

        #region Private Methods

        private Task<Proxy.Feedzai.Base.Messages.BaseResult<TransferInitiationResult>> GetTransferInitiationResult()
        {
            return Task.FromResult(new Proxy.Feedzai.Base.Messages.BaseResult<TransferInitiationResult>
            {
                IsSuccess = true,
                Result = new TransferInitiationResult
                {
                    Code = "344",
                    Decision = "approve"

                }
            });
        }

        private Task<ProcessearchResponse> GetProcessearchResponse()
        {
            return Task.FromResult(
                new ProcessearchResponse
                {
                    ProcessearchRecInfo = new List<ProcessearchRecInfo>
                    {
                        new ProcessearchRecInfo
                        {
                            AccountId = new AccountId
                            {
                                AccountNumber = "100000010",
                                AccountType = "D",
                            },
                            CustomerId = "DAA0001",
                            Amount = 1000,
                            PersonNameInfo = new PersonNameInfo.PersonName{
                                ComName = "Teste"
                            }
                        }
                    }
                });
        }

        private Task<StopCheckAddResponse> GetStopCheckAddResponse()
        {
            return Task.FromResult(
                new StopCheckAddResponse
                {
                    ResponseHeaderInfo = new ResponseMessageHeaderInfo
                    {
                        JxChangeHeaderInfo = new ChangeHeaderInfoMessage
                        {
                            AuditUserId = "AuditUserId"
                        },
                        RecordInformationMessage = new System.Collections.Generic.List<RecInfoMessage>
                        {
                            new RecInfoMessage
                            {
                                ErrorCategory = "ErrorCategory"
                            }
                        }
                    },
                    Status = "Success",
                    SequenceNumber = 1
                });
        }

        private Task<StopCheckCancelResponse> GetStopCheckCancelResponse()
        {
            return Task.FromResult(
                new StopCheckCancelResponse
                {
                    ResponseStatus = "Success",
                    ResponseHeaderInfo = new ResponseHeaderInfoMessage
                    {
                        JxChangeHeaderInfo = new ChangeHeaderInfoMessage
                        {
                            AuditUserId = "AuditUserId"
                        },
                        MsgRecInfo = new List<RecInfoMessage>
                        {
                            new RecInfoMessage
                            {
                                ErrorCategory = "ErrorCategory"
                            }
                        }
                    }
                });
        }

        private Task<Proxy.Feedzai.Base.Messages.BaseResult<TransferInitiationResult>> GeTransferInitiationResponse()
        {
            return Task.FromResult(new Proxy.Feedzai.Base.Messages.BaseResult<TransferInitiationResult>
            {
                IsSuccess = true,
                Result = new TransferInitiationResult
                {
                    Decision = "teste"
                }
            });
        }

        private TransferWireAddResponse GetTransferWireAddResponse()
        {
            return ConvertJson.ReadJson<TransferWireAddResponse>("TransferWireAddResponse.json");
        }

        private WireTransferAddRequest GetWireTransferAddRequest()
        {
            return ConvertJson.ReadJson<WireTransferAddRequest>("WireTransferAddRequest.json");
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

        #endregion   
    }
}