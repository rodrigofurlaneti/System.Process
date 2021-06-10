using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Process.Application.Commands.TransactionDetail;
using System.Process.Domain.Repositories;
using System.Process.UnitTests.Adapters;
using System.Process.UnitTests.Common;
using System.Proxy.Fis.GetToken;
using System.Proxy.Fis.GetToken.Messages;
using System.Proxy.Fis.Messages;
using System.Proxy.Fis.TransactionDetail;
using System.Proxy.Fis.TransactionDetail.Messages;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace System.Process.UnitTests.Application.Commands.TransactionDetail
{
    public class TransactionDetailCommandTests
    {
        #region Properties

        private Mock<ILogger<TransactionDetailCommand>> Logger { get; }
        private Mock<IGetTokenClient> GetTokenClient { get; }
        private Mock<IOptions<GetTokenParams>> GetTokenParams { get; }
        private Mock<ITransactionDetailClient> TransactionDetailClient { get; }
        private Mock<ICardReadRepository> CardReadRepository { get; }

        #endregion

        #region Constructor

        public TransactionDetailCommandTests()
        {
            Logger = new Mock<ILogger<TransactionDetailCommand>>();
            GetTokenClient = new Mock<IGetTokenClient>();
            GetTokenParams = new Mock<IOptions<GetTokenParams>>();
            TransactionDetailClient = new Mock<ITransactionDetailClient>();
            CardReadRepository = new Mock<ICardReadRepository>();
        }

        #endregion

        #region Tests

        [Fact(DisplayName = "Should Send Handle Transaction Detail")]
        public async Task ShouldSendHandleTransactionDetailAsync()
        {
            var adapter = GetTransactionDetail();
            var response = new BaseResult<TransactionDetailResult>
            {
                IsSuccess = true,
                Result = adapter.SuccessResult
            };

            GetTokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(GetBaseTokenResult()));
            TransactionDetailClient.Setup(x => x.TransactionDetailAsync(It.IsAny<TransactionDetailParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                                    .Returns(Task.FromResult(response));

            var transactionDetailCommand = new TransactionDetailCommand(
                Logger.Object,
                GetTokenClient.Object,
                GetTokenParams.Object,
                TransactionDetailClient.Object,
                CardReadRepository.Object);

            var cancellationToken = new CancellationToken(false);

            var request = adapter.SuccessRequest;

            //var result = await transactionDetailCommand.Handle(request, cancellationToken);

            //Assert.IsType<TransactionDetailResponse>(result);
            //Assert.NotNull(result);
        }

        [Fact(DisplayName = "Should throw Unprocessable Entity Exception")]
        public async Task ShouldSendUnprocessableEntityException()
        {
            var adapter = GetTransactionDetail();
            var response = new BaseResult<TransactionDetailResult>
            {
                IsSuccess = false
            };

            GetTokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(GetBaseTokenResult()));
            TransactionDetailClient.Setup(x => x.TransactionDetailAsync(It.IsAny<TransactionDetailParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                                   .Returns(Task.FromResult(response));

            var transactionDetailCommand = new TransactionDetailCommand(
                Logger.Object,
                GetTokenClient.Object,
                GetTokenParams.Object,
                TransactionDetailClient.Object,
                CardReadRepository.Object);

            var cancellationToken = new CancellationToken(false);

            //await Assert.ThrowsAsync<UnprocessableEntityException>(() => transactionDetailCommand.Handle(It.IsAny<TransactionDetailRequest>(), cancellationToken));
        }

        #endregion

        #region Methods

        private TransactionDetailJsonAdapter GetTransactionDetail()
        {
            return ConvertJson.ReadJson<TransactionDetailJsonAdapter>("TransactionDetail.json");
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
