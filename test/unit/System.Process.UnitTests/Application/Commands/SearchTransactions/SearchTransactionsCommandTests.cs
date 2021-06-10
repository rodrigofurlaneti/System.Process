using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Process.Application.Commands.SearchTransactions;
using System.Process.Domain.Repositories;
using System.Process.UnitTests.Adapters;
using System.Process.UnitTests.Common;
using System.Phoenix.Common.Exceptions;
using System.Proxy.Fis.GetToken;
using System.Proxy.Fis.GetToken.Messages;
using System.Proxy.Fis.Messages;
using System.Proxy.Fis.SearchTransactions;
using System.Proxy.Fis.SearchTransactions.Messages;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace System.Process.UnitTests.Application.Commands.SearchTransactions
{
    public class SearchTransactionsCommandTests
    {
        #region Properties

        private Mock<ILogger<SearchTransactionsCommand>> Logger { get; }
        private Mock<IGetTokenClient> GetTokenClient { get; }
        private Mock<IOptions<GetTokenParams>> GetTokenParams { get; }
        private Mock<ISearchTransactionsClient> SearchTransactionsClient { get; }
        private Mock<ICardReadRepository> CardReadRepository { get; }

        #endregion

        #region Constructor

        public SearchTransactionsCommandTests()
        {
            Logger = new Mock<ILogger<SearchTransactionsCommand>>();
            GetTokenClient = new Mock<IGetTokenClient>();
            GetTokenParams = new Mock<IOptions<GetTokenParams>>();
            SearchTransactionsClient = new Mock<ISearchTransactionsClient>();
            CardReadRepository = new Mock<ICardReadRepository>();
        }

        #endregion

        #region Tests

        [Fact(DisplayName = "Should Send Handle Search Transaction")]
        public async Task ShouldSendHandleSearchTransactionsAsync()
        {
            var adapter = GetSearchTransactions();
            var response = new BaseResult<SearchTransactionsResult>
            {
                IsSuccess = true,
                Result = adapter.SuccessResult
            };

            CardReadRepository.Setup(x => x.Find(It.IsAny<int>())).Returns(adapter.SuccessRepositoryResponse);
            GetTokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(GetBaseTokenResult()));
            SearchTransactionsClient.Setup(x => x.SearchTransactionsAsync(It.IsAny<SearchTransactionsParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                                    .Returns(Task.FromResult(response));

            var SearchTransactionsCommand = new SearchTransactionsCommand(
                Logger.Object,
                GetTokenClient.Object,
                GetTokenParams.Object,
                SearchTransactionsClient.Object,
                CardReadRepository.Object);

            var cancellationToken = new CancellationToken(false);

            var request = adapter.SuccessRequest;

            var result = await SearchTransactionsCommand.Handle(request, cancellationToken);

            Assert.IsType<SearchTransactionsResponse>(result);
            Assert.NotNull(result);
        }

        [Fact(DisplayName = "Should throw Unprocessable Entity Exception")]
        public async Task ShouldSendUnprocessableEntityException()
        {
            var adapter = GetSearchTransactions();
            var response = new BaseResult<SearchTransactionsResult>
            {
                IsSuccess = false,
                Message = "{\"metadata\":{\"messages\":[{\"code\":\"10\",\"text\":\"Invalid card number\"}]}}"
            };

            CardReadRepository.Setup(x => x.Find(It.IsAny<int>())).Returns(adapter.SuccessRepositoryResponse);
            GetTokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(GetBaseTokenResult()));
            SearchTransactionsClient.Setup(x => x.SearchTransactionsAsync(It.IsAny<SearchTransactionsParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                                   .Returns(Task.FromResult(response));

            var SearchTransactionsCommand = new SearchTransactionsCommand(
                Logger.Object,
                GetTokenClient.Object,
                GetTokenParams.Object,
                SearchTransactionsClient.Object,
                CardReadRepository.Object);

            var cancellationToken = new CancellationToken(false);

            await Assert.ThrowsAsync<UnprocessableEntityException>(() => SearchTransactionsCommand.Handle(new ConsultCardsByidCardRequest(), cancellationToken));
        }

        #endregion

        #region Methods

        private SearchTransactionsJsonAdapter GetSearchTransactions()
        {
            return ConvertJson.ReadJson<SearchTransactionsJsonAdapter>("SearchTransactions.json");
        }

        private BaseResult<GetTokenResult> GetBaseTokenResult()
        {
            return new BaseResult<GetTokenResult>
            {
                IsSuccess = true,
                Result = new GetTokenResult
                {
                    AccessToken = "D923GDM9-2943-9385-6B98-HFJC8742X901"
                }
            };
        }

        #endregion
    }
}
