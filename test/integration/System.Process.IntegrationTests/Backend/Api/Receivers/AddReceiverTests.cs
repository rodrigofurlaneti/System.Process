using FluentAssertions;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Process.Api.Controllers.V1;
using System.Process.Application.Commands.AddReceiver;
using System.Process.CrossCutting.DependencyInjection;
using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Phoenix.Common.Exceptions;
using System.Phoenix.Web.Filters;
using System.Collections.Generic;
using System.Threading;
using Xunit;

namespace System.Process.IntegrationTests.Backend.Api.Receivers
{
    public class AddReceiverTests
    {
        #region Properties

        private static ServiceProvider Provider;
        private static Mock<IHttpContextAccessor> HttpContextAccessor;
        private static ServiceCollection Services;

        #endregion

        #region Constructor
        static AddReceiverTests()
        {
            Services = new ServiceCollection();
            Services.AddMvc(options => options.Filters.Add(new ValidationFilterAttribute()))
               .AddFluentValidation(options =>
               {
                   options.RegisterValidatorsFromAssemblyContaining<AddReceiverValidator>();
               });
            Services.AddMediator();
            Services.AddScoped(typeof(Microsoft.Extensions.Logging.ILoggerFactory), typeof(LoggerFactory));
            Services.AddScoped(typeof(ILogger<>), typeof(Logger<>));

            Provider = Services.BuildServiceProvider();
        }

        #endregion

        #region Main Flow

        [Fact(DisplayName = "Main Flow - Success")]
        public async void ShouldValidateProcessuccessfully()
        {
            var receiverReadRepository = new Mock<IReceiverReadRepository>();
            var receiverWriteRepository = new Mock<IReceiverWriteRepository>();

            var receivers = new List<Receiver>();
            receiverReadRepository
                .Setup(r => r.FindExistent(It.IsAny<Receiver>())).Returns(receivers);

            receiverWriteRepository
                .Setup(r => r.Add(It.IsAny<Receiver>(), It.IsAny<CancellationToken>())).Verifiable();

            Services.AddSingleton(receiverReadRepository.Object);
            Services.AddSingleton(receiverWriteRepository.Object);

            Provider = Services.BuildServiceProvider();
            var mediator = GetInstance<IMediator>();
            var logger = new Mock<ILogger<ReceiversController>>();

            var receiversController = new ReceiversController(mediator, logger.Object);
            var request = new AddReceiverRequest
            {
                AccountNumber = "16516",
                AccountType = "D",
                BankType = "S",
                CompanyName = "",
                CustomerId = "123",
                EmailAdress = "email@mail.com",
                FirstName = "Raul",
                LastName = "Segal",
                NickName = "NickName",
                PhoneNumber = "+5512727222",
                ReceiverType = "D",
                RoutingNumber = "1"
            };
            var cancellationToken = new CancellationToken();

            var result = await receiversController.Add(request, cancellationToken);

            var objectResult = Assert.IsType<CreatedResult>(result);
            var addReceiverResponse = Assert.IsAssignableFrom<AddReceiverResponse>(objectResult.Value);
            Assert.NotNull(addReceiverResponse.Message);
        }

        #endregion

        #region Alternative Flows

        [Fact(DisplayName = "AF1 – Required AccountNumber Information Not Provided or Invalid - Should Returns Errors")]
        public void ShouldIndicatesAccountNumberIsNotProperlyFulfilled()
        {
            var request = new AddReceiverRequest
            {
                AccountNumber = null,
                AccountType = "D",
                BankType = "S",
                CompanyName = "",
                CustomerId = "123",
                EmailAdress = "email@mail.com",
                FirstName = "Raul",
                LastName = "Segal",
                NickName = "NickName",
                PhoneNumber = "+5512727222",
                ReceiverType = "D",
                RoutingNumber = "1",
                Ownership = "S"
            };

            var addReceiverValidator = new AddReceiverValidator();
            var responseValidator = addReceiverValidator.Validate(request);
            responseValidator.IsValid.Should().BeFalse();
        }

