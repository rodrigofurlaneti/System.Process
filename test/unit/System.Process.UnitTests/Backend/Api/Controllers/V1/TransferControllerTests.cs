using System;
using System.Collections.Generic;
using System.Threading;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Process.Api.Controllers.V1;
using System.Process.Application.Clients.Jarvis;
using System.Process.Application.Commands.TransferMoney;
using System.Process.Base.UnitTests;
using System.Process.CrossCutting.DependencyInjection;
using System.Process.Domain.Repositories;
using System.Process.Domain.ValueObjects;
using System.Proxy.Feedzai.Base.Config;
using System.Proxy.Feedzai.TransferInitiation;
using System.Proxy.Feedzai.TransferInitiation.Messages;
using System.Proxy.Salesforce.GetToken;
using System.Proxy.Salesforce.GetToken.Messages;
using System.Proxy.Salesforce.Messages;
using System.Proxy.Salesforce.SearchAddress;
using System.Proxy.Salesforce.SearchAddress.Messages;
using System.Proxy.Silverlake.Inquiry;
using System.Proxy.Silverlake.Inquiry.Messages.Request;
using System.Proxy.Silverlake.Transaction;
using System.Proxy.Silverlake.Transaction.Messages;
using System.Proxy.Silverlake.Transaction.Messages.Request;
using Xunit;

namespace System.Process.UnitTests.Backend.Api.Controllers.V1
{
    public class TransferControllerTests
    {
        #region Properties

        private const bool SUCCESS = true;
        private const bool FAIL = false;

        private readonly FakeData _fakeData;

        private Mock<ILogger<TransferController>> LoggerTransferController { get; set; }
        private Mock<ILogger<TransferMoneyCommand>> LoggerTransferMoneyCommand { get; set; }
        private ServiceProvider Provider { get; set; }
        private Mock<IHttpContextAccessor> HttpContextAccessor { get; set; }
        private ServiceCollection Service { get; set; }
        private Mock<ITransactionOperation> TransactionOperation { get; set; }
        private IOptions<ProcessConfig> ProcessConfig { get; set; }
        private Mock<ITransferInitiationClient> TransferInitiationClient { get; set; }
        private IOptions<FeedzaiConfig> FeedzaiConfig { get; set; }
        private Mock<IInquiryOperation> InquiryOperation { get; set; }
        private Mock<IReceiverReadRepository> ReceiverReadRepository { get; set; }
        private Mock<IJarvisClient> JarvisClient { get; set; }
        private Mock<IGetTokenClient> TokenClient { get; set; }
        private Mock<ISearchAddressClient> SearchAddressClient { get; set; }
        private IOptions<GetTokenParams> SalesforceTokenParams { get; set; }
        private Mock<ITransferWriteRepository> TransferWriteRepository { get; set; }

        #endregion Properties

        #region Constructor

        public TransferControllerTests()
        {
            Service = new ServiceCollection();
            _fakeData = new FakeData();
            LoggerTransferController = new Mock<ILogger<TransferController>>();
            LoggerTransferMoneyCommand = new Mock<ILogger<TransferMoneyCommand>>();
            TransactionOperation = new Mock<ITransactionOperation>();
            TransferInitiationClient = new Mock<ITransferInitiationClient>();
            InquiryOperation = new Mock<IInquiryOperation>();
            ReceiverReadRepository = new Mock<IReceiverReadRepository>();
            JarvisClient = new Mock<IJarvisClient>();
            TokenClient = new Mock<IGetTokenClient>();
            SearchAddressClient = new Mock<ISearchAddressClient>();
            TransferWriteRepository = new Mock<ITransferWriteRepository>();
        }

        #endregion Constructor

        #region Tests

        [Fact(DisplayName = "Should Return Sucessfully Transaction Operation")]
        [Trait("Success", "TransferControllerTests")]
        public async void ShouldReturnSuccessfullyTransactionOperation()
        {
            LoadAllSetups(SUCCESS);

            LoadServiceCollection();

            var request = _fakeData.GetTransferMoneyRequest();

            var controller = LoadTransferController();

            var result = await controller.TransferMoney(request, new CancellationToken());

            result.As<ObjectResult>().Value.As<TransferMoneyResponse>().TransactionId.Should().Contain("123456789");
        }

