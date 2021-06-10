using FluentAssertions;
using System.Process.Application.Commands.CreateAccount;
using System.Process.Domain.Enums;
using System.Process.Domain.ValueObjects;
using System.Collections.Generic;
using Xunit;

namespace System.Process.UnitTests.Application.Commands.CreateAccount
{
    public class UpdateAssetAdapterTests
    {
        [Fact(DisplayName = "Account UpdateAsset Adapter Test")]
        public void UpdateAssetAdapterTest()
        {
            var input = new List<AccountInfo>
            {
                new AccountInfo
                {
                    AssetId = "test",
                    Origin = OriginAccount.E
                }
            };

            var updateAssetAdapter = new UpdateAssetAdapter();

            var result = updateAssetAdapter.Adapt(input);

            result.Should().NotBeNull();
        }
    }
}
