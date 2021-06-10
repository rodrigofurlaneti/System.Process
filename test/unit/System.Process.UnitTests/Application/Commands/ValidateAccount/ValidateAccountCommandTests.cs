using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Process.Application.Commands.ValidateAccount;
using System.Process.Domain.ValueObjects;
using System.Process.UnitTests.Common;
using System.Phoenix.Common.Exceptions;
using System.Proxy.Silverlake.Base.Exceptions;
using System.Proxy.Silverlake.Inquiry;
using System.Proxy.Silverlake.Inquiry.Common;
using System.Proxy.Silverlake.Inquiry.Messages.Request;
using System.Proxy.Silverlake.Inquiry.Messages.Response;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace System.Process.UnitTests.Application.Commands.ValidateAccount
{
    public class ValidateAccountCommandTests
    {
        [Fact(DisplayName = "Should Send Handle Validate Account")]
        public async Task ShouldSendHandleValidateAccountAsync()
        {
            var logger = new Mock<ILogger<ValidateAccountCommand>>();
            var ProcessConfigOptions = new Mock<IOptions<ProcessConfig>>();
            var ProcessConfig = ConvertJson.ReadJson<ProcessConfig>("ProcessConfig.json");
            ProcessConfigOptions.Setup(m => m.Value).Returns(() => ProcessConfig);
            var inquiryOperation = new Mock<IInquiryOperation>();

            var validateAccountCommand = new ValidateAccountCommand(logger.Object, inquiryOperation.Object, ProcessConfigOptions.Object);
            var request = new ValidateAccountRequest
            {
                AccountId = "16561",
                AccountType = "D"
            };
            var cancellationToken = new CancellationToken();

            var ProcessearchResponse = await Task.FromResult(new ProcessearchResponse
            {
                ProcessearchRecInfo = new List<ProcessearchRecInfo>
                {
                    new ProcessearchRecInfo
                    {
                        AccountId = new AccountId
                        {
                            AccountNumber = "16561",
                            AccountType = "D"
                        },
                        Amount = 123,
                        Processtatus = "1",
                        AvailableBalance = 12,
                        ProductCode = "string",
                        ProductDesc = "string",
                        ProcesstatusDesc = "Closed"
                    }
                }
            });
            inquiryOperation.Setup(x => x.ProcessearchAsync(It.IsAny<ProcessearchRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(ProcessearchResponse);

            await validateAccountCommand.Handle(request, cancellationToken);
        }


        [Fact(DisplayName = "Should Send Handle Validate Account Error")]
        public async Task ShouldSendHandleValidateAccountErrorAsync()
        {
            var logger = new Mock<ILogger<ValidateAccountCommand>>();
            var ProcessConfig = new Mock<IOptions<ProcessConfig>>();
            var inquiryOperation = new Mock<IInquiryOperation>();

            var validateAccountCommand = new ValidateAccountCommand(logger.Object, inquiryOperation.Object, ProcessConfig.Object);
            var request = new ValidateAccountRequest();
            var cancellationToken = new CancellationToken();

            inquiryOperation
                .Setup(t => t.AccountInquiryAsync(It.IsAny<AccountInquiryRequest>(), It.IsAny<CancellationToken>())).Returns(Task<AccountInquiryResponse>.FromResult(
                    new AccountInquiryResponse
                    {
                        StatusDep = "1"
                    }));

            await Assert.ThrowsAsync<UnprocessableEntityException>(() => validateAccountCommand.Handle(request, cancellationToken));
        }

        [Fact(DisplayName = "Should Send Handle Validate Account Service Error", Skip = "true")]
        public async Task ShouldSendHandleValidateProcesserviceErrorAsync()
        {
            var logger = new Mock<ILogger<ValidateAccountCommand>>();
            var ProcessConfig = new Mock<IOptions<ProcessConfig>>();
            var inquiryOperation = new Mock<IInquiryOperation>();

            var validateAccountCommand = new ValidateAccountCommand(logger.Object, inquiryOperation.Object,ProcessConfig.Object);
            var request = new ValidateAccountRequest
            {
                AccountId = "16561",
                AccountType = "D"
            };
            var cancellationToken = new CancellationToken();

            inquiryOperation
                .Setup(t => t.AccountInquiryAsync(It.IsAny<AccountInquiryRequest>(), It.IsAny<CancellationToken>())).ThrowsAsync(new SilverlakeException("error"));

            await Assert.ThrowsAsync<SilverlakeException>(() => validateAccountCommand.Handle(request, cancellationToken));
        }
    }
}