        [Fact(DisplayName = "Should Return Error When ResponseStatus Is Review")]
        [Trait("Fail", "TransferControllerTests")]
        public async void ShouldReturnErrorWhenResponseStatusIsReview()
        {
            LoadAllSetups(FAIL, Errors.TransferAddValidateAsyncResponseStatusFail);

            LoadServiceCollection();

            var request = _fakeData.GetTransferMoneyRequest();

            var controller = LoadTransferController();

            var result = await Record.ExceptionAsync(async () => await controller.TransferMoney(request, new CancellationToken()));

            Assert.Equal("Error during TransferAddValidate execution", result.Message);    
        }

        [Fact(DisplayName = "Should Return Error When SearchAddressResult Is Null")]
        [Trait("Fail", "TransferControllerTests")]
        public async void ShouldReturnErrorWhenSearchAddressResultIsNull()
        {
            LoadAllSetups(FAIL, Errors.NullAddressResult);

            LoadServiceCollection();

            var request = _fakeData.GetTransferMoneyRequest();

            var controller = LoadTransferController();

            var result = await Record.ExceptionAsync(async () => await controller.TransferMoney(request, new CancellationToken()));

            Assert.Equal("SearchAddress did not return address", result.Message);
        }

        [Fact(DisplayName = "Should Return Error When Transaction Not Approved")]
        [Trait("Fail", "TransferControllerTests")]
        public async void ShouldReturnErrorWhenTransactionNotApproved()
        {
            LoadAllSetups(FAIL, Errors.TransferInitiationResultDecisionDeclined);

            LoadServiceCollection();

            var request = _fakeData.GetTransferMoneyRequest();

            var controller = LoadTransferController();

            var result = await Record.ExceptionAsync(async () => await controller.TransferMoney(request, new CancellationToken()));

            Assert.Equal("Transaction not approved", result.Message);
        }

        [Fact(DisplayName = "Should Return Error During TransferAdd Execution")]
        [Trait("Fail", "TransferControllerTests")]
        public async void ShouldReturnErrorDuringTransferAddExecution()
        {
            LoadAllSetups(FAIL, Errors.TransferAddAsyncResponseStatusFail);

            LoadServiceCollection();

            var request = _fakeData.GetTransferMoneyRequest();

            var controller = LoadTransferController();

            var result = await Record.ExceptionAsync(async () => await controller.TransferMoney(request, new CancellationToken()));

            Assert.Equal("Error during TransferAdd execution", result.Message);
        }

        #endregion Tests

        #region Methods

        private TransferController LoadTransferController()
        {
            Provider = Service.BuildServiceProvider();

            var mediator = GetInstance<IMediator>();

            return new TransferController(mediator, LoggerTransferController.Object);
        }

        private void LoadServiceCollection()
        {
            Service.AddMediator();
            Service.AddScoped(typeof(ILoggerFactory), typeof(LoggerFactory));
            Service.AddScoped(typeof(ILogger<>), typeof(Logger<>));
            Service.AddSingleton(TransactionOperation.Object);
            Service.AddSingleton(ProcessConfig);
            Service.AddSingleton(TransferInitiationClient.Object);
            Service.AddSingleton(FeedzaiConfig);
            Service.AddSingleton(InquiryOperation.Object);
            Service.AddSingleton(ReceiverReadRepository.Object);
            Service.AddSingleton(JarvisClient.Object);
            Service.AddSingleton(TokenClient.Object);
            Service.AddSingleton(SearchAddressClient.Object);
            Service.AddSingleton(SalesforceTokenParams);
            Service.AddSingleton(TransferWriteRepository.Object);

        }

        private void LoadAllSetups(bool success, Errors errors = 0)
        {
            SetupTransactionOperation(errors);
            SetupProcessConfig();
            SetupTransferInitiationClient(errors);
            SetupFeedzaiConfig();
            SetupInquiryOperation();
            SetupReceiverReadRepository();
            SetupJarvisClient();
            SetupTokenClient();
            SetupSearchAddressClient(success, errors);
            SetupSalesforceTokenParams();
            SetupTransferWriteRepository();
        }

        private void SetupTransferWriteRepository()
        {
            TransferWriteRepository.SetupAllProperties();
        }

