using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Process.Application.Clients.Jarvis;
using System.Process.Application.Commands.AchTransferMoney;
using System.Process.Base.IntegrationTests;
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

namespace System.Process.UnitTests.Application.Commands.AchTransferMoney
{
    public class AchTransferMoneyCommandTests
    {
        #region Properties

        private Mock<ILogger<AchTransferMoneyCommand>> Logger { get; set; }
        private Mock<ITransactionOperation> TransactionOperation { get; set; }
        private Mock<IOptions<ProcessConfig>> ProcessConfig { get; set; }
        private Mock<ITransferInitiationClient> TransferInitiationClient { get; set; }
        private Mock<IOptions<FeedzaiConfig>> FeedzaiConfig { get; set; }
        private Mock<IInquiryOperation> InquiryOperation { get; set; }
        private Mock<IReceiverReadRepository> ReceiverReadRepository { get; set; }
        private Mock<IGetTokenClient> TokenClient { get; set; }
        private Mock<ISearchAddressClient> SearchAddressClient { get; set; }
        private Mock<IOptions<GetTokenParams>> SalesforceTokenParams { get; set; }
        private Mock<IJarvisClient> JarvisClient { get; }
        private Mock<ITransferWriteRepository> TransferWriteRepository { get; set; }

        #endregion

        #region Constructor

