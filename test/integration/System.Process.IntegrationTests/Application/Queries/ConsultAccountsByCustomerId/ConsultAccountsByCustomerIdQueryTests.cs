using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Process.Api.Controllers.V1;
using System.Process.Application.Queries.ConsultProcessByCustomerId;
using System.Process.CrossCutting.DependencyInjection;
using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Process.Domain.ValueObjects;
using System.Phoenix.Common.Exceptions;
using System.Proxy.Silverlake.Inquiry;
using System.Proxy.Silverlake.Inquiry.Common;
using System.Proxy.Silverlake.Inquiry.Messages.Request;
using System.Proxy.Silverlake.Inquiry.Messages.Response;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace System.Process.IntegrationTests.Application.Queries.ConsultProcessByCustomerId
{
    public class ConsultProcessByCustomerIdQueryTests
    {
        #region Properties

        private static ServiceCollection Services;
        private static ServiceProvider Provider;
        private static Mock<IHttpContextAccessor> HttpContextAccessor;

        private Mock<ILogger<ConsultProcessByCustomerIdQuery>> Logger { get; }
        private Mock<IInquiryOperation> InquiryOperation { get; }
        private Mock<ICustomerReadRepository> CustomerReadRepository { get; }
        private Mock<IOptions<ProcessConfig>> ProcessConfig { get; set; }
        #endregion

        #region Constructor

        public ConsultProcessByCustomerIdQueryTests()
        {
            Logger = new Mock<ILogger<ConsultProcessByCustomerIdQuery>>();
            InquiryOperation = new Mock<IInquiryOperation>();
            CustomerReadRepository = new Mock<ICustomerReadRepository>();
            ProcessConfig = new Mock<IOptions<ProcessConfig>>();
            ProcessConfig.Setup(p => p.Value)
                .Returns(new ProcessConfig
                {
                    TransferTypeACH = "ACH"
                });

            Services = new ServiceCollection();
            Services.AddMediator();
            Services.AddScoped(typeof(ILoggerFactory), typeof(LoggerFactory));
            Services.AddScoped(typeof(ILogger<>), typeof(Logger<>));

            Provider = Services.BuildServiceProvider();
        }

        #endregion

        #region Tests

        [Trait("Integration", "Success")]
        [Fact(DisplayName = "ConsultCardsByCustomerIdQuery_Success")]
        public async void ShouldConsultCardByCustomerIdSuccessfully()
        {
            var response = GetCustomerReadRepositoryResponse();
            var ProcessearchResponse = GetProcessearchResponse();
            InquiryOperation.Setup(x => x.ProcessearchAsync(It.IsAny<ProcessearchRequest>(), It.IsAny<CancellationToken>())).Returns(ProcessearchResponse);
            CustomerReadRepository.Setup(x => x.FindBy(It.IsAny<string>())).Returns(response);

            Services.AddSingleton(Logger.Object);
            Services.AddSingleton(InquiryOperation.Object);
            Services.AddSingleton(CustomerReadRepository.Object);
            Services.AddSingleton(ProcessConfig.Object);
            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<ProcessController>>();
            var mediator = GetInstance<IMediator>();
            var controller = new ProcessController(mediator, logger.Object);
            var request = GetConsultProcessRequest();

            var result = await controller.ConsultProcess(request.ApplicationId, new CancellationToken());

            Assert.IsType<OkObjectResult>(result);

        }

        [Trait("Integration", "Error")]
        [Fact(DisplayName = "ConsultCardsByCustomerIdQuery_Error")]
        public async void ShouldNotConsultCardByCustomerId()
        {
            var response = GetCustomerReadRepositoryResponse();
            InquiryOperation.Setup(x => x.ProcessearchAsync(It.IsAny<ProcessearchRequest>(), It.IsAny<CancellationToken>())).Throws(new NotFoundException("Account Not Found"));
            CustomerReadRepository.Setup(x => x.FindBy(It.IsAny<string>())).Returns(response);

            Services.AddSingleton(Logger.Object);
            Services.AddSingleton(InquiryOperation.Object);
            Services.AddSingleton(CustomerReadRepository.Object);
            Services.AddSingleton(ProcessConfig.Object);
            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<ProcessController>>();
            var mediator = GetInstance<IMediator>();
            var controller = new ProcessController(mediator, logger.Object);
            var request = GetConsultProcessRequest();

            await Assert.ThrowsAsync<NotFoundException>(() => controller.ConsultProcess(request.ApplicationId, new CancellationToken()));

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

        private ConsultProcessByCustomerIdRequest GetConsultProcessRequest()
        {
            return new ConsultProcessByCustomerIdRequest("string");
        }

        private async Task<ProcessearchResponse> GetProcessearchResponse()
        {
            return await Task.FromResult(new ProcessearchResponse
            {
                ProcessearchRecInfo = new List<ProcessearchRecInfo>
                {
                    new ProcessearchRecInfo
                    {
                        AccountId = new AccountId
                        {
                            AccountNumber = "string",
                            AccountType = "string"
                        },
                        Amount = 123,
                        Processtatus = "2",
                        AvailableBalance = 12,
                        ProductCode = "string",
                        ProductDesc = "string",
                        ProcesstatusDesc = "Closed"
                    }
                }
            });
        }

        private Customer GetCustomerReadRepositoryResponse()
        {
            return new Customer
            {
                BusinessCif = "1255132",
                SalesforceId = "232",
                ApplicationId = "33344",
                Id = "ssgg135",
                IndustryType = "BSS"
            };
            #endregion
        }
    }
}
