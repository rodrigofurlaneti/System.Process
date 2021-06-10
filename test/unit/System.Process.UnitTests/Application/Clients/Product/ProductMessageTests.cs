using FluentAssertions;
using System.Process.Worker.Clients.Product;
using Xunit;

namespace System.Process.UnitTests.Application.Clients.Product
{
    public class ProductMessageTests
    {
        [Fact(DisplayName = "Product Message Test")]
        public void ProductMessageTest()
        {
            var productMessage = new ProductMessage
            {
                AccountType = "type",
            };

            productMessage.Should().NotBeEquivalentTo(new ProductMessage());
        }
    }
}