        public AchTransferMoneyCommandTests()
        {
            Logger = new Mock<ILogger<AchTransferMoneyCommand>>();
            TransactionOperation = new Mock<ITransactionOperation>();
            ProcessConfig = new Mock<IOptions<ProcessConfig>>();
            ProcessConfig
                .Setup(p => p.Value)
                .Returns(ProcessIntegrationTestsConfiguration.ReadJson<ProcessConfig>("ProcessConfig.json"));
            TransferInitiationClient = new Mock<ITransferInitiationClient>();
            TransferInitiationClient
                .Setup(x => x.TransferInitiation(It.IsAny<TransferInitiationParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(GeTransferInitiationResponse());
            FeedzaiConfig = new Mock<IOptions<FeedzaiConfig>>();
            FeedzaiConfig
                .Setup(x => x.Value)
                .Returns(new FeedzaiConfig { Token = "token", Url = "url" });
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
            SalesforceTokenParams = new Mock<IOptions<GetTokenParams>>();
            SalesforceTokenParams.Setup(x => x.Value).Returns(new GetTokenParams { ClientId = "ClientId", ClientSecret = "ClientSecret", GrantType = "GrantType", Password = "Password", Username = "Username" });
            SearchAddressClient = new Mock<ISearchAddressClient>();
            SearchAddressClient.Setup(x => x.SearchAddress(It.IsAny<SearchAddressParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(GetSearchAddressResult()));
            TokenClient = new Mock<IGetTokenClient>();
            TokenClient
                .Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(GetBaseTokenResult()));
            TransferWriteRepository = new Mock<ITransferWriteRepository>();
            
        }

        #endregion

        #region Tests

        [Fact(DisplayName = "Should Send Handle TransferMoney")]
        public async Task ShouldSendHandleACHTransferMoneyAsync()
        {
            TransactionOperation
               .Setup(t => t.TransferAddValidateAsync(It.IsAny<TransferAddValidateRequest>(), It.IsAny<CancellationToken>())).Returns(GetTransferAddValidateResponse());

            TransactionOperation
                .Setup(t => t.TransferAddAsync(It.IsAny<TransferAddRequest>(), It.IsAny<CancellationToken>())).Returns(GetTransferAddResponse());

            var transferMoneyCommand = new AchTransferMoneyCommand(
                Logger.Object,
                TransactionOperation.Object,
                ProcessConfig.Object,
                TransferInitiationClient.Object,
                FeedzaiConfig.Object,
                InquiryOperation.Object,
                ReceiverReadRepository.Object,
                JarvisClient.Object,
                TokenClient.Object,
                SearchAddressClient.Object,
                SalesforceTokenParams.Object,
                TransferWriteRepository.Object);

            var request = ProcessIntegrationTestsConfiguration.ReadJson<AchTransferMoneyRequest>("AchTransferMoneyRequest.json");

            var result = await transferMoneyCommand.Handle(request, new CancellationToken());

            result.TransactionId.Should().NotBeNullOrEmpty();
        }

        [Fact(DisplayName = "Should Send Handle TransferMoneyError")]
        public async Task ShouldSendHandleACHTransferAddValidateErrorAsync()
        {
            var transferMoneyCommand = new AchTransferMoneyCommand(
                Logger.Object, 
                TransactionOperation.Object,
                ProcessConfig.Object, 
                TransferInitiationClient.Object, 
                FeedzaiConfig.Object, 
                InquiryOperation.Object, 
                ReceiverReadRepository.Object, 
                JarvisClient.Object, 
                TokenClient.Object, 
                SearchAddressClient.Object, 
                SalesforceTokenParams.Object,
                TransferWriteRepository.Object);

            var request = ProcessIntegrationTestsConfiguration.ReadJson<AchTransferMoneyRequest>("AchTransferMoneyRequest.json");

            var responseHeaderInfo = GetTransferAddValidateResponse();

            responseHeaderInfo.Result.ResponseStatus = null;

            TransactionOperation
                .Setup(t => t.TransferAddValidateAsync(It.IsAny<TransferAddValidateRequest>(), It.IsAny<CancellationToken>())).Returns(responseHeaderInfo);

            var transferAddResponse = GetTransferAddResponse();

            transferAddResponse.Result.ResponseStatus = null;

            TransactionOperation
                .Setup(t => t.TransferAddAsync(It.IsAny<TransferAddRequest>(), It.IsAny<CancellationToken>())).Returns(transferAddResponse);

            var stopCheckAddResponse = GetStopCheckAddResponse();
            stopCheckAddResponse.Result.Status = null;
            TransactionOperation
                .Setup(t => t.StopCheckAddAsync(It.IsAny<StopCheckAddRequest>(), It.IsAny<CancellationToken>())).Returns(stopCheckAddResponse);

            var stopCheckCancelResponse = GetStopCheckCancelResponse();
            stopCheckAddResponse.Result.Status = null;
            TransactionOperation
                .Setup(t => t.StopCheckCancelAsync(It.IsAny<StopCheckCancelRequest>(), It.IsAny<CancellationToken>())).Returns(stopCheckCancelResponse);

            await Assert.ThrowsAsync<UnprocessableEntityException>(() => transferMoneyCommand.Handle(request, new CancellationToken()));
        }

        [Fact(DisplayName = "Should Send Handle TransferMoneyError")]
        public async Task ShouldSendHandleACHTransferAddErrorAsync()
        {
            var request = ProcessIntegrationTestsConfiguration.ReadJson<AchTransferMoneyRequest>("AchTransferMoneyRequest.json");

            TransactionOperation
                .Setup(t => t.TransferAddValidateAsync(It.IsAny<TransferAddValidateRequest>(), It.IsAny<CancellationToken>())).Returns(GetTransferAddValidateResponse());

            var transferAddResponse = GetTransferAddResponse();

            transferAddResponse.Result.ResponseStatus = null;

            TransactionOperation
                .Setup(t => t.TransferAddAsync(It.IsAny<TransferAddRequest>(), It.IsAny<CancellationToken>())).Returns(transferAddResponse);

            var transferMoneyCommand = new AchTransferMoneyCommand(
                Logger.Object,
                TransactionOperation.Object,
                ProcessConfig.Object,
                TransferInitiationClient.Object,
                FeedzaiConfig.Object,
                InquiryOperation.Object,
                ReceiverReadRepository.Object,
                JarvisClient.Object,
                TokenClient.Object,
                SearchAddressClient.Object,
                SalesforceTokenParams.Object,
                TransferWriteRepository.Object);

            await Assert.ThrowsAsync<UnprocessableEntityException>(() => transferMoneyCommand.Handle(request, new CancellationToken()));
        }

        #endregion

        #region Methods

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
