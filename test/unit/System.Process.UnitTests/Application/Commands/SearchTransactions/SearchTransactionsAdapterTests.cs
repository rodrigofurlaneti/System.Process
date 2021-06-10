using FluentAssertions;
using System.Process.Application.Commands.SearchTransactions;
using System;
using Xunit;

namespace System.Process.UnitTests.Application.Commands.SearchTransactions
{
    public class SearchTransactionsAdapterTests
    {
        #region Tests

        [Fact(DisplayName = "Should Return Search Transactions Params Adapter Successfully")]
        public void ShouldReturnSearchTransactionsParams()
        {
            var createRequest = GetSearchTransactionsRequest();
            var createAdapter = new SearchTransactionsAdapter();
            var result = createAdapter.Adapt(createRequest);

            result.Should().NotBeNull();
        }

        #endregion

        #region Methods

        private SearchTransactionsRequest GetSearchTransactionsRequest()
        {
            return new SearchTransactionsRequest
            {
                AccountId = "Test",
                Pan = "Test",
                MaxRows = 20,
                EndDate = DateTime.Now,
                StartDate = DateTime.Now
            };
        }

        #endregion
    }
}
