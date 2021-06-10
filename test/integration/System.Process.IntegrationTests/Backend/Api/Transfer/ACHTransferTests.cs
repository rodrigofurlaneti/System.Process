using FluentAssertions;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Process.Api.Controllers.V1;
using System.Process.Application.Commands.AchTransferMoney;
using System.Process.Application.Commands.ACHTransferMoney.Request;
using System.Process.Base.IntegrationTests;
using System.Process.CrossCutting.DependencyInjection;
using System.Phoenix.Common.Exceptions;
using System.Phoenix.Web.Filters;
using System.Proxy.Silverlake.Transaction;
using System.Proxy.Silverlake.Transaction.Messages;
using System.Proxy.Silverlake.Transaction.Messages.Request;
using System.Proxy.Silverlake.Transaction.Messages.Response;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace System.Process.IntegrationTests.Backend.Api.Transfer
{
    public class AchTransferTests
    {
        #region Properties

        private static ServiceProvider Provider;
        private static Mock<IHttpContextAccessor> HttpContextAccessor;
        private static ServiceCollection Services;

        #endregion

        #region Constructor
        static AchTransferTests()
        {
            var appSettings = new Dictionary<string, string>
            {
                {"MongoDB:Snapshot:Database", "test"},
                {"MongoDB:Snapshot:Collection", "test"},
                {"MongoDB:Snapshot:ConnectionString", "test"}
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(appSettings)
                .Build();
            Services = new ServiceCollection();
            Services.AddMvc(options => options.Filters.Add(new ValidationFilterAttribute()))
               .AddFluentValidation(options =>
               {
                   options.RegisterValidatorsFromAssemblyContaining<AchTransferMoneyValidator>();
               });
            Services.AddMediator();
            Services.AddScoped(typeof(ILoggerFactory), typeof(LoggerFactory));
            Services.AddScoped(typeof(ILogger<>), typeof(Logger<>));

            Provider = Services.BuildServiceProvider();
        }

        #endregion

        #region Main Flow

        [Fact(DisplayName = "Should Return Transaction Confirmation Number", Skip = "true")]
        public async Task ShouldACHTransfer()
        {
            var operation = new Mock<ITransactionOperation>();
            var response = await Task.FromResult(new TransferAddResponse
            {
                ResponseStatus = "Success",
                TransferKey = "12344"
            });

            var responseValidate = await Task.FromResult(new TransferAddValidateResponse
            {
                ResponseStatus = "Success"
            });

            operation.Setup(x => x.TransferAddValidateAsync(It.IsAny<TransferAddValidateRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(responseValidate);
            operation.Setup(x => x.TransferAddAsync(It.IsAny<TransferAddRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(response);
            
            Services.AddSingleton(operation.Object);
            Provider = Services.BuildServiceProvider();

            var mediator = GetInstance<IMediator>();
            var logger = new Mock<ILogger<TransferController>>();
            var transferController = new TransferController(mediator, logger.Object);
            var cancellationToken = new CancellationToken();
            var request = ProcessIntegrationTestsConfiguration.ReadJson<AchTransferMoneyRequest>("AchTransferMoneyRequest.json");

            var transferMoneyValidator = new AchTransferMoneyValidator();
            var responseValidator = transferMoneyValidator.Validate(request);
            responseValidator.IsValid.Should().BeTrue();

            var result = await transferController.AchTransferMoney(request, cancellationToken);

            var objectResult = Assert.IsType<OkObjectResult>(result);
            var achTransferMoneyResponse = Assert.IsAssignableFrom<AchTransferMoneyResponse>(objectResult.Value);
            Assert.NotNull(achTransferMoneyResponse.TransactionId);
        }

        #endregion

        #region Alternative Flows

        [Fact(DisplayName = "AF1 – Required FromAccountNumber Information Not Provided or Invalid - Should Returns Errors")]
        public void ShouldIndicatesFromAccountNumberIsNotProperlyFulfilled()
        {
            var request = new AchTransferMoneyRequest
            {
                SystemId = "teste",
                AccountFrom = new AchAccountFrom
                {
                    AccountId = "AccountId",
                    AccountType = "AccountType",
                    Name = "Name",
                    RoutingNumber = "0"
                },
                AccountTo = new AchAccountTo
                {
                    AccountId = "AccountId",
                    AccountType = "AccountType",
                    Name = "Name",
                    RoutingNumber = "0",
                    Email = "Email",
                    Phone = "Phone"
                },
                ReducedPrincipal = "Y",
                Amount =  0,
            };

            var transferMoneyValidator = new AchTransferMoneyValidator();
            var responseValidator = transferMoneyValidator.Validate(request);
            responseValidator.IsValid.Should().BeFalse();
        }

        [Fact(DisplayName = "AF1 – Required ReducedPrincipal Information Not Provided or Invalid - Should Returns Errors", Skip = "true")]
        public void ShouldIndicatesReducedPrincipalIsNotProperlyFulfilled()
        {
            var request = new AchTransferMoneyRequest
            {
                ReducedPrincipal = "A"
            };

            var transferMoneyValidator = new AchTransferMoneyValidator();
            var responseValidator = transferMoneyValidator.Validate(request);

            responseValidator.IsValid.Should().BeFalse();
        }

        [Fact(DisplayName = "AF2 – Error in Money ACHTransfer Validation - Should Returns Errors", Skip = "true")]
        public async Task ShouldReturnMoneyACHTransferValidationError()
        {
            var operation = new Mock<ITransactionOperation>();

            var responseValidate = await Task.FromResult(new TransferAddValidateResponse
            {
                ResponseStatus = null
            });
            var response = await Task.FromResult(new TransferAddResponse
            {
                ResponseStatus = "Success",
                TransferKey = "12344"
            });

            operation.Setup(x => x.TransferAddValidateAsync(It.IsAny<TransferAddValidateRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(responseValidate);
            operation.Setup(x => x.TransferAddAsync(It.IsAny<TransferAddRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(response);
            
            Services.AddSingleton(operation.Object);
            Provider = Services.BuildServiceProvider();

            var mediator = GetInstance<IMediator>();
            var logger = new Mock<ILogger<TransferController>>();
            var transferController = new TransferController(mediator, logger.Object);
            var cancellationToken = new CancellationToken();
            var request = ProcessIntegrationTestsConfiguration.ReadJson<AchTransferMoneyRequest>("AchTransferMoneyRequest.json");

            var transferMoneyValidator = new AchTransferMoneyValidator();
            var responseValidator = transferMoneyValidator.Validate(request);
            responseValidator.IsValid.Should().BeTrue();

            var result = await Assert.ThrowsAsync<UnprocessableEntityException>(() => transferController.AchTransferMoney(request, cancellationToken));

            result.Should().NotBeNull();
            result.Message.Equals("Error during TransferAddValidate execution");
        }

        [Fact(DisplayName = "AF3 – Error in Money ACHTransfer Addition - Should Returns Errors", Skip = "true")]
        public async Task ShouldReturnMoneyTransferAdditionError()
        {
            var operation = new Mock<ITransactionOperation>();

            var responseValidate = await Task.FromResult(new TransferAddValidateResponse
            {
                ResponseStatus = "Success"
            });
            var response = await Task.FromResult(new TransferAddResponse
            {
                ResponseStatus = null,
                TransferKey = "12344"
            });

            operation.Setup(x => x.TransferAddValidateAsync(It.IsAny<TransferAddValidateRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(responseValidate);
            operation.Setup(x => x.TransferAddAsync(It.IsAny<TransferAddRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(response);

            Services.AddSingleton(operation.Object);
            Provider = Services.BuildServiceProvider();

            var mediator = GetInstance<IMediator>();
            var logger = new Mock<ILogger<TransferController>>();
            var transferController = new TransferController(mediator, logger.Object);
            var cancellationToken = new CancellationToken();
            var request = ProcessIntegrationTestsConfiguration.ReadJson<AchTransferMoneyRequest>("AchTransferMoneyRequest.json");


            var transferMoneyValidator = new AchTransferMoneyValidator();
            var responseValidator = transferMoneyValidator.Validate(request);
            responseValidator.IsValid.Should().BeTrue();

            var result = await Assert.ThrowsAsync<UnprocessableEntityException>(() => transferController.AchTransferMoney(request, cancellationToken));

            result.Should().NotBeNull();
            result.Message.Equals("Error during TransferAdd execution");
        }

        #endregion

        #region Exception Flows

        [Fact(DisplayName = "EF1 – Failed connection with Core Banking - Should Throws Exception", Skip = "true")]
        public async Task ShouldThrowsNotFoundException()
        {
            var operation = new Mock<ITransactionOperation>();
            operation.Setup(x => x.TransferAddValidateAsync(It.IsAny<TransferAddValidateRequest>(), It.IsAny<CancellationToken>())).Throws(new Exception());
            operation.Setup(x => x.TransferAddAsync(It.IsAny<TransferAddRequest>(), It.IsAny<CancellationToken>())).Throws(new Exception());
            Services.AddSingleton(operation.Object);
            Provider = Services.BuildServiceProvider();

            var mediator = GetInstance<IMediator>();
            var logger = new Mock<ILogger<TransferController>>();
            var transferController = new TransferController(mediator, logger.Object);
            var cancellationToken = new CancellationToken();
            var request = ProcessIntegrationTestsConfiguration.ReadJson<AchTransferMoneyRequest>("AchTransferMoneyRequest.json");

            var transferMoneyValidator = new AchTransferMoneyValidator();
            var responseValidator = transferMoneyValidator.Validate(request);
            responseValidator.IsValid.Should().BeTrue();

            await Assert.ThrowsAsync<Exception>(() => transferController.AchTransferMoney(request, cancellationToken));
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
    }
}
