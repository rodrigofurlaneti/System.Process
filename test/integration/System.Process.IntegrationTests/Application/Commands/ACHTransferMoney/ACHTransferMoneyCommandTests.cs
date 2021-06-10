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
using System.Process.Application.Commands.AchTransferMoney;
using System.Process.Base.IntegrationTests;
using System.Process.CrossCutting.DependencyInjection;
using System.Process.Domain.Constants;
using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Process.Domain.ValueObjects;
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
using System.Proxy.Silverlake.Inquiry;
using System.Proxy.Silverlake.Inquiry.Common;
using System.Proxy.Silverlake.Inquiry.Messages.Request;
using System.Proxy.Silverlake.Inquiry.Messages.Response;
using System.Proxy.Silverlake.Transaction;
using System.Proxy.Silverlake.Transaction.Messages;
using System.Proxy.Silverlake.Transaction.Messages.Request;
using System.Proxy.Silverlake.Transaction.Messages.Response;
using Xunit;

namespace System.Process.IntegrationTests.Application.Commands.ACHTransferMoney
{
    public class ACHTransferMoneyCommandTests
    {
        #region Properties

        private static ServiceCollection Services;
        private static ServiceProvider Provider;
        private static Mock<IHttpContextAccessor> HttpContextAccessor;

        private Mock<ILogger<AchTransferMoneyCommand>> Logger { get; set; }
        private Mock<ITransactionOperation> TransactionOperation { get; set; }
        private Mock<IOptions<ProcessConfig>> ProcessConfig { get; set; }
        private Mock<ITransferInitiationClient> TransferInitiationClient { get; set; }
        private Mock<IOptions<FeedzaiConfig>> FeedzaiConfig { get; set; }
        private Mock<IInquiryOperation> InquiryOperation { get; set; }
        private Mock<IReceiverReadRepository> ReceiverReadRepository { get; set; }
        private Mock<ISearchAddressClient> GetSearchAddressClient { get; set; }
        private Mock<IGetTokenClient> GetTokenClient { get; }
        private Mock<IOptions<GetTokenParams>> ConfigSalesforce { get; }
        private Mock<IJarvisClient> JarvisClient { get; }
        private Mock<ITransferWriteRepository> TransferWriteRepository { get; set; }
        private Mock<ITransferReadRepository> TransferReadRepository { get; set; }

        #endregion

        #region Constructor

