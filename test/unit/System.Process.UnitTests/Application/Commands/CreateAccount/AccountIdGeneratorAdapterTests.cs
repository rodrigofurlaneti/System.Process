using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using System.Process.Application.Commands.CreateAccount;
using System.Process.Base.IntegrationTests;
using System.Process.Domain.ValueObjects;
using Xunit;

namespace System.Process.UnitTests.Application.Commands.CreateAccount
{
    public class AccountIdGeneratorAdapterTests
    {
        [Fact(DisplayName = "Account Id Generator Adapter Test")]
        public void AccountIdGeneratorAdapterTest()
        {
            var ProcessConfig = new Mock<IOptions<ProcessConfig>>();
            ProcessConfig.Setup(p => p.Value).Returns(ProcessIntegrationTestsConfiguration.ReadJson<ProcessConfig>("ProcessConfig.json"));
            var input = new CreateAccountParamsAdapter();
            var accountIdGeneratorAdapter = new AccountIdGeneratorAdapter(ProcessConfig.Object.Value);
            var result = accountIdGeneratorAdapter.Adapt(input);
            result.Should().NotBeNull();
        }
    }
}