        [Fact(DisplayName = "AF1 – Required AccountType Information Not Provided or Invalid - Should Returns Errors")]
        public void ShouldIndicatesAccountTypeIsNotProperlyFulfilled()
        {
            var request = new AddReceiverRequest
            {
                AccountNumber = "26141",
                AccountType = string.Empty,
                BankType = "S",
                CompanyName = "",
                CustomerId = "123",
                EmailAdress = "email@mail.com",
                FirstName = "Raul",
                LastName = "Segal",
                NickName = "NickName",
                PhoneNumber = "+5512727222",
                ReceiverType = "D",
                RoutingNumber = "1",
                Ownership = "S"
            };

            var addReceiverValidator = new AddReceiverValidator();
            var responseValidator = addReceiverValidator.Validate(request);
            responseValidator.IsValid.Should().BeFalse();
        }

        [Fact(DisplayName = "AF1 – Required BankType Information Not Provided or Invalid - Should Returns Errors")]
        public void ShouldIndicatesBankTypeIsNotProperlyFulfilled()
        {
            var request = new AddReceiverRequest
            {
                AccountNumber = "26141",
                AccountType = "D",
                BankType = string.Empty,
                CompanyName = "",
                CustomerId = "123",
                EmailAdress = "email@mail.com",
                FirstName = "Raul",
                LastName = "Segal",
                NickName = "NickName",
                PhoneNumber = "+5512727222",
                ReceiverType = "D",
                RoutingNumber = "1",
                Ownership = "S"
            };

            var addReceiverValidator = new AddReceiverValidator();
            var responseValidator = addReceiverValidator.Validate(request);
            responseValidator.IsValid.Should().BeFalse();
        }

        [Fact(DisplayName = "AF1 – Required CustomerId Information Not Provided or Invalid - Should Returns Errors")]
        public void ShouldIndicatesCustomerIdIsNotProperlyFulfilled()
        {
            var request = new AddReceiverRequest
            {
                AccountNumber = "26141",
                AccountType = "D",
                BankType = "S",
                CompanyName = "",
                CustomerId = string.Empty,
                EmailAdress = "email@mail.com",
                FirstName = "Raul",
                LastName = "Segal",
                NickName = "NickName",
                PhoneNumber = "+5512727222",
                ReceiverType = "D",
                RoutingNumber = "1",
                Ownership = "S"
            };

            var addReceiverValidator = new AddReceiverValidator();
            var responseValidator = addReceiverValidator.Validate(request);
            responseValidator.IsValid.Should().BeFalse();
        }

        [Fact(DisplayName = "AF1 – Required EmailAdress Information Not Provided or Invalid - Should Returns Errors")]
        public void ShouldIndicatesEmailAdressIsNotProperlyFulfilled()
        {
            var request = new AddReceiverRequest
            {
                AccountNumber = "26141",
                AccountType = "D",
                BankType = "S",
                CompanyName = "",
                CustomerId = "123",
                EmailAdress = string.Empty,
                FirstName = "Raul",
                LastName = "Segal",
                NickName = "NickName",
                PhoneNumber = string.Empty,
                ReceiverType = "D",
                RoutingNumber = "1",
                Ownership = "S"
            };

            var addReceiverValidator = new AddReceiverValidator();
            var responseValidator = addReceiverValidator.Validate(request);
            responseValidator.IsValid.Should().BeFalse();
        }

        [Fact(DisplayName = "AF1 – Required FirstName Information Not Provided or Invalid - Should Returns Errors")]
        public void ShouldIndicatesFirstNameIsNotProperlyFulfilled()
        {
            var request = new AddReceiverRequest
            {
                AccountNumber = "26141",
                AccountType = "D",
                BankType = "S",
                CompanyName = "",
                CustomerId = "671",
                EmailAdress = "email@mail.com",
                FirstName = string.Empty,
                LastName = "Segal",
                NickName = "NickName",
                PhoneNumber = "+5512727222",
                ReceiverType = "D",
                RoutingNumber = "1",
                Ownership = "S"
            };

            var addReceiverValidator = new AddReceiverValidator();
            var responseValidator = addReceiverValidator.Validate(request);
            responseValidator.IsValid.Should().BeFalse();
        }

