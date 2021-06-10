using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Process.Application.Clients.Cards;
using System.Process.Application.Commands.CardReplace;
using System.Process.Base.IntegrationTests;
using System.Process.Domain.Entities;
using System.Process.Domain.Repositories;
using System.Process.Domain.ValueObjects;
using System.Process.UnitTests.Adapters;
using System.Process.UnitTests.Common;
using System.Phoenix.Common.Exceptions;
using System.Proxy.Fis.CardReplace;
using System.Proxy.Fis.CardReplace.Messages;
using System.Proxy.Fis.CardReplace.Messages.Result;
using System.Proxy.Fis.GetCard;
using System.Proxy.Fis.GetCard.Messages;
using System.Proxy.Fis.GetToken;
using System.Proxy.Fis.GetToken.Messages;
using System.Proxy.Fis.Messages;
using System.Proxy.Fis.ReissueCard;
using System.Proxy.Fis.ReissueCard.Messages;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace System.Process.UnitTests.Application.Commands.CardReplace
{
    public class CardReplaceCommandTests
    {
        #region Properties

        private Mock<ILogger<CardReplaceCommand>> Logger { get; }
        private Mock<IGetTokenClient> GetTokenClient { get; }
        private Mock<IOptions<GetTokenParams>> GetTokenParams { get; }
        private Mock<ICardReplaceClient> CardReplaceClient { get; }
        private Mock<IReissueCardClient> ReissueCardClient { get; }
        private Mock<ICardReadRepository> CardReadRepository { get; }
        private Mock<ICardService> CardService { get; }
        private Mock<IOptions<ProcessConfig>> ProcessConfig { get; }
        private Mock<IGetCardClient> GetCardClient { get; }

        #endregion

        #region Constructor

        public CardReplaceCommandTests()
        {
            Logger = new Mock<ILogger<CardReplaceCommand>>();
            GetTokenClient = new Mock<IGetTokenClient>();
            GetTokenParams = new Mock<IOptions<GetTokenParams>>();
            CardReplaceClient = new Mock<ICardReplaceClient>();
            ReissueCardClient = new Mock<IReissueCardClient>();
            CardReadRepository = new Mock<ICardReadRepository>();
            CardService = new Mock<ICardService>();
            ProcessConfig = new Mock<IOptions<ProcessConfig>>();
            ProcessConfig.Setup(p => p.Value).Returns(ProcessIntegrationTestsConfiguration.ReadJson<ProcessConfig>("ProcessConfig.json"));
            GetCardClient = new Mock<IGetCardClient>();
        }

        #endregion

        #region Tests

        [Fact(DisplayName = "Should Send Handle Card Replace")]
        public async Task ShouldSendHandleCardReplaceAsync()
        {
            var adapterReplace = GetCardReplace();
            //var adapterReissue = GetReissueCard();

            //var responseReissue = new BaseResult<ReissueCardResult>
            //{
            //    IsSuccess = true,
            //    Result = adapterReissue.SuccessResult
            //};

            CardReadRepository.Setup(x => x.Find(It.IsAny<int>())).Returns(adapterReplace.SuccessRepositoryResponse);
            GetTokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(GetBaseTokenResult()));
            GetCardClient.Setup(x => x.GetCardAsync(It.IsAny<GetCardParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(GetCardResult()));
            ReissueCardClient.Setup(x => x.ReissueCardAsync(It.IsAny<ReissueCardParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(ReissueCardResult("00")));
            //ReissueCardClient.Setup(x => x.ReissueCard(It.IsAny<ReissueCardParams>(), It.IsAny<string>()))
            //.Returns(responseReissue);

            var CardReplaceCommand = new CardReplaceCommand(
                Logger.Object,
                GetTokenClient.Object,
                GetTokenParams.Object,
                ReissueCardClient.Object,
                CardReadRepository.Object,
                CardService.Object,
                ProcessConfig.Object,
                GetCardClient.Object
                );

            var cancellationToken = new CancellationToken(false);

            var request = adapterReplace.SuccessRequest;

            var result = await CardReplaceCommand.Handle(request, cancellationToken);

            Assert.IsType<CardReplaceResponse>(result);
            Assert.NotNull(result);
        }

        [Fact(DisplayName = "Should throw Not Found Exception")]
        public async Task ShouldThrowNotFoundException()
        {
            var adapter = GetCardReplace();

            CardReadRepository.Setup(x => x.Find(It.IsAny<int>())).Returns(new List<Card>());

            var CardReplaceCommand = new CardReplaceCommand(
                Logger.Object,
                GetTokenClient.Object,
                GetTokenParams.Object,
                ReissueCardClient.Object,
                CardReadRepository.Object,
                CardService.Object,
                ProcessConfig.Object,
                GetCardClient.Object);

            var cancellationToken = new CancellationToken(false);

            await Assert.ThrowsAsync<NotFoundException>(() => CardReplaceCommand.Handle(adapter.SuccessRequest, cancellationToken));
        }

        [Fact(DisplayName = "Should throw Unprocessable Entity Exception for not match card information")]
        public async Task ShouldThrowUnprocessableEntityExceptionForCard()
        {
            var adapter = GetCardReplace();
            adapter.SuccessRepositoryResponse[0].LastFour = "";

            CardReadRepository.Setup(x => x.Find(It.IsAny<int>())).Returns(adapter.SuccessRepositoryResponse);

            var CardReplaceCommand = new CardReplaceCommand(
                Logger.Object,
                GetTokenClient.Object,
                GetTokenParams.Object,
                ReissueCardClient.Object,
                CardReadRepository.Object,
                CardService.Object,
                ProcessConfig.Object,
                GetCardClient.Object);

            var cancellationToken = new CancellationToken(false);

            await Assert.ThrowsAsync<UnprocessableEntityException>(() => CardReplaceCommand.Handle(adapter.SuccessRequest, cancellationToken));
        }

        [Fact(DisplayName = "Should throw Unprocessable Entity Exception for error with External Provider")]
        public async Task ShouldThrowUnprocessableEntityException()
        {
            var adapter = GetCardReplace();

            CardReadRepository.Setup(x => x.Find(It.IsAny<int>())).Returns(adapter.SuccessRepositoryResponse);
            GetTokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(GetBaseTokenResult()));
            GetCardClient.Setup(x => x.GetCardAsync(It.IsAny<GetCardParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(GetCardResult()));
            ReissueCardClient.Setup(x => x.ReissueCardAsync(It.IsAny<ReissueCardParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(ReissueCardResult("171")));

            var CardReplaceCommand = new CardReplaceCommand(
                Logger.Object,
                GetTokenClient.Object,
                GetTokenParams.Object,
                ReissueCardClient.Object,
                CardReadRepository.Object,
                CardService.Object,
                ProcessConfig.Object,
                GetCardClient.Object);

            var cancellationToken = new CancellationToken(false);

            await Assert.ThrowsAsync<UnprocessableEntityException>(() => CardReplaceCommand.Handle(adapter.SuccessRequest, cancellationToken));
        }

        #endregion

        #region Methods

        private CardReplaceJsonAdapter GetCardReplace()
        {
            return ConvertJson.ReadJson<CardReplaceJsonAdapter>("CardReplace.json");
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

        private BaseResult<ReissueCardResult> ReissueCardResult()
        {
            var listMessage = new List<Message>();
            listMessage.Add(new Message()
            {
                Code = "00",
                Text = string.Empty
            });

            return new BaseResult<ReissueCardResult>()
            {
                IsSuccess = true,
                Result = new ReissueCardResult()
                {
                    Metadata = new Proxy.Fis.ReissueCard.Messages.Result.MetadataReissueCard()
                    {
                        Messages = listMessage
                    }
                }
            };
        }

        private BaseResult<GetCardResult> GetCardResult()
        {
            return ConvertJson.ReadJson<BaseResult<GetCardResult>>("GetCardResult.json");
        }

        private BaseResult<ReissueCardResult> ReissueCardResult(string statusCode)
        {
            var listMessage = new List<Message>();
            listMessage.Add(new Message()
            {
                Code = statusCode,
                Text = string.Empty
            });

            return new BaseResult<ReissueCardResult>()
            {
                IsSuccess = true,
                Result = new ReissueCardResult()
                {
                    Metadata = new Proxy.Fis.ReissueCard.Messages.Result.MetadataReissueCard()
                    {
                        Messages = listMessage
                    }
                }
            };
        }

        #endregion
    }
}