        public ACHTransferMoneyCommandTests()
        {
            Logger = new Mock<ILogger<AchTransferMoneyCommand>>();
            GetTokenClient = new Mock<IGetTokenClient>();
            ConfigSalesforce = new Mock<IOptions<GetTokenParams>>();
            GetSearchAddressClient = new Mock<ISearchAddressClient>();
            TransactionOperation = new Mock<ITransactionOperation>();
            ProcessConfig = new Mock<IOptions<ProcessConfig>>();
            ProcessConfig.Setup(p => p.Value)
                .Returns(ProcessIntegrationTestsConfiguration.ReadJson<ProcessConfig>("ProcessConfig.json"));
            FeedzaiConfig = new Mock<IOptions<FeedzaiConfig>>();
            FeedzaiConfig.Setup(p => p.Value)
                .Returns(new FeedzaiConfig
                {
                    Token = "344j43h4",
                    Url = "httptest"
                });
            TransferInitiationClient = new Mock<ITransferInitiationClient>();
            TransferInitiationClient
                .Setup(x => x.TransferInitiation(It.IsAny<TransferInitiationParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(GeTransferInitiationResponse());
            InquiryOperation = new Mock<IInquiryOperation>();
            InquiryOperation
                .Setup(x => x.ProcessearchAsync(It.IsAny<ProcessearchRequest>(), It.IsAny<CancellationToken>()))
                .Returns(GetProcessearchResponse());
            ReceiverReadRepository = new Mock<IReceiverReadRepository>();
            ReceiverReadRepository
                .Setup(x => x.Find(It.IsAny<int>()))
                .Returns(new List<Receiver> {
                    new Receiver
                    {
                        CustomerId = "DAA0001",
                        AccountNumber = "100000037",
                        AccountType = "D",
                        RoutingNumber = "26008413"
                    }
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

            TransferWriteRepository = new Mock<ITransferWriteRepository>();
            TransferReadRepository = new Mock<ITransferReadRepository>();
            Services = new ServiceCollection();
            Services.AddMediator();
            Services.AddScoped(typeof(ILoggerFactory), typeof(LoggerFactory));
            Services.AddScoped(typeof(ILogger<>), typeof(Logger<>));

            Provider = Services.BuildServiceProvider();
        }

        #endregion

        #region Tests

        [Trait("Integration", "Success")]
        [Fact(DisplayName = "AchTransferMoneyCommand_Success")]
        public async Task ShouldTranserMoneyACHSuccessfully()
        {
            var ProcessearchResponse = GetProcessearchResponse();
            var receivers = new List<Receiver>
            {
                new Receiver
                {
                   AccountNumber = "100000037",
                   AccountType = "D",
                   ReceiverId = 2,
                   RoutingNumber = "26008413",
                   CustomerId = "DAA0001"
                }
            };
            TransferInitiationClient
                .Setup(t => t.TransferInitiation(It.IsAny<TransferInitiationParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetTransferInitiationResult);
            ReceiverReadRepository
                .Setup(t => t.Find(It.IsAny<int>())).Returns(receivers);
            InquiryOperation.Setup(x => x.ProcessearchAsync(It.IsAny<ProcessearchRequest>(), It.IsAny<CancellationToken>())).Returns(ProcessearchResponse);
            TransactionOperation
                .Setup(t => t.TransferAddValidateAsync(It.IsAny<TransferAddValidateRequest>(), It.IsAny<CancellationToken>())).Returns(GetTransferAddValidateResponse());
            TransactionOperation
                .Setup(t => t.TransferAddAsync(It.IsAny<TransferAddRequest>(), It.IsAny<CancellationToken>())).Returns(GetTransferAddResponse());
            GetSearchAddressClient
               .Setup(x => x.SearchAddress(It.IsAny<SearchAddressParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(GetSearchAddressResult()));
            GetTokenClient
                .Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(GetBaseTokenResult()));

            Services.AddSingleton(Logger.Object);
            Services.AddSingleton(TransactionOperation.Object);
            Services.AddSingleton(ProcessConfig.Object);
            Services.AddSingleton(TransferInitiationClient.Object);
            Services.AddSingleton(FeedzaiConfig.Object);
            Services.AddSingleton(InquiryOperation.Object);
            Services.AddSingleton(ReceiverReadRepository.Object);
            Services.AddSingleton(GetSearchAddressClient.Object);
            Services.AddSingleton(ConfigSalesforce.Object);
            Services.AddSingleton(GetTokenClient.Object);
            Services.AddSingleton(JarvisClient.Object);
            Services.AddSingleton(TransferReadRepository.Object);
            Services.AddSingleton(TransferWriteRepository.Object);
            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<TransferController>>();
            var mediator = GetInstance<IMediator>();
            var controller = new TransferController(mediator, logger.Object);
            var request = ProcessIntegrationTestsConfiguration.ReadJson<AchTransferMoneyRequest>("AchTransferMoneyRequest.json");

            var result = await controller.AchTransferMoney(request, new CancellationToken());

            Assert.IsType<OkObjectResult>(result);
        }

        [Trait("Integration", "Not Enough balance")]
        [Fact(DisplayName = "AchTransferMoneyCommand_NotEnoughBalance")]
        public async Task ShouldReturnNotEnoughBalanceErrorAsync()
        {
            var ProcessearchResponse = GetProcessearchResponse();
            var receivers = new List<Receiver>
            {
                new Receiver
                {
                   AccountNumber = "100000037",
                   AccountType = "D",
                   ReceiverId = 2,
                   RoutingNumber = "26008413",
                   CustomerId = "DAA0001"
                }
            };
            TransferInitiationClient
                .Setup(t => t.TransferInitiation(It.IsAny<TransferInitiationParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetTransferInitiationResult);
            ReceiverReadRepository
                .Setup(t => t.Find(It.IsAny<int>())).Returns(receivers);
            InquiryOperation.Setup(x => x.ProcessearchAsync(It.IsAny<ProcessearchRequest>(), It.IsAny<CancellationToken>())).Returns(ProcessearchResponse);
            TransactionOperation
                .Setup(t => t.TransferAddValidateAsync(It.IsAny<TransferAddValidateRequest>(), It.IsAny<CancellationToken>())).Returns(GetTransferAddValidateResponse());
            TransactionOperation
                .Setup(t => t.TransferAddAsync(It.IsAny<TransferAddRequest>(), It.IsAny<CancellationToken>())).Returns(GetTransferAddResponse());
            GetSearchAddressClient
               .Setup(x => x.SearchAddress(It.IsAny<SearchAddressParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(GetSearchAddressResult()));
            GetTokenClient
                .Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(GetBaseTokenResult()));

            Services.AddSingleton(Logger.Object);
            Services.AddSingleton(TransactionOperation.Object);
            Services.AddSingleton(ProcessConfig.Object);
            Services.AddSingleton(TransferInitiationClient.Object);
            Services.AddSingleton(FeedzaiConfig.Object);
            Services.AddSingleton(InquiryOperation.Object);
            Services.AddSingleton(ReceiverReadRepository.Object);
            Services.AddSingleton(GetSearchAddressClient.Object);
            Services.AddSingleton(ConfigSalesforce.Object);
            Services.AddSingleton(GetTokenClient.Object);
            Services.AddSingleton(JarvisClient.Object);
            Services.AddSingleton(TransferReadRepository.Object);
            Services.AddSingleton(TransferWriteRepository.Object);
            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<TransferController>>();
            var mediator = GetInstance<IMediator>();
            var controller = new TransferController(mediator, logger.Object);
            var request = ProcessIntegrationTestsConfiguration.ReadJson<AchTransferMoneyRequest>("AchTransferMoneyRequest _NotEnoughBalance.json");

            var exception = await Record.ExceptionAsync(() => controller.AchTransferMoney(request, new CancellationToken()));
            Assert.IsType<UnprocessableEntityException>(exception);
            Assert.True(((UnprocessableEntityException)exception).ErrorCode == ErrorCodes.AvailableBalanceExceeded);
        }

        [Trait("Integration", "Error")]
        [Fact(DisplayName = "AchTransferMoneyCommand_Error")]
        public async Task ShouldNotTranserMoneyACH()
        {
            Logger = new Mock<ILogger<AchTransferMoneyCommand>>();
            TransactionOperation = new Mock<ITransactionOperation>();

            var request = ProcessIntegrationTestsConfiguration.ReadJson<AchTransferMoneyRequest>("AchTransferMoneyRequest.json");
            var transferAddResponse = GetTransferAddResponse();
            transferAddResponse.Result.ResponseStatus = null;
            var ProcessearchResponse = GetProcessearchResponse();
            var receivers = new List<Receiver>
            {
                new Receiver
                {
                   AccountNumber = "12342",
                   AccountType = "D",
                   ReceiverId = 2,
                   RoutingNumber = "674574"
                }
            };
            TransferInitiationClient
                .Setup(t => t.TransferInitiation(It.IsAny<TransferInitiationParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetTransferInitiationResult);
            ReceiverReadRepository
                .Setup(t => t.Find(It.IsAny<int>())).Returns(receivers);
            InquiryOperation.Setup(x => x.ProcessearchAsync(It.IsAny<ProcessearchRequest>(), It.IsAny<CancellationToken>())).Returns(ProcessearchResponse);
            TransactionOperation
                .Setup(t => t.TransferAddAsync(It.IsAny<TransferAddRequest>(), It.IsAny<CancellationToken>())).Returns(transferAddResponse);
            TransactionOperation
                .Setup(t => t.TransferAddValidateAsync(It.IsAny<TransferAddValidateRequest>(), It.IsAny<CancellationToken>())).Returns(GetTransferAddValidateResponse());
            GetSearchAddressClient
               .Setup(x => x.SearchAddress(It.IsAny<SearchAddressParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(GetSearchAddressResult()));
            GetTokenClient
                .Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(GetBaseTokenResult()));

            Services.AddSingleton(Logger.Object);
            Services.AddSingleton(TransactionOperation.Object);
            Services.AddSingleton(ProcessConfig.Object);
            Services.AddSingleton(TransferInitiationClient.Object);
            Services.AddSingleton(FeedzaiConfig.Object);
            Services.AddSingleton(InquiryOperation.Object);
            Services.AddSingleton(ReceiverReadRepository.Object);
            Services.AddSingleton(GetSearchAddressClient.Object);
            Services.AddSingleton(ConfigSalesforce.Object);
            Services.AddSingleton(GetTokenClient.Object);
            Services.AddSingleton(JarvisClient.Object);
            Services.AddSingleton(TransferReadRepository.Object);
            Services.AddSingleton(TransferWriteRepository.Object);
            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<TransferController>>();
            var mediator = GetInstance<IMediator>();
            var controller = new TransferController(mediator, logger.Object);

            await Assert.ThrowsAsync<UnprocessableEntityException>(() => controller.AchTransferMoney(request, new CancellationToken()));
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

        private Task<TransferAddValidateResponse> GetTransferAddValidateResponse()
        {
            return Task.FromResult(new TransferAddValidateResponse
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
                ResponseStatus = "Success"
            });
        }

        private Task<TransferAddResponse> GetTransferAddResponse()
        {
            return Task.FromResult(
                new TransferAddResponse
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
                    ResponseStatus = "Success",
                    TransferKey = "100000090"
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
                            Amount = 1000
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