        [Fact(DisplayName = "AF1 – Required LastName Information Not Provided or Invalid - Should Returns Errors")]
        public void ShouldIndicatesLastNameIsNotProperlyFulfilled()
        {
            var request = new AddReceiverRequest
            {
                AccountNumber = "26141",
                AccountType = "D",
                BankType = "S",
                CompanyName = "",
                CustomerId = "761",
                EmailAdress = "email@mail.com",
                FirstName = "Raul",
                LastName = string.Empty,
                NickName = "NickName",
                PhoneNumber = "+5512727222",
                ReceiverType = "D",
                RoutingNumber = "1",
                Ownership = "S",

            };

            var addReceiverValidator = new AddReceiverValidator();
            var responseValidator = addReceiverValidator.Validate(request);
            responseValidator.IsValid.Should().BeFalse();
        }

        [Fact(DisplayName = "AF1 – Required PhoneNumber Information Not Provided or Invalid - Should Returns Errors")]
        public void ShouldIndicatesPhoneNumberIsNotProperlyFulfilled()
        {
            var request = new AddReceiverRequest
            {
                AccountNumber = "26141",
                AccountType = "D",
                BankType = "S",
                CompanyName = "",
                CustomerId = "718",
                EmailAdress = string.Empty,
                FirstName = "Raul",
                LastName = "Segal",
                NickName = "NickName",
                PhoneNumber = string.Empty,
                ReceiverType = "D",
                RoutingNumber = "1",
                Ownership = "S"
            };

            var addReceiverValidator = new AddReceiverValidator();
            var responseValidator = addReceiverValidator.Validate(request);
            responseValidator.IsValid.Should().BeFalse();
        }

        [Fact(DisplayName = "AF1 – Required RoutingNumber Information Not Provided or Invalid - Should Returns Errors")]
        public void ShouldIndicatesRoutingNumberIsNotProperlyFulfilled()
        {
            var request = new AddReceiverRequest
            {
                AccountNumber = "26141",
                AccountType = "D",
                BankType = "S",
                CompanyName = "",
                CustomerId = "123",
                EmailAdress = "email@mail.com",
                FirstName = "Raul",
                LastName = "Segal",
                NickName = "NickName",
                PhoneNumber = "+557175272",
                ReceiverType = "D",
                RoutingNumber = string.Empty,
                Ownership = "S"
            };

            var addReceiverValidator = new AddReceiverValidator();
            var responseValidator = addReceiverValidator.Validate(request);
            responseValidator.IsValid.Should().BeFalse();
        }

        [Fact(DisplayName = "AF2 – Account information already in use - Should Returns Errors")]
        public async void ShouldIndicatesAccountInformationNotFound()
        {
            var receiverReadRepository = new Mock<IReceiverReadRepository>();
            var receiverWriteRepository = new Mock<IReceiverWriteRepository>();
            var receivers = new List<Receiver>
            {
                new Receiver
                {
                    AccountNumber = "15171",
                    AccountType = "D"
                }
            };

            receiverReadRepository
                .Setup(r => r.FindExistent(It.IsAny<Receiver>())).Returns(receivers);

            receiverWriteRepository
                .Setup(r => r.Add(It.IsAny<Receiver>(), It.IsAny<CancellationToken>())).Verifiable();

            Services.AddSingleton(receiverReadRepository.Object);
            Services.AddSingleton(receiverWriteRepository.Object);

            Provider = Services.BuildServiceProvider();
            var mediator = GetInstance<IMediator>();
            var logger = new Mock<ILogger<ReceiversController>>();

            var receiversController = new ReceiversController(mediator, logger.Object);
            var request = new AddReceiverRequest
            {
                AccountNumber = "16516",
                AccountType = "D",
                BankType = "S",
                CompanyName = "",
                CustomerId = "123",
                EmailAdress = "email@mail.com",
                FirstName = "Raul",
                LastName = "Segal",
                NickName = "NickName",
                PhoneNumber = "+5512727222",
                ReceiverType = "D",
                RoutingNumber = "1"
            };
            var cancellationToken = new CancellationToken();

            await Assert.ThrowsAsync<ConflictException>(() => receiversController.Add(request, cancellationToken));
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
