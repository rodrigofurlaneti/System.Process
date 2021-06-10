using Microsoft.Extensions.Logging;
using Moq;
using System.Process.Application.Queries.GetAccountHistory;
using System.Process.Domain.Repositories;
using System.Process.UnitTests.Adapters;
using System.Process.UnitTests.Common;
using System.Phoenix.Common.Exceptions;
using System.Proxy.Silverlake.Inquiry;
using System.Proxy.Silverlake.Inquiry.Messages.Request;
using System.Threading;
using Xunit;

namespace System.Process.UnitTests.Application.Queries.GetAccountHistory
{
    public class GetAccountHistoryQueryTests
    {
        [Fact(DisplayName = "Should Get Account History Successfully")]
        public async void ShouldGetAccountHistorySuccessfullyAsync()
        {
            var adapter = ConvertJson.ReadJson<GetAccountHistoryJsonAdapter>("GetAccountHistory.json");
            var inquiryOperation = new Mock<IInquiryOperation>();
            var logger = new Mock<ILogger<GetAccountHistoryQuery>>();
            var transactionRepo = new Mock<ITransactionReadRepository>();

            inquiryOperation.Setup(x => x.ProcessearchAsync(It.IsAny<ProcessearchRequest>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(adapter.AcctSrchSuccessResponse);

            inquiryOperation.Setup(x => x.AccountHistorySearchAsync(It.IsAny<AccountHistorySearchRequest>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(adapter.AcctHistSuccessResponse);

            transactionRepo.Setup(x => x.Find()).Returns(adapter.TransactionSuccessResponse);

            var consultProcessQuery = new GetAccountHistoryQuery(inquiryOperation.Object, logger.Object, transactionRepo.Object);

            var cancelToken = new CancellationToken();

            var result = await consultProcessQuery.Handle(adapter.SuccessRequest, cancelToken);

            Assert.NotNull(result);
        }

        [Fact(DisplayName = "Should Throw Not Found Account Id")]
        public async void ShouldThrowNotFoundAccountId()
        {
            var adapter = ConvertJson.ReadJson<GetAccountHistoryJsonAdapter>("GetAccountHistory.json");
            var inquiryOperation = new Mock<IInquiryOperation>();
            var logger = new Mock<ILogger<GetAccountHistoryQuery>>();
            var transactionRepo = new Mock<ITransactionReadRepository>();

            inquiryOperation.Setup(x => x.ProcessearchAsync(It.IsAny<ProcessearchRequest>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(adapter.AcctSrchErrorResponse);

            var consultProcessQuery = new GetAccountHistoryQuery(inquiryOperation.Object, logger.Object, transactionRepo.Object);

            var cancelToken = new CancellationToken();

            await Assert.ThrowsAsync<NotFoundException>(() => consultProcessQuery.Handle(adapter.SuccessRequest, cancelToken));
        }

        [Fact(DisplayName = "Should Throw Exception Account Status Not valid")]
        public async void ShouldThrowProcesstatusNotValid()
        {
            var adapter = ConvertJson.ReadJson<GetAccountHistoryJsonAdapter>("GetAccountHistory.json");
            var inquiryOperation = new Mock<IInquiryOperation>();
            var logger = new Mock<ILogger<GetAccountHistoryQuery>>();
            var transactionRepo = new Mock<ITransactionReadRepository>();

            inquiryOperation.Setup(x => x.ProcessearchAsync(It.IsAny<ProcessearchRequest>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(adapter.AcctSrchInvalidStatusResponse);

            var consultProcessQuery = new GetAccountHistoryQuery(inquiryOperation.Object, logger.Object, transactionRepo.Object);

            var cancelToken = new CancellationToken();

            var ex = await Assert.ThrowsAsync<UnprocessableEntityException>(() => consultProcessQuery.Handle(adapter.SuccessRequest, cancelToken));

            Assert.Contains("Account Status: Escheat not valid for querying transaction history.", ex.Message);
        }
    }
}
