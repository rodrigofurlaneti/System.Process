using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Process.Application.Clients.Cards;
using System.Process.Application.Commands.ChangeCardPin;
using System.Process.Base.IntegrationTests;
using System.Process.Domain.Entities;
using System.Process.Domain.ValueObjects;
using System.Proxy.Fis.ActivateCard;
using System.Proxy.Fis.ActivateCard.Messages;
using System.Proxy.Fis.ChangeCardPin;
using System.Proxy.Fis.ChangeCardPin.Messages;
using System.Proxy.Fis.GetToken;
using System.Proxy.Fis.GetToken.Messages;
using System.Proxy.Fis.Messages;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace System.Process.UnitTests.Application.Commands.ActivateCard
{
    public class ChancePinCommandTests
    {
        [Fact(DisplayName = "Handle should succeed")]
        public async Task ShouldSucceed()
        {
            var logger = new Mock<ILogger<ChangeCardPinCommand>>();
            var cardService = new Mock<ICardService>();
            var getTokenClient = new Mock<IGetTokenClient>();
            var getTokenParams = new Mock<IOptions<GetTokenParams>>();
            var changeCardPinClient = new Mock<IChangeCardPinClient>();
            var activateCardClient = new Mock<IActivateCardClient>();
            var ProcessConfig = new Mock<IOptions<ProcessConfig>>();

            IList<Card> cards = new List<Card>
            {
                new Card
                {
                    CardId = 1,
                    CustomerId = "SP-340",
                    ExpirationDate = "2312",
                    LastFour = "3133",
                    CardType = "DR",
                    Validated = 1,
                    Pan = "1397587290620175"
                }
            };

            var cancellationToken = new CancellationToken();

            cardService
                .Setup(r => r.FindByCardId(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(cards));
            getTokenClient
                .Setup(r => r.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(GetBaseTokenResult()));
            changeCardPinClient
                .Setup(r => r.ChangeCardPinAsync(It.IsAny<ChangeCardPinParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(new BaseResult<ChangeCardPinResult>
                {
                    IsSuccess = true
                }));
            activateCardClient
                .Setup(r => r.ActivateCard(It.IsAny<ActivateCardParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(new BaseResult<ActivateCardResult> {
                    IsSuccess = true,
                    Result = new ActivateCardResult
                    {
                        Metadata = new Metadata
                        {
                            Messages = new List<Message>
                            {
                                new Message { Code = "00", Text = "Text" }
                            }
                        }
                    }
                }));
            cardService
                .Setup(r => r.HandleCardUpdate(It.IsAny<Card>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(cards[0]));
            ProcessConfig.Setup(p => p.Value).Returns(ProcessIntegrationTestsConfiguration.ReadJson<ProcessConfig>("ProcessConfig.json"));
            var changeCardPinCommand = new ChangeCardPinCommand(
                    logger.Object,
                    getTokenClient.Object,
                    getTokenParams.Object,
                    changeCardPinClient.Object,
                    activateCardClient.Object,
                    cardService.Object,
                    ProcessConfig.Object
                );

            var request = new ChangeCardPinRequest
            {
                CardId = 1,
                CustomerId = "SP-340",
                NewPin = "1234",
            };

            var result = await changeCardPinCommand.Handle(request, cancellationToken);

            Assert.Equal("00", result.Code);
        }

        [Fact(DisplayName = "Handle should throw 'No card was found' if no card was found")]
        public async Task ShouldThrowNoCardMessage()
        {
            var logger = new Mock<ILogger<ChangeCardPinCommand>>();
            var cardService = new Mock<ICardService>();
            var getTokenClient = new Mock<IGetTokenClient>();
            var getTokenParams = new Mock<IOptions<GetTokenParams>>();
            var changeCardPinClient = new Mock<IChangeCardPinClient>();
            var activateCardClient = new Mock<IActivateCardClient>();
            var ProcessConfig = new Mock<IOptions<ProcessConfig>>();

            IList<Card> cards = new List<Card>();

            var cancellationToken = new CancellationToken();

            cardService
                .Setup(r => r.FindByCardId(It.IsAny<string>(), It.IsAny<int>(), cancellationToken)).Returns(Task.FromResult(cards));
            ProcessConfig.Setup(p => p.Value).Returns(ProcessIntegrationTestsConfiguration.ReadJson<ProcessConfig>("ProcessConfig.json"));
            var changeCardPinCommand = new ChangeCardPinCommand(
                    logger.Object,
                    getTokenClient.Object,
                    getTokenParams.Object,
                    changeCardPinClient.Object,
                    activateCardClient.Object,
                    cardService.Object,
                    ProcessConfig.Object
                );

            var request = new ChangeCardPinRequest
            {
                CardId = 1,
                CustomerId = "SP-340",
                NewPin = "1234"
            };

            try
            {
                await changeCardPinCommand.Handle(request, cancellationToken);
            }
            catch (Exception ex)
            {
                Assert.Equal("No card was found", ex.Message);
            }
        }

        [Fact(DisplayName = "Handle should throw 'This resource needs Card Activation before Pin Creation' if card isn't validated")]
        public async Task ShouldThrowNeedValidationMessage()
        {
            var logger = new Mock<ILogger<ChangeCardPinCommand>>();
            var cardService = new Mock<ICardService>();
            var getTokenClient = new Mock<IGetTokenClient>();
            var getTokenParams = new Mock<IOptions<GetTokenParams>>();
            var changeCardPinClient = new Mock<IChangeCardPinClient>();
            var activateCardClient = new Mock<IActivateCardClient>();
            var ProcessConfig = new Mock<IOptions<ProcessConfig>>();

            IList<Card> cards = new List<Card>
            {
                new Card
                {
                    CardId = 1,
                    CustomerId = "SP-340",
                    ExpirationDate = "2312",
                    LastFour = "3133",
                    CardType = "DR",
                    Validated = 0
                }
            };

            var cancellationToken = new CancellationToken();

            cardService
                .Setup(r => r.FindByCardId(It.IsAny<string>(), It.IsAny<int>(), cancellationToken)).Returns(Task.FromResult(cards));
            ProcessConfig.Setup(p => p.Value).Returns(ProcessIntegrationTestsConfiguration.ReadJson<ProcessConfig>("ProcessConfig.json"));
            var changeCardPinCommand = new ChangeCardPinCommand(
                    logger.Object,
                    getTokenClient.Object,
                    getTokenParams.Object,
                    changeCardPinClient.Object,
                    activateCardClient.Object,
                    cardService.Object,
                    ProcessConfig.Object
                );

            var request = new ChangeCardPinRequest
            {
                CardId = 1,
                CustomerId = "SP-340",
                NewPin = "1234"
            };

            try
            {
                await changeCardPinCommand.Handle(request, cancellationToken);
            }
            catch (Exception ex)
            {
                Assert.Equal("This resource needs Card Activation before Pin Creation", ex.Message);
            }
        }

        #region Methods

        private BaseResult<GetTokenResult> GetBaseTokenResult()
        {
            return new BaseResult<GetTokenResult>
            {
                IsSuccess = true,
                Result = new GetTokenResult
                {
                    AccessToken = "AccessToken"
                }
            };
        }

        #endregion
    }
}
