using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Process.Api.Controllers.V1;
using System.Process.Application.Clients.Cards;
using System.Process.Application.Commands.ActivateCard;
using System.Process.CrossCutting.DependencyInjection;
using System.Process.Domain.Entities;
using System.Proxy.Salesforce;
using System.Proxy.Salesforce.GetToken.Messages;
using System.Proxy.Salesforce.UpdateAsset;
using System.Proxy.Salesforce.UpdateAsset.Messages;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace System.Process.IntegrationTests.Application.Commands.ActivateCard
{
    public class ActivateCardCommandTests
    {
        #region Properties

        private static ServiceCollection Services;
        private static ServiceProvider Provider;
        private static Mock<IHttpContextAccessor> HttpContextAccessor;

        private Mock<ILogger<ActivateCardCommand>> Logger { get; }
        private Mock<ICardService> CardService { get; }
        private CancellationToken CancellationToken { get; }
        private Mock<IUpdateAssetClient> UpdateAssetClient { get; }
        private Mock<IOptions<GetTokenParams>> ConfigSalesforce { get; }
        private Mock<Proxy.Salesforce.GetToken.IGetTokenClient> TokenClient { get; }

        #endregion

        #region Constructor

        public ActivateCardCommandTests()
        {
            Logger = new Mock<ILogger<ActivateCardCommand>>();
            CardService = new Mock<ICardService>();
            CancellationToken = new CancellationToken();
            UpdateAssetClient = new Mock<IUpdateAssetClient>();
            ConfigSalesforce = new Mock<IOptions<GetTokenParams>>();
            TokenClient = new Mock<Proxy.Salesforce.GetToken.IGetTokenClient>();

            Services = new ServiceCollection();
            Services.AddMediator();
            Services.AddScoped(typeof(ILoggerFactory), typeof(LoggerFactory));
            Services.AddScoped(typeof(ILogger<>), typeof(Logger<>));

            Provider = Services.BuildServiceProvider();
        }

        #endregion

        #region Tests

        [Trait("Integration", "Success")]
        [Fact(DisplayName = "ActivateCardCommand_Success")]
        public async void ShouldActivateCardSuccessfully()
        {
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

            CardService
                .Setup(r => r.FindByCardId(It.IsAny<string>(), It.IsAny<int>(), CancellationToken)).Returns(Task.FromResult(cards));
            CardService
                .Setup(r => r.HandleCardUpdate(cards[0], CancellationToken)).Returns(Task.FromResult(cards[0]));
            TokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(GetTokenBaseResult());
            UpdateAssetClient.Setup(x => x.UpdateAsset(It.IsAny<UpdateAssetParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetSalesforceResult());

            Services.AddSingleton(Logger.Object);
            Services.AddSingleton(CardService.Object);
            Services.AddSingleton(TokenClient.Object);
            Services.AddSingleton(UpdateAssetClient.Object);
            Services.AddSingleton(ConfigSalesforce.Object);
            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<CardsController>>();
            var mediator = GetInstance<IMediator>();
            var controller = new CardsController(mediator, logger.Object, CardService.Object);
            var request = new ActivateCardRequest
            {
                CardId = 1,
                CustomerId = "SP-340",
                ExpireDate = "2312",
                Pan = "3133"
            };

            var result = await controller.ActivateCard(request, new CancellationToken());

            Assert.IsType<OkObjectResult>(result);
        }

        [Trait("Integration", "Error")]
        [Fact(DisplayName = "ActivateCardCommand_Error - Handle should throw 'Card is already active' if card.CardStatus is 'Active'")]
        public async void ShouldThrowMessageIfCardAlreadyActive()
        {
            IList<Card> cards = new List<Card>
            {
                new Card
                {
                    CardStatus = "Active"
                }
            };
            CardService
                .Setup(r => r.FindByCardId(It.IsAny<string>(), It.IsAny<int>(), CancellationToken)).Returns(Task.FromResult(cards));
            TokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(GetTokenBaseResult());
            UpdateAssetClient.Setup(x => x.UpdateAsset(It.IsAny<UpdateAssetParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetSalesforceResult());

            Services.AddSingleton(Logger.Object);
            Services.AddSingleton(CardService.Object);
            Services.AddSingleton(TokenClient.Object);
            Services.AddSingleton(UpdateAssetClient.Object);
            Services.AddSingleton(ConfigSalesforce.Object);
            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<CardsController>>();
            var mediator = GetInstance<IMediator>();
            var controller = new CardsController(mediator, logger.Object, CardService.Object);
            var request = new ActivateCardRequest
            {
                CardId = 1,
                CustomerId = "SP-340",
                ExpireDate = "2312",
                Pan = "3133"
            };

            try
            {
                await controller.ActivateCard(request, new CancellationToken());
            }
            catch (Exception ex)
            {
                Assert.Equal("Card is already active", ex.Message);
            }
        }

        [Trait("Integration", "Error")]
        [Fact(DisplayName = "ActivateCardCommand_Error - Handle should throw 'The info provided does not match the info on a card' if 'request.CardId' is different from 'card.CardId'")]
        public async void ShouldThrowMessageIfCardIdDoesntMatchRequest()
        {
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
            CardService
                .Setup(r => r.FindByCardId(It.IsAny<string>(), It.IsAny<int>(), CancellationToken)).Returns(Task.FromResult(cards));
            TokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(GetTokenBaseResult());
            UpdateAssetClient.Setup(x => x.UpdateAsset(It.IsAny<UpdateAssetParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetSalesforceResult());

            Services.AddSingleton(Logger.Object);
            Services.AddSingleton(CardService.Object);
            Services.AddSingleton(TokenClient.Object);
            Services.AddSingleton(UpdateAssetClient.Object);
            Services.AddSingleton(ConfigSalesforce.Object);
            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<CardsController>>();
            var mediator = GetInstance<IMediator>();
            var controller = new CardsController(mediator, logger.Object, CardService.Object);
            var request = new ActivateCardRequest
            {
                CardId = 2,
                CustomerId = "SP-340",
                ExpireDate = "2312",
                Pan = "3133"
            };

            try
            {
                await controller.ActivateCard(request, new CancellationToken());
            }
            catch (Exception ex)
            {
                Assert.Equal("The info provided does not match the info on a card", ex.Message);
            }
        }

        [Trait("Integration", "Error")]
        [Fact(DisplayName = "ActivateCardCommand_Error - Handle should throw 'The info provided does not match the info on a card' if 'request.ExpireDate' is different from 'card.ExpirationDate'")]
        public async void ShouldThrowMessageIfExpirationDateDoesntMatchRequest()
        {
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
            CardService
                .Setup(r => r.FindByCardId(It.IsAny<string>(), It.IsAny<int>(), CancellationToken)).Returns(Task.FromResult(cards));
            TokenClient.Setup(x => x.GetToken(It.IsAny<GetTokenParams>(), It.IsAny<CancellationToken>())).Returns(GetTokenBaseResult());
            UpdateAssetClient.Setup(x => x.UpdateAsset(It.IsAny<UpdateAssetParams>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(GetSalesforceResult());


            Services.AddSingleton(Logger.Object);
            Services.AddSingleton(CardService.Object);
            Services.AddSingleton(TokenClient.Object);
            Services.AddSingleton(UpdateAssetClient.Object);
            Services.AddSingleton(ConfigSalesforce.Object);
            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<CardsController>>();
            var mediator = GetInstance<IMediator>();
            var controller = new CardsController(mediator, logger.Object, CardService.Object);
            var request = new ActivateCardRequest
            {
                CardId = 1,
                CustomerId = "SP-340",
                ExpireDate = "2313",
                Pan = "3133"
            };

            try
            {
                await controller.ActivateCard(request, new CancellationToken());
            }
            catch (Exception ex)
            {
                Assert.Equal("The info provided does not match the info on a card", ex.Message);
            }
        }

        [Trait("Integration", "Error")]
        [Fact(DisplayName = "ActivateCardCommand_Error - Handle should throw 'The info provided does not match the info on a card' if 'request.Pan' is different from 'card.LastFour'")]
        public async void ShouldThrowMessageIfLastFourDoesntMatchRequest()
        {
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
            CardService
                .Setup(r => r.FindByCardId(It.IsAny<string>(), It.IsAny<int>(), CancellationToken)).Returns(Task.FromResult(cards));

            Services.AddSingleton(Logger.Object);
            Services.AddSingleton(CardService.Object);
            Services.AddSingleton(TokenClient.Object);
            Services.AddSingleton(UpdateAssetClient.Object);
            Services.AddSingleton(ConfigSalesforce.Object);
            Provider = Services.BuildServiceProvider();

            var logger = new Mock<ILogger<CardsController>>();
            var mediator = GetInstance<IMediator>();
            var controller = new CardsController(mediator, logger.Object, CardService.Object);
            var request = new ActivateCardRequest
            {
                CardId = 1,
                CustomerId = "SP-340",
                ExpireDate = "2312",
                Pan = "3134"
            };

            try
            {
                await controller.ActivateCard(request, new CancellationToken());
            }
            catch (Exception ex)
            {
                Assert.Equal("The info provided does not match the info on a card", ex.Message);
            }
        }
        #endregion

        #region Methods

        public static T GetInstance<T>()
        {
            T result = Provider.GetRequiredService<T>();
            ControllerBase controllerBase = result as ControllerBase;
            if (controllerBase != null)
            {
                SetControllerContext(controllerBase);
            }
            Controller controller = result as Controller;
            if (controller != null)
            {
                SetControllerContext(controller);
            }
            return result;
        }

        private static void SetControllerContext(Controller controller)
        {
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = HttpContextAccessor.Object.HttpContext
            };
        }

        private static void SetControllerContext(ControllerBase controller)
        {
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = HttpContextAccessor.Object.HttpContext
            };
        }

        private Task<Proxy.Salesforce.Messages.BaseResult<GetTokenResult>> GetTokenBaseResult()
        {
            return Task.FromResult(new Proxy.Salesforce.Messages.BaseResult<GetTokenResult>
            {
                IsSuccess = true,
                Result = new GetTokenResult
                {
                    AccessToken = "test"
                }
            });
        }

        private Task<Proxy.Salesforce.Messages.BaseResult<SalesforceResult>> GetSalesforceResult()
        {
            return Task.FromResult(new Proxy.Salesforce.Messages.BaseResult<SalesforceResult>
            {
                IsSuccess = true
            });
        }

        #endregion
    }
}
