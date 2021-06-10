using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Process.Application.Clients.Cards;
using System.Process.Application.Commands.ActivateCard;
using System.Process.Domain.Entities;
using System.Proxy.Salesforce.Messages;
using System.Proxy.Salesforce.GetToken;
using System.Proxy.Salesforce.GetToken.Messages;
using System.Proxy.Salesforce.UpdateAsset;
using System.Proxy.Salesforce.UpdateAsset.Messages;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using System.Proxy.Salesforce;

namespace System.Process.UnitTests.Application.Commands.ActivateCard
{
    public class ActivateCardCommandTests
    {
        [Fact(DisplayName = "Handle should return 'Code = 00' and 'Message = SUCCESS'")]
        public async Task ShouldReturnSuccessAsMessage()
        {
            var logger = new Mock<ILogger<ActivateCardCommand>>();
            var cardService = new Mock<ICardService>();
            var updateAssetClient = new Mock<IUpdateAssetClient>();
            var configSalesforce = new Mock<IOptions<GetTokenParams>>();
            var tokenClient = new Mock<IGetTokenClient>();

            IList<Card> cards = new List<Card>
            {
                new Card
                {
                    CardId = 1,
                    CustomerId = "SP-340",
                    ExpirationDate = "2312",
                    LastFour = "3133"
                }
            };

            var cancellationToken = new CancellationToken();
            cardService
                .Setup(r => r.FindByCardId(It.IsAny<string>(), It.IsAny<int>(), cancellationToken)).Returns(Task.FromResult(cards));
            cardService
                .Setup(r => r.HandleCardUpdate(cards[0], cancellationToken)).Returns(Task.FromResult(cards[0]));
            tokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(GetTokenBaseResult());
            updateAssetClient.Setup(x => x.UpdateAsset(It.IsAny<UpdateAssetParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetSalesforceResult());

            var activateCardCommand = new ActivateCardCommand(logger.Object, cardService.Object, tokenClient.Object, configSalesforce.Object, updateAssetClient.Object);

            var request = new ActivateCardRequest
            {
                CardId = 1,
                CustomerId = "SP-340",
                ExpireDate = "2312",
                Pan = "3133"
            };

            var handleResponse = await activateCardCommand.Handle(request, cancellationToken);

            Assert.Equal(handleResponse.Code, "00");
            Assert.Equal(handleResponse.Message, "SUCCESS");
        }

        [Fact(DisplayName = "Handle should update 'card.Validated' to 1 if request validation is OK")]
        public async Task ShouldUpdateCardValidatedTo1IfRequestValidationIdOk()
        {
            var logger = new Mock<ILogger<ActivateCardCommand>>();
            var cardService = new Mock<ICardService>();
            var updateAssetClient = new Mock<IUpdateAssetClient>();
            var configSalesforce = new Mock<IOptions<GetTokenParams>>();
            var tokenClient = new Mock<IGetTokenClient>();

            IList<Card> cards = new List<Card>
            {
                new Card
                {
                    CardId = 1,
                    CustomerId = "SP-340",
                    ExpirationDate = "2312",
                    LastFour = "3133"
                }
            };

            var cancellationToken = new CancellationToken();
            cardService
                .Setup(r => r.FindByCardId(It.IsAny<string>(), It.IsAny<int>(), cancellationToken)).Returns(Task.FromResult(cards));
            cardService
                .Setup(r => r.HandleCardUpdate(cards[0], cancellationToken)).Returns(Task.FromResult(cards[0]));
            tokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(GetTokenBaseResult());
            updateAssetClient.Setup(x => x.UpdateAsset(It.IsAny<UpdateAssetParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetSalesforceResult());

            var activateCardCommand = new ActivateCardCommand(logger.Object, cardService.Object, tokenClient.Object, configSalesforce.Object, updateAssetClient.Object);

            var request = new ActivateCardRequest
            {
                CardId = 1,
                CustomerId = "SP-340",
                ExpireDate = "2312",
                Pan = "3133"
            };

            await activateCardCommand.Handle(request, cancellationToken);

            Assert.Equal(cards[0].Validated, 1);
        }

        [Fact(DisplayName = "Handle should throw 'The info provided does not match the info on a card' if no card was found")]
        public async Task ShouldThrowMessageIfDoesntFindACard()
        {
            var logger = new Mock<ILogger<ActivateCardCommand>>();
            var cardService = new Mock<ICardService>();
            var cancellationToken = new CancellationToken();
            var updateAssetClient = new Mock<IUpdateAssetClient>();
            var configSalesforce = new Mock<IOptions<GetTokenParams>>();
            var tokenClient = new Mock<IGetTokenClient>();

            IList<Card> cards = new List<Card>();
            cardService
                .Setup(r => r.FindByCardId(It.IsAny<string>(), It.IsAny<int>(), cancellationToken)).Returns(Task.FromResult(cards));
            tokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(GetTokenBaseResult());
            updateAssetClient.Setup(x => x.UpdateAsset(It.IsAny<UpdateAssetParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetSalesforceResult());

            var activateCardCommand = new ActivateCardCommand(logger.Object, cardService.Object, tokenClient.Object, configSalesforce.Object, updateAssetClient.Object);
            var request = new ActivateCardRequest
            {
                CardId = 1,
                CustomerId = "SP-340",
                ExpireDate = "2312",
                Pan = "3133"
            };

            try
            {
                await activateCardCommand.Handle(request, cancellationToken);
            }
            catch (Exception ex)
            {
                Assert.Equal("The info provided does not match the info on a card", ex.Message);
            }
        }

        [Fact(DisplayName = "Handle should throw 'Card is already active' if card.CardStatus is 'Active'")]
        public async Task ShouldThrowMessageIfCardAlreadyActive()
        {
            var logger = new Mock<ILogger<ActivateCardCommand>>();
            var cardService = new Mock<ICardService>();
            var cancellationToken = new CancellationToken();
            var updateAssetClient = new Mock<IUpdateAssetClient>();
            var configSalesforce = new Mock<IOptions<GetTokenParams>>();
            var tokenClient = new Mock<IGetTokenClient>();

            IList<Card> cards = new List<Card>
            {
                new Card
                {
                    CardStatus = "Active"
                }
            };
            cardService
                .Setup(r => r.FindByCardId(It.IsAny<string>(), It.IsAny<int>(), cancellationToken)).Returns(Task.FromResult(cards));
            tokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(GetTokenBaseResult());
            updateAssetClient.Setup(x => x.UpdateAsset(It.IsAny<UpdateAssetParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetSalesforceResult());

            var activateCardCommand = new ActivateCardCommand(logger.Object, cardService.Object, tokenClient.Object, configSalesforce.Object, updateAssetClient.Object);

            var request = new ActivateCardRequest
            {
                CardId = 1,
                CustomerId = "SP-340",
                ExpireDate = "2312",
                Pan = "3133"
            };

            try
            {
                await activateCardCommand.Handle(request, cancellationToken);
            }
            catch (Exception ex)
            {
                Assert.Equal("Card is already active", ex.Message);
            }
        }

        [Fact(DisplayName = "Handle should throw 'The info provided does not match the info on a card' if 'request.CardId' is different from 'card.CardId'")]
        public async Task ShouldThrowMessageIfCardIdDoesntMatchRequest()
        {
            var logger = new Mock<ILogger<ActivateCardCommand>>();
            var cardService = new Mock<ICardService>();
            var cancellationToken = new CancellationToken();
            var updateAssetClient = new Mock<IUpdateAssetClient>();
            var configSalesforce = new Mock<IOptions<GetTokenParams>>();
            var tokenClient = new Mock<IGetTokenClient>();

            IList<Card> cards = new List<Card>
            {
                new Card
                {
                    CardId = 1,
                    CustomerId = "SP-340",
                    ExpirationDate = "2312",
                    LastFour = "3133"
                }
            };
            cardService
                .Setup(r => r.FindByCardId(It.IsAny<string>(), It.IsAny<int>(), cancellationToken)).Returns(Task.FromResult(cards));
            tokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(GetTokenBaseResult());
            updateAssetClient.Setup(x => x.UpdateAsset(It.IsAny<UpdateAssetParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetSalesforceResult());

            var activateCardCommand = new ActivateCardCommand(logger.Object, cardService.Object, tokenClient.Object, configSalesforce.Object, updateAssetClient.Object);
            var request = new ActivateCardRequest
            {
                CardId = 2,
                CustomerId = "SP-340",
                ExpireDate = "2312",
                Pan = "3133"
            };

            try
            {
                await activateCardCommand.Handle(request, cancellationToken);
            }
            catch (Exception ex)
            {
                Assert.Equal("The info provided does not match the info on a card", ex.Message);
            }
        }

        [Fact(DisplayName = "Handle should throw 'The info provided does not match the info on a card' if 'request.ExpireDate' is different from 'card.ExpirationDate'")]
        public async Task ShouldThrowMessageIfExpirationDateDoesntMatchRequest()
        {
            var logger = new Mock<ILogger<ActivateCardCommand>>();
            var cardService = new Mock<ICardService>();
            var cancellationToken = new CancellationToken();
            var updateAssetClient = new Mock<IUpdateAssetClient>();
            var configSalesforce = new Mock<IOptions<GetTokenParams>>();
            var tokenClient = new Mock<IGetTokenClient>();

            IList<Card> cards = new List<Card>
            {
                new Card
                {
                    CardId = 1,
                    CustomerId = "SP-340",
                    ExpirationDate = "2312",
                    LastFour = "3133"
                }
            };
            cardService
                .Setup(r => r.FindByCardId(It.IsAny<string>(), It.IsAny<int>(), cancellationToken)).Returns(Task.FromResult(cards));
            tokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(GetTokenBaseResult());
            updateAssetClient.Setup(x => x.UpdateAsset(It.IsAny<UpdateAssetParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetSalesforceResult());

            var activateCardCommand = new ActivateCardCommand(logger.Object, cardService.Object, tokenClient.Object, configSalesforce.Object, updateAssetClient.Object);

            var request = new ActivateCardRequest
            {
                CardId = 1,
                CustomerId = "SP-340",
                ExpireDate = "2313",
                Pan = "3133"
            };

            try
            {
                await activateCardCommand.Handle(request, cancellationToken);
            }
            catch (Exception ex)
            {
                Assert.Equal("The info provided does not match the info on a card", ex.Message);
            }
        }

        [Fact(DisplayName = "Handle should throw 'The info provided does not match the info on a card' if 'request.Pan' is different from 'card.LastFour'")]
        public async Task ShouldThrowMessageIfLastFourDoesntMatchRequest()
        {
            var logger = new Mock<ILogger<ActivateCardCommand>>();
            var cardService = new Mock<ICardService>();
            var cancellationToken = new CancellationToken();
            var updateAssetClient = new Mock<IUpdateAssetClient>();
            var configSalesforce = new Mock<IOptions<GetTokenParams>>();
            var tokenClient = new Mock<IGetTokenClient>();

            IList<Card> cards = new List<Card>
            {
                new Card
                {
                    CardId = 1,
                    CustomerId = "SP-340",
                    ExpirationDate = "2312",
                    LastFour = "3133"
                }
            };
            cardService
                .Setup(r => r.FindByCardId(It.IsAny<string>(), It.IsAny<int>(), cancellationToken)).Returns(Task.FromResult(cards));
            tokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(GetTokenBaseResult());
            updateAssetClient.Setup(x => x.UpdateAsset(It.IsAny<UpdateAssetParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetSalesforceResult());

            var activateCardCommand = new ActivateCardCommand(logger.Object, cardService.Object, tokenClient.Object, configSalesforce.Object, updateAssetClient.Object);
            var request = new ActivateCardRequest
            {
                CardId = 1,
                CustomerId = "SP-340",
                ExpireDate = "2312",
                Pan = "3134"
            };

            try
            {
                await activateCardCommand.Handle(request, cancellationToken);
            }
            catch (Exception ex)
            {
                Assert.Equal("The info provided does not match the info on a card", ex.Message);
            }
        }

        private Task<BaseResult<GetTokenResult>> GetTokenBaseResult()
        {
            return Task.FromResult(new BaseResult<GetTokenResult>
            {
                IsSuccess = true,
                Result = new GetTokenResult
                {
                    AccessToken = "test"
                }
            });
        }

        private Task<BaseResult<SalesforceResult>> GetSalesforceResult()
        {
            return Task.FromResult(new BaseResult<SalesforceResult>
            {
                IsSuccess = true
            });
        }
    }
}