        private void SetupSalesforceTokenParams()
        {
            SalesforceTokenParams = Options.Create(new GetTokenParams
            {
                ClientId = "12345",
                ClientSecret = "Secret",
                GrantType = "GrantType",
                Password = "Password",
                Username = "Name"
            });
        }

        private void SetupSearchAddressClient(bool success, Errors errors)
        {
            Func<BaseResult<QueryResult<SearchAddressResponse>>> address = () =>
            {
                if (!success)
                {
                    if (errors == Errors.NullAddressResult)
                        return new BaseResult<QueryResult<SearchAddressResponse>>();
                }
                
                return new BaseResult<QueryResult<SearchAddressResponse>>
                {
                    ErrorMessage = "ErrorMessage",
                    IsSuccess = true,
                    ItemReferenceId = "123",
                    Message = "Message",
                    Result = _fakeData.GetQueryResultSearchAddressResponse()
                };
            };

            SearchAddressClient.Setup(s => s.SearchAddress(It.IsAny<SearchAddressParams>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(address);         
        }

        private void SetupTokenClient()
        {
            TokenClient.Setup(s => s.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BaseResult<GetTokenResult>
                {
                    ErrorMessage = "ErrorMessage",
                    IsSuccess = true,
                    ItemReferenceId = "123",
                    Message = "Message",
                    Result = new GetTokenResult
                    {
                        AccessToken = "12345",
                        Id = "123",
                        InstanceUrl = "url",
                        IssuedAt = "IssuedAt",
                        Signature = "Signature",
                        TokenType = "TokenType"
                    }
                }
            );           
        }

        private void SetupJarvisClient()
        {
            JarvisClient.Setup(j => j.GetDeviceDetails(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_fakeData.getDeviceDetails());
        }

        private void SetupReceiverReadRepository()
        {
            ReceiverReadRepository.Setup(r => r.Find(It.IsAny<int>()))
                .Returns(_fakeData.GetReceivers());
        }

        private void SetupInquiryOperation()
        {
            InquiryOperation.Setup(i => i.ProcessearchAsync(It.IsAny<ProcessearchRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_fakeData.GetProcessearchResponse());
        }

        private void SetupFeedzaiConfig()
        {
            FeedzaiConfig = Options.Create(_fakeData.GetFeedzaiConfig());
        }
        
        private void SetupTransferInitiationClient(Errors errors)
        {
            Func<Proxy.Feedzai.Base.Messages.BaseResult<TransferInitiationResult>> fakeData = () =>
            {
                var data = _fakeData.GetBaseResultTransferInitiationResult();
                if (errors == Errors.TransferInitiationResultDecisionDeclined)
                {
                    data.Result.Decision = "decline";
                }
                return data;
            };

            TransferInitiationClient.Setup(t => t.TransferInitiation(It.IsAny<TransferInitiationParams>(),
                It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(fakeData);
        }

        private void SetupTransactionOperation(Errors errors)
        {
            var transferAddValidateResponse = _fakeData.GetTransferAddValidateResponse();
            var transferAddResponse = _fakeData.GetTransferAddResponse();

            switch(errors)
            {
                case Errors.TransferAddValidateAsyncResponseStatusFail:
                    {
                        transferAddValidateResponse.ResponseStatus = "Review";
                        break;
                    }
                case Errors.TransferAddAsyncResponseStatusFail:
                    {
                        transferAddResponse.ResponseStatus = "decline";
                        break;
                    }
            }

            TransactionOperation.Setup(t => t.TransferAddValidateAsync(It.IsAny<TransferAddValidateRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transferAddValidateResponse);          

            TransactionOperation.Setup(t => t.TransferAddAsync(It.IsAny<TransferAddRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(transferAddResponse);
        }

        private void SetupProcessConfig()
        {
            ProcessConfig = Options.Create(_fakeData.GetProcessConfig());           
        }

        public T GetInstance<T>()
        {
            T result = Provider.GetRequiredService<T>();
            if (result is ControllerBase controllerBase)
            {
                SetControllerContext(controllerBase);
            }
            if (result is Controller controller)
            {
                SetControllerContext(controller);
            }
            return result;
        }

        private void SetControllerContext(Controller controller)
        {
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = HttpContextAccessor.Object.HttpContext
            };
        }

        private void SetControllerContext(ControllerBase controller)
        {
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = HttpContextAccessor.Object.HttpContext
            };
        }

        #endregion Methods
    }
}
